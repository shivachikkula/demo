import { UpperCasePipe } from '@angular/common';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

import { LeaDashboardApiService } from '../../core/lea-dashboard-api.service';
import {
  CollectionDetail,
  CollectionSummary,
  LeaDashboard,
  SubmissionStatusChangedMessage,
} from '../../core/api.models';
import { SignalrService } from '../../core/signalr.service';

type CollectionsTab = 'active' | 'inactive';

@Component({
  selector: 'app-dashboard',
  imports: [MatButtonModule, MatIconModule, RouterLink, UpperCasePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  private readonly api = inject(LeaDashboardApiService);
  private readonly signalR = inject(SignalrService);

  readonly leaId = input<string>('dcps');
  readonly locked = input<boolean>(false);
  readonly collection = input<string | undefined>(undefined);

  protected readonly dashboard = signal<LeaDashboard | null>(null);
  protected readonly isDashboardLoading = signal(false);
  protected readonly dashboardError = signal<string | null>(null);

  protected readonly districtName = computed(() => this.dashboard()?.displayName ?? '');
  protected readonly activeCollections = computed<CollectionSummary[]>(() => this.dashboard()?.activeCollections ?? []);
  protected readonly inactiveCollections = computed<CollectionSummary[]>(
    () => this.dashboard()?.inactiveCollections ?? [],
  );

  protected readonly activeTab = signal<CollectionsTab>('active');
  protected readonly selectedCollectionId = signal<string>('');

  protected readonly selectedDetail = signal<CollectionDetail | null>(null);
  protected readonly isDetailLoading = signal(false);
  protected readonly detailError = signal<string | null>(null);

  protected readonly isUploading = signal(false);
  protected readonly uploadError = signal<string | null>(null);
  protected readonly isDragging = signal(false);

  private static readonly AUTO_ALIGN_THRESHOLD = 10;

  protected readonly visibleCollections = computed<CollectionSummary[]>(() =>
    this.activeTab() === 'active' ? this.activeCollections() : this.inactiveCollections(),
  );

  protected readonly useAutoAlignGrid = computed<boolean>(
    () => this.visibleCollections().length > Dashboard.AUTO_ALIGN_THRESHOLD,
  );

  constructor() {
    // Reload the whole dashboard whenever the LEA changes (Sponsor is always the same
    // LEA; the Admin drill-in page changes this per row selected), and keep this
    // component's SignalR group membership in sync so it only receives pushes for the
    // LEA currently on screen.
    effect((onCleanup) => {
      const leaId = this.leaId();
      this.activeTab.set('active');
      this.loadDashboard(leaId, { resetSelection: true });
      void this.signalR.joinLeaGroup(leaId);
      onCleanup(() => void this.signalR.leaveLeaGroup(leaId));
    });

    // Fetch a collection's detail whenever the selection (or LEA) changes.
    effect(() => {
      const leaId = this.leaId();
      const collectionId = this.selectedCollectionId();
      if (!leaId || !collectionId) {
        return;
      }
      this.loadDetail(leaId, collectionId);
    });

    // Jump to a specific collection when the `collection` query param changes (e.g. a
    // notification click) - tracked as its own effect since `collection` and `dashboard`
    // are both plain signals read synchronously here, unlike the async HTTP callback in
    // loadDashboard() below, which Angular can't treat as a reactive dependency.
    effect(() => {
      const requested = this.collection();
      const dashboardValue = this.dashboard();
      if (!requested || !dashboardValue) {
        return;
      }
      const allCollections = [...dashboardValue.activeCollections, ...dashboardValue.inactiveCollections];
      if (allCollections.some((c) => c.id === requested)) {
        this.selectedCollectionId.set(requested);
      }
    });

    // Live push: replaces polling for the submission's pass/fail result.
    void this.signalR.ensureStarted().then(() => {
      this.signalR.connection.on('SubmissionStatusChanged', (message: SubmissionStatusChangedMessage) => {
        if (message.leaId !== this.leaId()) {
          return;
        }
        // A card's status/record count may have changed too - refresh the summary list
        // without disturbing what the user currently has selected.
        this.loadDashboard(message.leaId, { resetSelection: false });
        if (message.collectionId === this.selectedCollectionId()) {
          this.selectedDetail.set(message.collectionDetail);
        }
      });
    });
  }

  private loadDashboard(leaId: string, options: { resetSelection: boolean }): void {
    this.isDashboardLoading.set(true);
    this.dashboardError.set(null);

    this.api.getDashboard(leaId).subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.isDashboardLoading.set(false);

        if (options.resetSelection) {
          const allCollections = [...dashboard.activeCollections, ...dashboard.inactiveCollections];
          const requested = this.collection();
          const requestedIsValid = requested && allCollections.some((c) => c.id === requested);
          const defaultId = dashboard.activeCollections[0]?.id ?? dashboard.inactiveCollections[0]?.id ?? '';
          this.selectedCollectionId.set(requestedIsValid ? requested : defaultId);
        }
      },
      error: (error: unknown) => {
        console.error('Failed to load LEA dashboard', error);
        this.isDashboardLoading.set(false);
        this.dashboardError.set('Unable to load this LEA. Is the API running?');
      },
    });
  }

  private loadDetail(leaId: string, collectionId: string): void {
    this.isDetailLoading.set(true);
    this.detailError.set(null);

    this.api.getCollectionDetail(leaId, collectionId).subscribe({
      next: (detail) => {
        this.selectedDetail.set(detail);
        this.isDetailLoading.set(false);
      },
      error: (error: unknown) => {
        console.error('Failed to load collection detail', error);
        this.isDetailLoading.set(false);
        this.detailError.set('Unable to load this collection.');
      },
    });
  }

  protected setTab(tab: CollectionsTab): void {
    this.activeTab.set(tab);
  }

  protected selectCollection(id: string): void {
    this.selectedCollectionId.set(id);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  protected onDragLeave(): void {
    this.isDragging.set(false);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.uploadFile(file);
    }
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.uploadFile(file);
    }
    input.value = '';
  }

  protected downloadUrl(submissionId: string): string {
    return this.api.downloadUrl(this.leaId(), this.selectedCollectionId(), submissionId);
  }

  private uploadFile(file: File): void {
    const leaId = this.leaId();
    const collectionId = this.selectedCollectionId();
    if (!collectionId) {
      return;
    }

    this.isUploading.set(true);
    this.uploadError.set(null);

    this.api.uploadSubmission(leaId, collectionId, file).subscribe({
      next: () => {
        this.isUploading.set(false);
        // Show the new "Processing" row immediately; the pass/fail result arrives over
        // SignalR a few seconds later instead of the client having to poll for it.
        this.loadDetail(leaId, collectionId);
        this.loadDashboard(leaId, { resetSelection: false });
      },
      error: (error: unknown) => {
        console.error('Upload failed', error);
        this.isUploading.set(false);
        this.uploadError.set('Upload failed. Please try again.');
      },
    });
  }

  protected formatNumber(value: number): string {
    return value.toLocaleString('en-US');
  }

  protected formatKb(bytes: number): string {
    return Math.round(bytes / 1024).toLocaleString('en-US');
  }
}

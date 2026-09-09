import { UpperCasePipe } from '@angular/common';
import { Component, computed, effect, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

import { CollectionDetail, CollectionSummary } from './dashboard.models';
import { LEA_DASHBOARD_DATA } from './lea-dashboard-data';

type CollectionsTab = 'active' | 'inactive';

@Component({
  selector: 'app-dashboard',
  imports: [MatButtonModule, MatIconModule, RouterLink, UpperCasePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  readonly leaId = input<string>('dcps');
  readonly locked = input<boolean>(false);
  readonly collection = input<string | undefined>(undefined);

  protected readonly leaRecord = computed(
    () => LEA_DASHBOARD_DATA[this.leaId()] ?? LEA_DASHBOARD_DATA['dcps'],
  );

  protected readonly districtName = computed(() => this.leaRecord().displayName);
  protected readonly activeCollections = computed(() => this.leaRecord().activeCollections);
  protected readonly inactiveCollections = computed(() => this.leaRecord().inactiveCollections);
  protected readonly collectionDetails = computed(() => this.leaRecord().collectionDetails);

  protected readonly activeTab = signal<CollectionsTab>('active');
  protected readonly selectedCollectionId = signal<string>('');

  constructor() {
    effect(() => {
      const record = this.leaRecord();
      const requested = this.collection();
      const defaultId = record.activeCollections[0]?.id ?? '';
      const targetId = requested && record.collectionDetails[requested] ? requested : defaultId;
      this.selectedCollectionId.set(targetId);
      this.activeTab.set('active');
    });
  }

  protected readonly selectedCollection = computed<CollectionSummary | undefined>(() =>
    this.activeCollections().find((collection) => collection.id === this.selectedCollectionId()),
  );

  protected readonly selectedDetail = computed<CollectionDetail | undefined>(
    () => this.collectionDetails()[this.selectedCollectionId()],
  );

  private static readonly AUTO_ALIGN_THRESHOLD = 10;

  protected readonly visibleCollections = computed<CollectionSummary[]>(() =>
    this.activeTab() === 'active' ? this.activeCollections() : this.inactiveCollections(),
  );

  protected readonly useAutoAlignGrid = computed<boolean>(
    () => this.visibleCollections().length > Dashboard.AUTO_ALIGN_THRESHOLD,
  );

  protected readonly isDragging = signal(false);

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
  }

  protected formatNumber(value: number): string {
    return value.toLocaleString('en-US');
  }
}

import { Component, effect, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

import { AdminApiService } from '../../core/admin-api.service';
import { LeaRosterEntry, LeaRosterSummary } from '../../core/api.models';

type SortKey = 'name' | 'overdueCount' | 'failedCount' | 'processingCount';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-admin-dashboard',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboard {
  private readonly api = inject(AdminApiService);

  protected readonly searchTerm = signal('');
  protected readonly sortKey = signal<SortKey>('name');
  protected readonly sortDir = signal<SortDir>('asc');

  protected readonly roster = signal<LeaRosterEntry[]>([]);
  protected readonly summary = signal<LeaRosterSummary | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal<string | null>(null);

  private searchDebounceHandle?: ReturnType<typeof setTimeout>;

  constructor() {
    effect(() => {
      // Reading these signals here is what makes the effect re-run on every search,
      // sort key, or sort direction change.
      const search = this.searchTerm();
      const sortBy = this.sortKey();
      const sortDir = this.sortDir();
      this.loadRoster(search, sortBy, sortDir);
    });
  }

  private loadRoster(search: string, sortBy: SortKey, sortDir: SortDir): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.api.getRoster(search || undefined, sortBy, sortDir).subscribe({
      next: (response) => {
        this.roster.set(response.leas);
        this.summary.set(response.summary);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        console.error('Failed to load LEA roster', error);
        this.isLoading.set(false);
        this.loadError.set('Unable to load LEAs. Is the API running?');
      },
    });
  }

  protected onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    clearTimeout(this.searchDebounceHandle);
    this.searchDebounceHandle = setTimeout(() => this.searchTerm.set(value), 300);
  }

  protected sortBy(key: SortKey): void {
    if (this.sortKey() === key) {
      this.sortDir.update((dir) => (dir === 'asc' ? 'desc' : 'asc'));
    } else {
      this.sortKey.set(key);
      this.sortDir.set('asc');
    }
  }

  protected sortIndicator(key: SortKey): string {
    if (this.sortKey() !== key) {
      return '';
    }
    return this.sortDir() === 'asc' ? '↑' : '↓';
  }
}

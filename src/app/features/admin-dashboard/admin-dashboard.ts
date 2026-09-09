import { Component, computed, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

import { LEA_ROSTER, LEA_ROSTER_SUMMARY, LeaRosterEntry } from '../../data/lea-directory';
import { LEA_DASHBOARD_DATA } from '../dashboard/lea-dashboard-data';

type SortKey = 'name' | 'overdueCount' | 'failedCount' | 'processingCount';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-admin-dashboard',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboard {
  protected readonly summary = LEA_ROSTER_SUMMARY;

  protected readonly searchTerm = signal('');
  protected readonly sortKey = signal<SortKey>('name');
  protected readonly sortDir = signal<SortDir>('asc');

  private collectionNamesFor(leaId: string): string[] {
    const record = LEA_DASHBOARD_DATA[leaId];
    if (!record) {
      return [];
    }
    return [...record.activeCollections, ...record.inactiveCollections].map((collection) => collection.name);
  }

  protected readonly filteredRoster = computed<LeaRosterEntry[]>(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const filtered = term
      ? LEA_ROSTER.filter(
          (lea) =>
            lea.name.toLowerCase().includes(term) ||
            this.collectionNamesFor(lea.id).some((name) => name.toLowerCase().includes(term)),
        )
      : LEA_ROSTER.slice();

    const key = this.sortKey();
    const dir = this.sortDir() === 'asc' ? 1 : -1;

    return filtered.sort((a, b) => {
      const aValue = a[key];
      const bValue = b[key];
      if (typeof aValue === 'string' && typeof bValue === 'string') {
        return aValue.localeCompare(bValue) * dir;
      }
      return ((aValue as number) - (bValue as number)) * dir;
    });
  });

  protected matchedCollections(lea: LeaRosterEntry): string[] {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term || lea.name.toLowerCase().includes(term)) {
      return [];
    }
    return this.collectionNamesFor(lea.id).filter((name) => name.toLowerCase().includes(term));
  }

  protected onSearchInput(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
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

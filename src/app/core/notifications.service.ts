import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { CollectionNotification } from '../features/dashboard/dashboard.models';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly router = inject(Router);

  readonly notifications = signal<CollectionNotification[]>([
    {
      id: 'n1',
      severity: 'info',
      title: 'CLSD End of Year (EOY) window is open',
      message:
        'The CLSD End of Year (EOY) data collection window is open Jul 28 – Aug 18, 2026. Please upload data as soon as possible after fixing all errors. Questions: Clara.Smith@dc.gov.',
      timestamp: 'Jul 28, 2026',
      read: false,
    },
    {
      id: 'n2',
      severity: 'error',
      title: 'CLSD-LEA upload failed',
      message: 'The last submission for CLSD-LEA had 9,134 validation errors. Review and resubmit before the due date.',
      timestamp: 'Aug 20, 2026 · 2:47 PM',
      read: false,
      collectionId: 'clsd-lea',
    },
    {
      id: 'n3',
      severity: 'warning',
      title: 'Course collection due soon',
      message: 'The Course collection is due Aug 19, 2026 — 3 days remaining.',
      timestamp: 'Aug 16, 2026',
      read: false,
      collectionId: 'course',
    },
    {
      id: 'n4',
      severity: 'info',
      title: 'Discipline collection passed validation',
      message: 'DCPS_Enrollment_Fall2026_v3.xlsx passed with 4,822 of 4,908 records clean.',
      timestamp: 'Aug 10, 2026',
      read: true,
      collectionId: 'discipline',
    },
  ]);

  readonly unreadCount = computed(() => this.notifications().filter((notification) => !notification.read).length);

  readonly isOpen = signal(false);

  toggle(): void {
    this.isOpen.update((open) => !open);
  }

  close(): void {
    this.isOpen.set(false);
  }

  markAllRead(): void {
    this.notifications.update((items) => items.map((item) => ({ ...item, read: true })));
  }

  dismiss(id: string, event: Event): void {
    event.stopPropagation();
    this.notifications.update((items) => items.filter((item) => item.id !== id));
  }

  open(notification: CollectionNotification): void {
    this.notifications.update((items) =>
      items.map((item) => (item.id === notification.id ? { ...item, read: true } : item)),
    );
    this.close();
    if (notification.collectionId) {
      this.router.navigate(['/sponsor'], { queryParams: { collection: notification.collectionId } });
    }
  }
}

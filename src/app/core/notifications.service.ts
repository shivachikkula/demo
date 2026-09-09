import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { environment } from '../../environments/environment';
import { CollectionNotification } from './api.models';
import { SignalrService } from './signalr.service';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly signalR = inject(SignalrService);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/notifications`;

  readonly notifications = signal<CollectionNotification[]>([]);
  readonly unreadCount = computed(() => this.notifications().filter((notification) => !notification.read).length);
  readonly isOpen = signal(false);

  constructor() {
    this.refresh();

    // Live-push: a new notification (e.g. a submission finishing processing) lands here
    // the moment the backend's background worker creates it - no polling required.
    void this.signalR.ensureStarted().then(() => {
      this.signalR.connection.on('NotificationCreated', (notification: CollectionNotification) => {
        this.notifications.update((items) => [notification, ...items]);
      });
    });
  }

  private refresh(): void {
    this.http.get<CollectionNotification[]>(this.baseUrl).subscribe({
      next: (items) => this.notifications.set(items),
      error: (error: unknown) => console.error('Failed to load notifications', error),
    });
  }

  toggle(): void {
    this.isOpen.update((open) => !open);
  }

  close(): void {
    this.isOpen.set(false);
  }

  markAllRead(): void {
    this.http.post(`${this.baseUrl}/read-all`, {}).subscribe({
      next: () => this.notifications.update((items) => items.map((item) => ({ ...item, read: true }))),
      error: (error: unknown) => console.error('Failed to mark all notifications as read', error),
    });
  }

  dismiss(id: string, event: Event): void {
    event.stopPropagation();
    this.http.delete(`${this.baseUrl}/${id}`).subscribe({
      next: () => this.notifications.update((items) => items.filter((item) => item.id !== id)),
      error: (error: unknown) => console.error('Failed to dismiss notification', error),
    });
  }

  open(notification: CollectionNotification): void {
    this.http.post(`${this.baseUrl}/${notification.id}/read`, {}).subscribe({
      error: (error: unknown) => console.error('Failed to mark notification as read', error),
    });
    this.notifications.update((items) =>
      items.map((item) => (item.id === notification.id ? { ...item, read: true } : item)),
    );
    this.close();
    if (notification.collectionId) {
      this.router.navigate(['/sponsor'], { queryParams: { collection: notification.collectionId } });
    }
  }
}

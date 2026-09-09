import { Component, computed, inject, signal } from '@angular/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';

import { NotificationsService } from './core/notifications.service';

type AppView = 'sponsor' | 'admin';

@Component({
  selector: 'app-root',
  imports: [MatBadgeModule, MatButtonModule, MatIconModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly notificationsService = inject(NotificationsService);
  private readonly router = inject(Router);

  private readonly currentUrl = signal(this.router.url);

  protected readonly activeView = computed<AppView>(() =>
    this.currentUrl().startsWith('/admin') ? 'admin' : 'sponsor',
  );

  protected readonly viewMenuOpen = signal(false);

  constructor() {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.currentUrl.set(event.urlAfterRedirects);
      }
    });
  }

  protected toggleViewMenu(): void {
    this.viewMenuOpen.update((open) => !open);
  }

  protected closeViewMenu(): void {
    this.viewMenuOpen.set(false);
  }

  protected switchView(view: AppView): void {
    this.closeViewMenu();
    this.router.navigateByUrl(view === 'admin' ? '/admin' : '/sponsor');
  }
}

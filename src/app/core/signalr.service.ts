import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

import { environment } from '../../environments/environment';

/**
 * Thin wrapper around the FileProcessingHub connection. This is what replaces the old
 * "poll every 5 minutes" behavior: consumers join a LEA's group while its dashboard is
 * open and get pushed a SubmissionStatusChanged event the moment the backend's
 * background worker finishes validating a file, plus a NotificationCreated event for
 * the bell menu.
 */
@Injectable({ providedIn: 'root' })
export class SignalrService {
  readonly connection = new signalR.HubConnectionBuilder()
    .withUrl(`${environment.apiBaseUrl}/hubs/file-processing`)
    .withAutomaticReconnect()
    .build();

  private startPromise: Promise<void> | null = null;

  ensureStarted(): Promise<void> {
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      return Promise.resolve();
    }

    if (!this.startPromise) {
      this.startPromise = this.connection.start().catch((error: unknown) => {
        console.error('SignalR connection failed to start', error);
        this.startPromise = null;
      });
    }

    return this.startPromise;
  }

  async joinLeaGroup(leaId: string): Promise<void> {
    await this.ensureStarted();
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinLeaGroup', leaId).catch((error: unknown) => console.error(error));
    }
  }

  async leaveLeaGroup(leaId: string): Promise<void> {
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveLeaGroup', leaId).catch((error: unknown) => console.error(error));
    }
  }
}

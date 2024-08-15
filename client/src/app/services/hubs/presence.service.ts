import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { LoggedInUser } from '../../models/logged-in-user.model';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  private _hubUrl = environment.hubUrl;
  private _hubConnection: HubConnection | undefined;
  private _matSnack = inject(MatSnackBar);
  private _isConnectionOnClose = false;
  private _isConnectionStopped = false;
  onlineUsersSig = signal<string[]>([]);

  createHubConnection(loggedInUser: LoggedInUser): void {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl + 'presence', { // 'presence' has to match the route set in Program.cs 
        accessTokenFactory: () => loggedInUser.token
      })
      // .withAutomaticReconnect() // To make onclose() work. Also it only tries 4 times which is not enough.
      .build();

    this.handleConnection();
  }

  private handleConnection(): void {
    this.startConnection();

    if (this._hubConnection) {
      this._hubConnection.onclose(() => {
        if (!this._isConnectionStopped) {
          this._isConnectionOnClose = true;
          this._matSnack.open('You are offline. Check your internet connection.', 'Close', { horizontalPosition: 'center', verticalPosition: 'top' });
          this.startConnection();
        }
      });
    }
  }

  private async startConnection(): Promise<void> {
    this._isConnectionStopped = false;

    await this._hubConnection?.start()
      .then(() => {
        console.log('PresenceHub connection started.');
        if (this._isConnectionOnClose && !this._isConnectionStopped) {
          this._isConnectionOnClose = false;
          this._matSnack.open('You are back online.', 'Close', { horizontalPosition: 'center', verticalPosition: 'bottom', duration: 7000 });
        }
      })
      .catch(() => {
        console.error('Failed start connection. Retrying every 2 seconds.');
        setTimeout(() => this.startConnection(), 2000);
      });
  }

  async stopHubConnection(): Promise<void> {
    this._isConnectionStopped = true;
    await this._hubConnection?.stop().catch(err => console.log(err));
  }
}

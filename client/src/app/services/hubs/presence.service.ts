import {inject, Injectable, signal} from '@angular/core';
import {environment} from '../../../environments/environment';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {MatSnackBar} from '@angular/material/snack-bar';
import {OnlineUser} from '../../models/online-users.model';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  onlineUsersSig = signal<OnlineUser[]>([]);
  private _hubUrl = environment.hubUrl;
  private _hubConnection: HubConnection | undefined;
  private _matSnack = inject(MatSnackBar);
  private _isConnectionOnClose = false;
  private _isConnectionStopped = false;
  private readonly _GetOnlineUsers = "GetOnlineUsers";

  createHubConnection(): void {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl + 'presence', { // 'presence' has to match the route set in Program.cs
        withCredentials: true
      })
      // .withAutomaticReconnect() // To make onclose() work. Also it only tries 4 times which is not enough.
      .build();

    this.handleConnection();
    this.getOnlineUsers();
  }

  async stopHubConnection(): Promise<void> {
    this._isConnectionStopped = true;
    await this._hubConnection?.stop().catch(err => console.log(err));
  }

  private getOnlineUsers(): void {
    this._hubConnection?.on(this._GetOnlineUsers, onlineUsers => {
      if (onlineUsers)
        this.onlineUsersSig.set(onlineUsers);
      else
        this._matSnack.open('Checking user status failed. Refresh the page or login again.', 'Close', {
          horizontalPosition: 'center',
          verticalPosition: 'top'
        });
    })
  }

  private handleConnection(): void {
    this.startConnection();

    if (this._hubConnection) {
      this._hubConnection.onclose(() => {
        if (!this._isConnectionStopped) {
          this._isConnectionOnClose = true;
          this._matSnack.open('You are offline. Check your internet connection.', 'Close', {
            horizontalPosition: 'center',
            verticalPosition: 'top'
          });
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
          this._matSnack.open('You are back online.', 'Close', {
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            duration: 7000
          });
        }
      })
      .catch(() => {
        console.error('Failed start connection. Retrying every 1 seconds.');
        setTimeout(() => this.startConnection(), 1000);
      });
  }
}

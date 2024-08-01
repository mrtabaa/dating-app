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
  private readonly _CheckUserIsOnline = "CheckUserIsOnline";
  private readonly _CheckUserIsOffline = "CheckUserIsOffline";
  private readonly _GetOnlineUsers = "GetOnlineUsers";
  onlineUsersSig = signal<string[]>([]);

  createHubConnection(loggedInUser: LoggedInUser): void {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl(this._hubUrl + 'presence', { // 'presence' has to match the route set in Program.cs 
        accessTokenFactory: () => loggedInUser.token
      })
      .withAutomaticReconnect()
      .build();

    this._hubConnection.start().catch(err => console.log(err));

    this._hubConnection.on(this._CheckUserIsOnline, username => {
      this._matSnack.open(username + ' is online.', 'Close', { duration: 7000, horizontalPosition: 'center', verticalPosition: 'bottom' });
    });

    this._hubConnection.on(this._CheckUserIsOffline, username => {
      this._matSnack.open(username + ' went offline.', 'Close', { duration: 7000, horizontalPosition: 'center', verticalPosition: 'top' });
    });

    this._hubConnection.on(this._GetOnlineUsers, onlineUsers => {
      this.onlineUsersSig.set(onlineUsers);
    })
  }

  stopHubConnection(): void {
    this._hubConnection?.stop().catch(err => console.log(err));
  }
}

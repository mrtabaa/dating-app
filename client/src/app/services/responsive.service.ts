import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ResponsiveService {
  isMobileSig = signal<boolean>(false);
  isWelcomeCompSig = signal<boolean>(true);
}

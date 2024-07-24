import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CommonService {
  isPreventingLeavingPage: boolean = false;
  isMessageCompSig = signal<boolean>(false);
  isCreatingMessageSig = signal<boolean>(false);
  isNavMobileBrandClickedSig = signal<boolean>(false);
}

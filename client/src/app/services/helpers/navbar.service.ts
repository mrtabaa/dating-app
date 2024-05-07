import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NavbarService {
  showNavbarSig = signal<boolean>(true);
}

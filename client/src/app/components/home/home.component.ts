import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { NavbarService } from '../../services/helpers/navbar.service';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { LoginRegisterComponent } from '../account/login-register/login-register.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    LoginRegisterComponent,
    NgOptimizedImage, MatIconModule,
    MatButtonModule, MatTabsModule, RouterLink, RouterLinkActive
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  navbarService = inject(NavbarService);
  router = inject(Router);

  ngOnInit(): void {
    this.navbarService.showNavbarSig.set(false);
  }
  ngOnDestroy(): void {
    this.navbarService.showNavbarSig.set(true);
  }
}

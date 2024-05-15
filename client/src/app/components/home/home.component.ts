import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { LoginComponent } from '../account/login/login.component';
import { NgOptimizedImage } from '@angular/common';
import { NavbarService } from '../../services/helpers/navbar.service';
import { MatIconModule } from '@angular/material/icon';
import { LoginDemoComponent } from '../demo/login-demo/login-demo.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    LoginComponent, LoginDemoComponent,
    RouterLink, NgOptimizedImage, MatIconModule,
    MatButtonModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  navbarService = inject(NavbarService);
  router = inject(Router);
  // loginDemo: boolean;

  ngOnInit(): void {
    this.navbarService.showNavbarSig.set(false);
  }
  ngOnDestroy(): void {
    this.navbarService.showNavbarSig.set(true);
  }
}

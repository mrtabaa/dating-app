import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
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
export class HomeComponent {
  router = inject(Router);
}

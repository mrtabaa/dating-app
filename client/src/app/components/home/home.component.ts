import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { LoginRegisterComponent } from '../account/login-register/login-register.component';
import { WelcomeComponent } from './welcome/welcome.component';
import { FooterComponent } from '../footer/footer.component';
import { ResponsiveService } from '../../services/responsive.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    LoginRegisterComponent, WelcomeComponent, FooterComponent,
    NgOptimizedImage, MatIconModule,
    MatButtonModule, MatTabsModule, RouterLink, RouterLinkActive
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  isMobileSig = inject(ResponsiveService).isMobileSig;
  isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;
}

import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ResponsiveService } from '../../../services/responsive.service';

@Component({
  selector: 'app-welcome',
  standalone: true,
  imports: [
    CommonModule, RouterLink, RouterLinkActive, NgOptimizedImage,
    MatButtonModule, MatIconModule, MatStepperModule
  ],
  templateUrl: './welcome.component.html',
  styleUrl: './welcome.component.scss'
})
export class WelcomeComponent {
  isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;

  toggleIsGetStartedClicked(isWelcome: boolean): void {
    this.isWelcomeCompSig.set(isWelcome);
  }
}

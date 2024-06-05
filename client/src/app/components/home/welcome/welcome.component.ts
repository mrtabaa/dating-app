import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { RouterLink, RouterLinkActive } from '@angular/router';

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
  @Output() isLogingRegisterOut = new EventEmitter(false);

  toggleIsLogginRegister(): void {
    this.isLogingRegisterOut.emit(true);
  }
}

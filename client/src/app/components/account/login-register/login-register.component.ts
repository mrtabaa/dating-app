import { Component } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LoginComponent } from '../login/login.component';
import { RegisterComponent } from '../register/register.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login-register',
  standalone: true,
  imports: [
    CommonModule, RouterLink, RouterLinkActive,
    LoginComponent, RegisterComponent,
    MatTabsModule
  ],
  templateUrl: './login-register.component.html',
  styleUrl: './login-register.component.scss'
})
export class LoginRegisterComponent {
  isLoginShown = true;
  isRegisterShown = false;

  toggleIsLoginShown(): void {
    this.isLoginShown = !this.isLoginShown;
    this.isRegisterShown = !this.isRegisterShown;
  }
}

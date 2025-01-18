import {Component, inject} from '@angular/core';
import {MatButtonModule} from '@angular/material/button';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {MatIconModule} from '@angular/material/icon';
import {MatTabsModule} from '@angular/material/tabs';
import {LoginRegisterComponent} from '../account/login-register/login-register.component';
import {WelcomeComponent} from './welcome/welcome.component';
import {ResponsiveService} from '../../services/responsive.service';
import {CommonService} from "../../services/common.service";
import {VerifyComponent} from "../account/verify/verify.component";
import {RequestResetPasswordComponent} from "../account/request-reset-password/request-reset-password.component";

@Component({
  selector: 'app-home',
  imports: [
    CommonModule,
    LoginRegisterComponent, WelcomeComponent, VerifyComponent,
    NgOptimizedImage, MatIconModule,
    MatButtonModule, MatTabsModule, RequestResetPasswordComponent
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  isMobileSig = inject(ResponsiveService).isMobileSig;
  isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;
  isVerifyingAccount = inject(CommonService).isVerifyingAccountSig;
  isResettingPasswordSig = inject(CommonService).isResettingPasswordSig;
}

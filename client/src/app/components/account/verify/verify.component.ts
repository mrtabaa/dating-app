import {Component, inject, OnDestroy} from '@angular/core';
import {AccountService} from "../../../services/account.service";
import {FormBuilder, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {Subscription, take} from "rxjs";
import {Verify} from '../../../models/account/verify.model';
import {ResponsiveService} from "../../../services/responsive.service";
import {MatFormFieldModule} from "@angular/material/form-field";
import {AutofocusDirective} from "../../../directives/autofocus.directive";
import {MatInputModule} from "@angular/material/input";
import {MatButtonModule} from "@angular/material/button";
import {MatIconModule} from "@angular/material/icon";
import {RecaptchaV3Module, ReCaptchaV3Service} from "ng-recaptcha";
import {MatSlideToggle} from "@angular/material/slide-toggle";
import {MatProgressSpinner} from "@angular/material/progress-spinner";
import {CommonService} from "../../../services/common.service";
import {RecoveryValidationRequest} from "../../../models/account/recovery-validation-request.model";

@Component({
  selector: 'app-verify',
  imports: [
    FormsModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    AutofocusDirective, MatSlideToggle, MatProgressSpinner,
    RecaptchaV3Module
  ],
  templateUrl: './verify.component.html',
  styleUrl: './verify.component.scss'
})
export class VerifyComponent implements OnDestroy {
  isMobileSig = inject(ResponsiveService).isMobileSig;
  isRequestingAnotherCode: boolean = false;
  email: string | undefined;
  recaptchaToken: string | undefined;
  isRecaptchaValidating = false;
  private _accountService = inject(AccountService);
  private _recaptchaService = inject(ReCaptchaV3Service);
  private _fb = inject(FormBuilder);
  verificationCodeCtrl = this._fb.control('',
    [Validators.required, Validators.minLength(6), Validators.maxLength(6), Validators.pattern(/^\d+$/)]);
  recaptchaCtrl = this._fb.control(false, [Validators.required]);
  private _subscribedRecaptcha: Subscription | undefined;
  private _isVerifyingAccount = inject(CommonService).isVerifyingAccountSig;

  constructor() {
    this.setEmail();
  }

  ngOnDestroy(): void {
    this._subscribedRecaptcha?.unsubscribe();
  }

  validateRecaptcha(): void {
    this.recaptchaToken = undefined; // reset
    this.isRecaptchaValidating = true;

    if (this.recaptchaCtrl.value)
      this._subscribedRecaptcha = this._recaptchaService.execute('login').subscribe(
        (token: string) => {
          if (token) {
            this.recaptchaToken = token;
            this.isRecaptchaValidating = false;
          }
        });
  }

  setEmail() {
    const email = sessionStorage.getItem('email');

    if (email)
      this.email = email;
  }

  verifyAccount(): void {
    if (this.email && this.verificationCodeCtrl.value) {
      const verify: Verify = {
        email: this.email,
        code: this.verificationCodeCtrl.value
      }

      this._accountService.verify(verify)
        .pipe(take(1))
        .subscribe();
    }
  }

  requestAnotherCode(): void {
    this.isRequestingAnotherCode = true;
  }

  cancelRequestAnotherCode(): void {
    this.isRequestingAnotherCode = false;
  }

  cancelVerification(): void {
    this._isVerifyingAccount.set(false);
  }

  resendVerifyCode(): void {
    if (this.email && this.recaptchaToken) {
      const resendRequest: RecoveryValidationRequest = {
        email: this.email,
        recaptchaToken: this.recaptchaToken
      }

      this._accountService.resendVerifyCode(resendRequest)
        .pipe(take(1))
        .subscribe({
          next: () => this.isRequestingAnotherCode = false,
          error: () => this.isRequestingAnotherCode = true
        });
    }
  }
}

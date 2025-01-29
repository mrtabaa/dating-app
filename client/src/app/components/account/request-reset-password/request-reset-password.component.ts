import {Component, inject, OnDestroy} from '@angular/core';
import {MatButtonModule} from "@angular/material/button";
import {MatIconModule} from "@angular/material/icon";
import {MatProgressSpinner} from "@angular/material/progress-spinner";
import {MatSlideToggle} from "@angular/material/slide-toggle";
import {InputCvaComponent} from "../../_helpers/input-cva/input-cva.component";
import {MatFormFieldModule} from "@angular/material/form-field";
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {MatInputModule} from "@angular/material/input";
import {RecaptchaV3Module, ReCaptchaV3Service} from "ng-recaptcha";
import {AccountService} from "../../../services/account.service";
import {Subscription, take} from "rxjs";
import {ResponsiveService} from "../../../services/responsive.service";
import {RecoveryValidationRequest} from "../../../models/account/recovery-validation-request.model";
import {CommonService} from "../../../services/common.service";

@Component({
  selector: 'app-request-reset-password',
  imports: [
    FormsModule, ReactiveFormsModule, InputCvaComponent,
    MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    MatSlideToggle, MatProgressSpinner,
    RecaptchaV3Module
  ],
  templateUrl: './request-reset-password.component.html',
  styleUrl: './request-reset-password.component.scss'
})
export class RequestResetPasswordComponent implements OnDestroy {
  isMobileSig = inject(ResponsiveService).isMobileSig;
  recaptchaToken: string | undefined;
  isRecaptchaValidating = false;
  isEmailSent = false;
  apiMessage: string | undefined;
  private _isWelcomeSig = inject(ResponsiveService).isWelcomeCompSig;
  private _accountService = inject(AccountService);
  private _fb = inject(FormBuilder);
  requestResetFg = this._fb.group({
    emailCtrl: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^\s*([\w.-]+)@([\w-]+)((\.(\w){2,5})+)\s*$/)]],
    recaptchaCtrl: [false, [Validators.required]],
  });
  private _isResettingPassword = inject(CommonService).isResetPasswordRequestCompSig;
  private _recaptchaService = inject(ReCaptchaV3Service);
  private _subscribedRecaptcha: Subscription | undefined;

  get EmailCtrl(): FormControl {
    return this.requestResetFg.get('emailCtrl') as FormControl;
  }

  get RecaptchaCtrl(): FormControl {
    return this.requestResetFg.get('recaptchaCtrl') as FormControl;
  }

  ngOnDestroy(): void {
    this._subscribedRecaptcha?.unsubscribe();
  }

  validateRecaptcha(): void {
    this.recaptchaToken = undefined; // reset
    this.isRecaptchaValidating = true;

    if (this.RecaptchaCtrl.value)
      this._subscribedRecaptcha = this._recaptchaService.execute('login')
        .subscribe(
          (token: string) => {
            if (token) {
              this.recaptchaToken = token;
              this.isRecaptchaValidating = false;
            }
          });
  }

  requestPasswordReset(): void {
    if (this.recaptchaToken) {
      const request: RecoveryValidationRequest = {
        email: this.EmailCtrl.value.trim(),
        recaptchaToken: this.recaptchaToken,
      }

      this._accountService.requestResetPassword(request)
        .pipe(
          take(1)
        ).subscribe({
        next: (res) => {
          if (res) {
            this.apiMessage = res.message;
            this.isEmailSent = true;
          }
        }
      });
    }
  }

  cancelRequest(): void {
    this._isResettingPassword.set(false);
  }
}

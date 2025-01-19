import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {
    AbstractControlOptions,
    FormBuilder,
    FormControl,
    FormsModule,
    ReactiveFormsModule,
    Validators
} from "@angular/forms";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {MatButtonModule} from "@angular/material/button";
import {MatIconModule} from "@angular/material/icon";
import {InputCvaComponent} from "../../_helpers/input-cva/input-cva.component";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatInputModule} from "@angular/material/input";
import {ResetPassword} from "../../../models/account/reset-password.model";
import {AccountService} from "../../../services/account.service";
import {finalize, take} from "rxjs";
import {NgStyle} from "@angular/common";
import {ResponsiveService} from "../../../services/responsive.service";
import {RegisterValidators} from "../../_helpers/validators/register.validator";
import {CommonService} from "../../../services/common.service";

@Component({
    selector: 'app-reset-password',
    imports: [
        FormsModule, ReactiveFormsModule, InputCvaComponent,
        MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, RouterLink, NgStyle
    ],
    templateUrl: './reset-password.component.html',
    styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit, OnDestroy {
    private _isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;
    private _isResetPasswordCompSig = inject(CommonService).isResetPasswordCompSig;
    private _router = inject(Router);
    private _route = inject(ActivatedRoute);
    private _accountService = inject(AccountService);
    private _fb = inject(FormBuilder);
    resetPasswordFg = this._fb.group({
        passwordCtrl: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(50), Validators.pattern(/^(?=.*[A-Z])(?=.*\d).+$/)]],
        confirmPasswordCtrl: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(50)]],
    }, {validators: [RegisterValidators.confirmPassword]} as AbstractControlOptions);
    private _email: string | null = null;
    private _resetToken: string | null = null;

    constructor() {
        this.setValuesFromResetLink();
    }

    get PasswordCtrl(): FormControl {
        return this.resetPasswordFg.get('passwordCtrl') as FormControl;
    }

    get ConfirmPasswordCtrl(): FormControl {
        return this.resetPasswordFg.get('confirmPasswordCtrl') as FormControl;
    }

    ngOnDestroy(): void {
        this._isWelcomeCompSig.set(true);
        this._isResetPasswordCompSig.set(false);
    }

    ngOnInit(): void {
        this._isWelcomeCompSig.set(false);
        this._isResetPasswordCompSig.set(true);
    }

    resetPassword(): void {
        if (this._email && this._resetToken) {
            const resetPassword: ResetPassword = {
                email: this._email,
                password: this.PasswordCtrl.value,
                confirmPassword: this.ConfirmPasswordCtrl.value,
                resetToken: this._resetToken
            }

            this._accountService.resetPassword(resetPassword)
                .pipe(
                    take(1),
                    finalize(() => this._router.navigate(['/']))
                )
                .subscribe();
        }
    }

    private setValuesFromResetLink(): void {
        this._email = this._route.snapshot.queryParamMap.get('email');
        this._resetToken = this._route.snapshot.queryParamMap.get('resetToken');
    }
}

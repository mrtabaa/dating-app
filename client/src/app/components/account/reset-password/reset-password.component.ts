import {Component, inject} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-reset-password',
  imports: [
    FormsModule
  ],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent {
  private _route = inject(ActivatedRoute);
  private _email: string | null = null;
  private _resetToken: string | null = null;

  private setValuesFromResetLink(): void {
    this._email = this._route.snapshot.queryParamMap.get('email');
    this._resetToken = this._route.snapshot.queryParamMap.get('resetToken');
  }
}

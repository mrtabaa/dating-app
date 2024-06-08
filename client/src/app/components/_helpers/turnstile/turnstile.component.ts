import { Component, Input } from '@angular/core';
import { NgxTurnstileModule, NgxTurnstileFormsModule } from "ngx-turnstile"; // CloudFlare
import { environment } from '../../../../environments/environment';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-turnstile',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    NgxTurnstileModule, NgxTurnstileFormsModule
  ],
  templateUrl: './turnstile.component.html',
  styleUrl: './turnstile.component.scss'
})
export class TurnstileComponent {
  @Input() turnstileCtrlIn!: FormControl;

  turnsTileSiteKey = environment.turnstileSiteKey;
}

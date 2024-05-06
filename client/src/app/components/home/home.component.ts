import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { LoginComponent } from '../account/login/login.component';
import { MatDivider } from '@angular/material/divider';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    LoginComponent,
    RouterLink,
    MatButtonModule, MatDivider
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {

  constructor() { }

}

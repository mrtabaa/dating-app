import {MatSidenavModule} from '@angular/material/sidenav'; 
import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-nav-mobile',
  standalone: true,
  imports: [
    MatSidenavModule, MatToolbarModule, MatIconModule, MatButtonModule
  ],
  templateUrl: './nav-mobile.component.html',
  styleUrl: './nav-mobile.component.scss'
})
export class NavMobileComponent {

}

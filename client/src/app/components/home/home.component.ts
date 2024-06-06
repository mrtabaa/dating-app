import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { LoginRegisterComponent } from '../account/login-register/login-register.component';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Observable, map } from 'rxjs';
import { WelcomeComponent } from './welcome/welcome.component';
import { FooterComponent } from '../footer/footer.component';
import { ResponsiveService } from '../../services/responsive.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    LoginRegisterComponent, WelcomeComponent, FooterComponent,
    NgOptimizedImage, MatIconModule,
    MatButtonModule, MatTabsModule, RouterLink, RouterLinkActive
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private responsiveService = inject(ResponsiveService);

  isMobileView$: Observable<boolean>;
  isGetStartedClicked = false;

  constructor() {
    this.isMobileView$ = this.breakpointObserver.observe('(min-width: 51rem)') // include iPad
      .pipe(map(({ matches }) => {
        matches = matches ? false : true

        this.responsiveService.isMobileSig.set(matches);

        return matches;
      }));
  }
}

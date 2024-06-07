import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet } from '@angular/router';
import { AccountService } from './services/account.service';
import { NgxSpinnerModule } from "ngx-spinner";
import { NavbarComponent } from './components/navbar/navbar.component';
import { UserService } from './services/user.service';
import { FooterComponent } from './components/footer/footer.component';
import { WelcomeComponent } from './components/home/welcome/welcome.component';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Observable, map } from 'rxjs';
import { ResponsiveService } from './services/responsive.service';
import { NavMobileComponent } from './components/navbar/nav-mobile/nav-mobile.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, RouterOutlet, NgxSpinnerModule,
    NavbarComponent, NavMobileComponent, FooterComponent, WelcomeComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  accountService = inject(AccountService);
  userService = inject(UserService);
  router = inject(Router);
  private breakpointObserver = inject(BreakpointObserver);
  loggedInUserSig = inject(AccountService).loggedInUserSig;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  isWelcomeCompSig = inject(ResponsiveService).isWelcomeCompSig;

  isMobileView$: Observable<boolean>;

  constructor() {
    this.isMobileView$ = this.breakpointObserver.observe('(min-width: 51rem)') // include iPad/tablet
      .pipe(map(({ matches }) => {
        matches = matches ? false : true

        this.isMobileSig.set(matches);

        return matches;
      }));
  }

  title = 'Hallboard';
  isLoading: boolean = false;

  ngOnInit(): void {
    this.accountService.reloadLoggedInUser();
  }
}

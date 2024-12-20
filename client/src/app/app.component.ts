import {Component, inject, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router, RouterOutlet} from '@angular/router';
import {AccountService} from './services/account.service';
import {NgxSpinnerModule} from "ngx-spinner";
import {NavbarComponent} from './components/navbar/navbar.component';
import {FooterComponent} from './components/footer/footer.component';
import {WelcomeComponent} from './components/home/welcome/welcome.component';
import {BreakpointObserver} from '@angular/cdk/layout';
import {map, Observable} from 'rxjs';
import {ResponsiveService} from './services/responsive.service';
import {NavMobileComponent} from './components/navbar/nav-mobile/nav-mobile.component';
import {CommonService} from './services/common.service';

@Component({
    selector: 'app-root',
    imports: [
        CommonModule, RouterOutlet, NgxSpinnerModule,
        NavbarComponent, NavMobileComponent, FooterComponent, WelcomeComponent
    ],
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  accountService = inject(AccountService);
  router = inject(Router);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  commonService = inject(CommonService);
  isMobileView$: Observable<boolean>;
  title = 'Hallboard';
  isLoading: boolean = false;
  private breakpointObserver = inject(BreakpointObserver);

  constructor() {
    this.isMobileView$ = this.breakpointObserver.observe('(min-width: 51rem)') // include iPad/tablet
      .pipe(map(({matches}) => {
        matches = !matches

        this.isMobileSig.set(matches);

        return matches;
      }));
  }

  ngOnInit(): void {
    this.accountService.reloadLoggedInUser();
  }
}

import {Component, effect, ElementRef, inject, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router, RouterOutlet} from '@angular/router';
import {AccountService} from './services/account.service';
import {NgxSpinnerModule} from "ngx-spinner";
import {NavbarComponent} from './components/navbar/navbar.component';
import {FooterComponent} from './components/footer/footer.component';
import {BreakpointObserver} from '@angular/cdk/layout';
import {map, Observable} from 'rxjs';
import {ResponsiveService} from './services/responsive.service';
import {NavMobileComponent} from './components/navbar/nav-mobile/nav-mobile.component';
import {CommonService} from './services/common.service';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule, RouterOutlet, NgxSpinnerModule,
    NavbarComponent, NavMobileComponent, FooterComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  accountService = inject(AccountService);
  router = inject(Router);
  isMobileSig = inject(ResponsiveService).isMobileSig;
  commonService = inject(CommonService);
  isMobileView$: Observable<boolean> | undefined;
  title = 'Hallboard';
  isLoading: boolean = false;
  private elementRef = inject(ElementRef);
  private breakpointObserver = inject(BreakpointObserver);

  constructor() {
    this.setBreakpointObserver();
    this.applyEffect();
  }

  ngOnInit(): void {
    this.accountService.reloadLoggedInUser();
  }

  private setBreakpointObserver(): void {
    this.isMobileView$ = this.breakpointObserver.observe('(min-width: 51rem)') // include iPad/tablet
      .pipe(map(({matches}) => {
        matches = !matches

        this.isMobileSig.set(matches);

        return matches;
      }));
  }

  private applyEffect(): void {
    effect(() => {
      if (this.accountService.loggedInUserSig()) {
        const badge = this.elementRef.nativeElement.ownerDocument.querySelector('.grecaptcha-badge');
        if (badge) {
          console.log(this.accountService.loggedInUserSig());
          (badge as HTMLElement).style.display = 'none';
        }
      }
    });
  }
}

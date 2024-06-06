import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet } from '@angular/router';
import { AccountService } from './services/account.service';
import { NgxSpinnerModule } from "ngx-spinner";
import { NavbarComponent } from './components/navbar/navbar.component';
import { UserService } from './services/user.service';
import { FooterComponent } from './components/footer/footer.component';
import { WelcomeComponent } from './components/home/welcome/welcome.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, RouterOutlet, NgxSpinnerModule,
    NavbarComponent, FooterComponent, WelcomeComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  accountService = inject(AccountService);
  userService = inject(UserService);
  router = inject(Router);

  title = 'Hallboard';
  isLoading: boolean = false;

  ngOnInit(): void {
    this.accountService.reloadLoggedInUser();
  }
}

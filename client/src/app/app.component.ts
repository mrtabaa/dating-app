import { AfterContentChecked, AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { User } from './models/user.model';
import { AccountService } from './services/account.service';
import { LoadingService } from './services/loading.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, AfterContentChecked {
  title = 'Dating App';
  isLoading: boolean = true;

  constructor(
    private accountService: AccountService,
    private loadingService: LoadingService,
    private cdRef: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.setLocalStorageCurrentValues();
  }
  
  // fix ChangeDetection issue after loading value is changed
  ngAfterContentChecked(): void {
    this.isLoading = this.loadingService.isLoading;
    this.cdRef.detectChanges();
  }

  setLocalStorageCurrentValues() {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user: User = JSON.parse(userString);
      this.accountService.setCurrentUser(user);
    }
  }
}

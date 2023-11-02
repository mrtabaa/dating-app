import { AfterContentChecked, AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { User } from './models/user.model';
import { AccountService } from './services/account.service';
import { LoadingService } from './services/loading.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Dating App';
  isLoading: boolean = false;

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
    this.getLocalStorageCurrentValues();
  }

  getLocalStorageCurrentValues() {
    const userString = localStorage.getItem('user');
    
    if (userString) {
      this.accountService.setCurrentUser(JSON.parse(userString));
    }
  }
}

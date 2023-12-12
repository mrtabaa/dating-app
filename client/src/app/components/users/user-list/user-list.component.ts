import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { Pagination } from '../../../models/helpers/pagination';
import { User } from '../../../models/user.model';
import { UserService } from '../../../services/user.service';
import { UserCardComponent } from '../user-card/user-card.component';
import { MatPaginatorModule } from '@angular/material/paginator';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    UserCardComponent,
    MatPaginatorModule
  ],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit, OnDestroy {
  //#region Variables
  private userService = inject(UserService);

  subscribed: Subscription | undefined;

  users: User[] = [];
  pagination: Pagination | undefined;

  // Material Pagination attrs
  pageSize = 5;
  pageIndex = 0; // add 1 before sending to API since endpoint's pageNumber starts from 1
  pageSizeOptions = [5, 10, 25];

  hidePageSize = false;
  showPageSizeOptions = true;
  showFirstLastButtons = true;
  disabled = false;

  pageEvent: PageEvent | undefined;

  //#endregion

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.subscribed?.unsubscribe();
  }

  loadUsers(): void {
    this.subscribed = this.userService.getUsers(this.pageIndex + 1, this.pageSize).subscribe({
      next: response => {
        if (response.result && response.pagination) {
          this.users = response.result;
          this.pagination = response.pagination;
        }
      }
    });
  }

  handlePageEvent(e: PageEvent) {
    this.pageEvent = e;
    this.pageSize = e.pageSize;
    this.pageIndex = e.pageIndex;
    this.loadUsers();
  }
}

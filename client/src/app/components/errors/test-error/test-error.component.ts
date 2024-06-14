import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { take } from 'rxjs';

@Component({
  standalone: true,
  imports: [],
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.scss']
})
export class TestErrorComponent {
  private http = inject(HttpClient);

  private baseUrl = 'https://localhost:5001/';
  validationErrors: string[] =[];

  get404Error(): void {
    this.http.get(this.baseUrl + 'buggy/not-found').pipe(take(1)).subscribe({
      next: res => console.log(res),
      error: err => console.log(err)
    });
  }

  get400Error(): void {
    this.http.get(this.baseUrl + 'buggy/bad-request').pipe(take(1)).subscribe({
      next: res => console.log(res),
      error: err => console.log(err)
    });
  }

  get500Error(): void {
    this.http.get(this.baseUrl + 'buggy/server-error').pipe(take(1)).subscribe({
      next: res => console.log(res),
      error: err => console.log(err)
    });
  }

  get401Error(): void {
    this.http.get(this.baseUrl + 'buggy/auth').pipe(take(1)).subscribe({
      next: res => console.log(res),
      error: err => console.log(err)
    });
  }

  get400RegisterValidationErrors(): void {
    this.http.post(this.baseUrl + 'account/register', {}).pipe(take(1)).subscribe({
      next: res => console.log(res),
      error: err => {
        this.validationErrors = err;
      }
    });
  }

  get400LoginValidationErrors(): void {
    this.http.post(this.baseUrl + 'account/login', {}).pipe(take(1)).subscribe({
      next: res => console.log(res),
      error: err => {
        this.validationErrors = err;
      }
    });
  }
}

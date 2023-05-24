import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {

  constructor() { }

  // getting updated in the LoadingInterceptor
  isLoading: boolean = true;
}

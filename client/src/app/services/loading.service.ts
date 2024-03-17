import { Injectable, inject } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private spinnerService = inject(NgxSpinnerService);
  isLoading: boolean | undefined;

  loading() {
    this.spinnerService.show(undefined, {
      type: 'ball-atom',
      bdColor: 'rgba(0, 0, 0, 0.8)',
      color: '#fff'
    })

    this.isLoading = true;
  }

  idle() {
    this.spinnerService.hide();

    this.isLoading = false;
  }
}

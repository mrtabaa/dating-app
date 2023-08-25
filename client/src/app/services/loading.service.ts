import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  constructor(private spinnerService: NgxSpinnerService) { }

  loading() {
    this.spinnerService.show(undefined, {
      type: 'ball-atom',
      bdColor: 'rgba(0, 0, 0, 0.8)',
      color: '#fff'
    })
  }

  idle() {
    this.spinnerService.hide();
  }
}

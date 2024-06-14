import { Injectable, inject, signal } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private spinnerService = inject(NgxSpinnerService);
  isLoadingsig = signal<boolean>(false);

  loading() {
    this.spinnerService.show(undefined, {
      type: 'ball-atom',
      bdColor: 'rgba(0, 0, 0, 0.8)',
      color: '#fff'
    })

    this.isLoadingsig.set(true);
  }

  idle() {
    this.spinnerService.hide();

    this.isLoadingsig.set(false);
  }
}

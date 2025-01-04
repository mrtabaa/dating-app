import {Component, inject, OnDestroy} from '@angular/core';
import {FormBuilder, ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatSelectModule} from '@angular/material/select';
import {MemberService} from '../../../../../services/member.service';
import {MemberParams} from '../../../../../models/helpers/member-params';
import {Subscription, tap} from 'rxjs';

@Component({
  selector: 'app-order-bottom-sheet',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule, MatSelectModule
  ],
  templateUrl: './order-bottom-sheet.component.html',
  styleUrl: './order-bottom-sheet.component.scss'
})
export class OrderBottomSheetComponent implements OnDestroy {
  memberParams: MemberParams | undefined;
  subscribed: Subscription | undefined;
  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];
  private _memberService = inject(MemberService);
  private _fb = inject(FormBuilder);
  orderByCtrl = this._fb.control(this._memberService.memberParams?.orderBy, [])

  constructor() {
    this.memberParams = this._memberService.memberParams;
  }

  ngOnDestroy(): void {
    if (this.subscribed)
      this.subscribed.unsubscribe();
  }

  getMembers(): void {
    this.setMemberParams()

    if (this.memberParams)
      this.subscribed = this._memberService.getMembers(this.memberParams).pipe(
        tap(res => {
          if (res)
            this._memberService.eventEmitOrderFilterBottomSheet.emit();
        })
      ).subscribe();
  }

  setMemberParams(): MemberParams | undefined {
    if (this.memberParams && this.orderByCtrl.value)
      this.memberParams.orderBy = this.orderByCtrl.value;

    return this.memberParams;
  }
}



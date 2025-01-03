import {Component, inject, OnDestroy, OnInit} from '@angular/core';
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
export class OrderBottomSheetComponent implements OnInit, OnDestroy {
  selectedOrderSig = inject(MemberService).selectedOrderSig;
  memberParams: MemberParams | undefined;
  subscribed: Subscription | undefined;
  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];
  private _memberService = inject(MemberService);
  private _fb = inject(FormBuilder);
  orderByCtrl = this._fb.control('lastActive', [])

  ngOnInit(): void {
    this.orderByCtrl.setValue(this._memberService.selectedOrderSig());
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
    this.selectedOrderSig.set(this.orderByCtrl.value);
    this.memberParams = this._memberService.memberParams;

    if (this.memberParams && this.orderByCtrl.value)
      this.memberParams.orderBy = this.orderByCtrl.value;

    return this.memberParams;
  }
}



import { Component, inject } from '@angular/core';
import { FormBuilder, AbstractControl, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { MemberService } from '../../../../../services/member.service';

@Component({
  selector: 'app-filter-bottom-sheet',
  standalone: true,
  imports: [
    FormsModule, ReactiveFormsModule, MatDividerModule,
    MatSliderModule, MatSelectModule, MatButtonModule,
  ],
  templateUrl: './filter-bottom-sheet.component.html',
  styleUrl: './filter-bottom-sheet.component.scss'
})
export class FilterBottomSheetComponent {
  private _fb = inject(FormBuilder);
  private _memberService = inject(MemberService);
  private _memberParams = inject(MemberService).memberParams;

  minAge = 18;
  maxAge = 99;

  //#region Reactive Form 
  filterFg = this._fb.group({
    genderCtrl: [this._memberService.selectedGenderSig()],
    minAgeCtrl: [this._memberService.selectedMinAgeSig()],
    maxAgeCtrl: [this._memberService.selectedMaxAgeSig()]
  });

  get GenderCtrl(): AbstractControl {
    return this.filterFg.get('genderCtrl') as FormControl;
  }
  get MinAgeCtrl(): AbstractControl {
    return this.filterFg.get('minAgeCtrl') as FormControl;
  }
  get MaxAgeCtrl(): AbstractControl {
    return this.filterFg.get('maxAgeCtrl') as FormControl;
  }
  //#endregion Reactive form

  updateMemberParams(): void {
    if (this._memberParams) {
      if (this.GenderCtrl.value) // skip setting gender if not selected
        this._memberParams.gender = this.GenderCtrl.value;
      this._memberParams.minAge = this.MinAgeCtrl.value;
      this._memberParams.maxAge = this.MaxAgeCtrl.value;

      this._memberService.selectedGenderSig.set(this.GenderCtrl.value)
      this._memberService.selectedMinAgeSig.set(this.MinAgeCtrl.value)
      this._memberService.selectedMaxAgeSig.set(this.MaxAgeCtrl.value)

      this._memberService.setMemberParams(this._memberParams);

      this._memberService.eventEmitOrderFilterBottomSheet.emit();
    }
  }

  disableButton(): boolean {
    return this.MinAgeCtrl.value === this._memberService.selectedMinAgeSig() &&
      this.MaxAgeCtrl.value === this._memberService.selectedMaxAgeSig() &&
      this.GenderCtrl.value === this._memberService.selectedGenderSig()
  }
}

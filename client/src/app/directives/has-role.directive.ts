import { Directive, Input, OnInit, TemplateRef, ViewContainerRef, inject } from '@angular/core';
import { AccountService } from '../services/account.service';

@Directive({
  selector: '[dirHasRole]',
  standalone: true
})
export class HasRoleDirective implements OnInit {
  @Input('dirHasRole') hasRole: string[] = [];

  private viewcontainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef<any>);
  private accountService = inject(AccountService);

  ngOnInit(): void {
    if(this.accountService.loggedInUserSig()?.roles.some(role => this.hasRole.includes(role)))
      this.viewcontainerRef.createEmbeddedView(this.templateRef);
    else
      this.viewcontainerRef.clear();
  }
}

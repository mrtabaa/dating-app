import { Directive, Input, OnInit, TemplateRef, ViewContainerRef, inject } from '@angular/core';
import { LoggedInUser } from '../models/logged-in-user.model';

@Directive({
  selector: '[dirHasRole]',
  standalone: true
})
export class HasRoleDirective implements OnInit {
  @Input('dirHasRole') hasRole: string[] = [];

  private viewcontainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef<any>);

  ngOnInit(): void {
    const loggedInUserStr = localStorage.getItem('loggedInUser');

    if (loggedInUserStr) {
      const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

      const roles = JSON.parse(atob(loggedInUser.token.split('.')[1])).role;

      if (roles.some((role: string) => this.hasRole.includes(role)))
        this.viewcontainerRef.createEmbeddedView(this.templateRef);
      else
        this.viewcontainerRef.clear();
    }
  }
}

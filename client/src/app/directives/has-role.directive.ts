import { Directive, Input, OnInit, TemplateRef, ViewContainerRef, inject } from '@angular/core';

@Directive({
  selector: '[dirHasRole]',
  standalone: true
})
export class HasRoleDirective implements OnInit {
  @Input('dirHasRole') hasRole: string[] = [];

  private viewcontainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef<any>);

  ngOnInit(): void {
    const token = localStorage.getItem('token');

    if (token) {
      const roles = JSON.parse(atob(token.split('.')[1])).role;

      if (roles.some((role: string) => this.hasRole.includes(role)))
        this.viewcontainerRef.createEmbeddedView(this.templateRef);
      else
        this.viewcontainerRef.clear();
    }
  }
}

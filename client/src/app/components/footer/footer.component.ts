import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ResponsiveService } from '../../services/responsive.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule
  ],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss'
})
export class FooterComponent {
  responsiveService = inject(ResponsiveService);
  
  currentYear: number = new Date().getFullYear();

  @Input() isLoggedInIn: boolean = false;
}

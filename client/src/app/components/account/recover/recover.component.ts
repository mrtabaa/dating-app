import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-recover',
    imports: [
        RouterModule,
        MatButtonModule
    ],
    templateUrl: './recover.component.html',
    styleUrl: './recover.component.scss'
})
export class RecoverComponent {

}

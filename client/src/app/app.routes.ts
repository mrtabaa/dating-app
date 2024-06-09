import { Routes } from '@angular/router';
import { NoAccessComponent } from './components/errors/no-access/no-access.component';
import { NotFoundComponent } from './components/errors/not-found/not-found.component';
import { ServerErrorComponent } from './components/errors/server-error/server-error.component';
import { TestErrorComponent } from './components/errors/test-error/test-error.component';
import { HomeComponent } from './components/home/home.component';
import { MessagesComponent } from './components/messages/messages.component';
import { MemberDetailComponent } from './components/members/member-detail/member-detail.component';
import { UserEditComponent } from './components/user/user-edit/user-edit.component';
import { authGuard } from './guards/auth.guard';
import { authLoggedInGuard } from './guards/auth-logged-in.guard';
import { preventUnsavedChangesGuard } from './guards/prevent-unsaved-changes.guard';
import { FollowsComponent } from './components/follows/follows.component';
import { AdminPanelComponent } from './components/admin/admin-panel/admin-panel.component';
import { adminGuard } from './guards/admin.guard';
import { RecoverComponent } from './components/account/recover/recover.component';
import { CompleteProfileComponent } from './components/account/complete-profile/complete-profile.component';
import { completeProfileGuard } from './guards/complete-profile.guard';
import { profileIsCompletedGuard } from './guards/profile-is-completed.guard';
import { MainComponent } from './components/main/main.component';

export const routes: Routes = [
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authLoggedInGuard],
        children: [
            { path: '', component: HomeComponent },
            { path: 'account/login', component: HomeComponent },
            { path: 'account/register', component: HomeComponent },
            { path: 'account/recover', component: RecoverComponent },
            { path: 'demo', component: HomeComponent }, // DEMO
        ]
    },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard, completeProfileGuard],
        children: [
            { path: 'home', component: MainComponent },
            { path: 'main', component: MainComponent },
            { path: 'member/:userName', component: MemberDetailComponent },
            { path: 'user/edit', component: UserEditComponent, canDeactivate: [preventUnsavedChangesGuard] },
            { path: 'friends', component: FollowsComponent },
            { path: 'messages', component: MessagesComponent },
            { path: 'admin', component: AdminPanelComponent, canActivate: [adminGuard] } // both authGuard and adminGuard applied
        ]
    },
    {
        path: '',
        children: [
            { path: 'account/complete-profile', component: CompleteProfileComponent, canActivate: [authGuard, profileIsCompletedGuard] },
            { path: 'errors', component: TestErrorComponent },
            { path: 'server-error', component: ServerErrorComponent },
            { path: 'no-access', component: NoAccessComponent }
        ]
    },
    { path: '**', component: NotFoundComponent, pathMatch: 'full' }
];

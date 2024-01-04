import { Routes } from '@angular/router';
import { LoginComponent } from './components/account/login/login.component';
import { RegisterComponent } from './components/account/register/register.component';
import { NoAccessComponent } from './components/errors/no-access/no-access.component';
import { NotFoundComponent } from './components/errors/not-found/not-found.component';
import { ServerErrorComponent } from './components/errors/server-error/server-error.component';
import { TestErrorComponent } from './components/errors/test-error/test-error.component';
import { HomeComponent } from './components/home/home.component';
import { ListsComponent } from './components/lists/lists.component';
import { MessagesComponent } from './components/messages/messages.component';
import { MemberDetailComponent } from './components/members/member-detail/member-detail.component';
import { MemberListComponent } from './components/members/member-list/member-list.component';
import { UserEditComponent } from './components/user/user-edit/user-edit.component';
import { authGuard } from './guards/auth.guard';
import { authLoggedInGuard } from './guards/auth-logged-in.guard';
import { preventUnsavedChangesGuard } from './guards/prevent-unsaved-changes.guard';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            { path: 'members', component: MemberListComponent },
            { path: 'member/:id', component: MemberDetailComponent },
            { path: 'user/edit', component: UserEditComponent, canDeactivate: [preventUnsavedChangesGuard] },
            { path: 'lists', component: ListsComponent },
            { path: 'messages', component: MessagesComponent },
        ]
    },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authLoggedInGuard],
        children: [
            { path: 'account/login', component: LoginComponent },
            { path: 'account/register', component: RegisterComponent },
        ]
    },
    {
        path: '',
        children: [
            { path: 'errors', component: TestErrorComponent },
            { path: 'server-error', component: ServerErrorComponent },
            { path: 'no-access', component: NoAccessComponent }
        ]
    },
    { path: '**', component: NotFoundComponent, pathMatch: 'full' }
];

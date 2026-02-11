import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { LabsListComponent } from './features/labs/labs-list/labs-list.component';
import { LabDetailsComponent } from './features/labs/lab-details/lab-details.component';
import { ToolsListComponent } from './features/tools/tools-list/tools-list.component';
import { ToolConsoleComponent } from './features/tools/tool-console/tool-console.component';
import { LeaderboardComponent } from './features/gamification/leaderboard/leaderboard.component';
import { ProfileComponent } from './features/profile/profile.component';
import { ManageAccountComponent } from './features/profile/manage-account/manage-account.component';
import { AdminComponent } from './features/admin/admin.component';
import { ReportsListComponent } from './features/reports/reports-list/reports-list.component';
import { AIChatComponent } from './features/ai-assistant/ai-chat/ai-chat.component';
import { ToolsSetupComponent } from './features/tools/tools-setup/tools-setup.component';
import { ArticleDetailComponent } from './features/knowledge-base/article-detail/article-detail.component';
import { KbListComponent } from './features/knowledge-base/kb-list/kb-list.component';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

import { LandingComponent } from './features/landing/landing/landing.component';

export const routes: Routes = [
    { path: '', component: LandingComponent, pathMatch: 'full' },
    { path: 'landing', component: LandingComponent },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'forgot-password', component: ForgotPasswordComponent },
    {
        path: '',
        component: MainLayoutComponent,
        canActivate: [authGuard],
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'dashboard', component: DashboardComponent },
            { path: 'labs', component: LabsListComponent },
            { path: 'labs/:id', component: LabDetailsComponent },
            { path: 'tools', component: ToolsListComponent },
            { path: 'tools/setup', component: ToolsSetupComponent },
            { path: 'tools/:id', component: ToolConsoleComponent },
            { path: 'leaderboard', component: LeaderboardComponent },
            { path: 'profile', component: ProfileComponent },
            { path: 'manage-account', component: ManageAccountComponent },
            { path: 'reports', component: ReportsListComponent },
            { path: 'ai-assistant', component: AIChatComponent },
            { path: 'knowledge-base', component: KbListComponent },
            { path: 'knowledge-base/:id', component: ArticleDetailComponent },
            { path: 'admin', component: AdminComponent, canActivate: [adminGuard] }
        ]
    },
    { path: '**', redirectTo: 'dashboard' }
];

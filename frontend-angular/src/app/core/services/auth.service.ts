import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, map, of, tap } from 'rxjs';
import { User } from '../models/user.model';
import { JwtHelperService } from '@auth0/angular-jwt';

import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private apiUrl = `${environment.apiUrl}/auth`;
    private userApiUrl = `${environment.apiUrl}/users`;
    private jwtHelper = new JwtHelperService();

    private userSubject = new BehaviorSubject<User | null>(this.loadUser());
    public user$ = this.userSubject.asObservable();

    private tokenSubject = new BehaviorSubject<string | null>(localStorage.getItem('token'));
    public token$ = this.tokenSubject.asObservable();

    constructor(private http: HttpClient, private router: Router) { }

    public get currentUserValue(): User | null {
        return this.userSubject.value;
    }

    public get isAuthenticated(): boolean {
        const token = this.tokenSubject.value;
        return !!token && !this.jwtHelper.isTokenExpired(token);
    }

    private loadUser(): User | null {
        const userJson = localStorage.getItem('user');
        return userJson ? JSON.parse(userJson) : null;
    }

    login(credentials: { email?: string; password?: string; }): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
            tap(response => {
                if (response.token) {
                    this.setSession(response.token, this.mapUser(response));
                }
            })
        );
    }

    verifyTwoFactorLogin(userId: number, code: string): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/2fa/verify-login`, { userId, code }).pipe(
            tap(response => {
                if (response.token) {
                    this.setSession(response.token, this.mapUser(response));
                }
            })
        );
    }

    register(data: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/register`, data).pipe(
            tap(response => {
                if (response.token) {
                    this.setSession(response.token, this.mapUser(response));
                }
            })
        );
    }

    resetPassword(token: string, password: string): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/reset-password`, { token, password });
    }

    forgotPassword(email: string): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/forgot-password`, { email });
    }

    /* Profile Management */

    updateProfilePhoto(photo: string): Observable<any> {
        return this.http.post(`${this.userApiUrl}/update-photo`, { photoUrl: photo }).pipe(
            tap(() => this.updateUserLocal({ profilePhoto: photo }))
        );
    }

    deleteProfilePhoto(): Observable<any> {
        return this.http.post(`${this.userApiUrl}/delete-photo`, {}).pipe(
            tap(() => this.updateUserLocal({ profilePhoto: undefined }))
        );
    }

    updateBio(bio: string): Observable<any> {
        return this.http.post(`${this.userApiUrl}/update-bio`, { bio }).pipe(
            tap(() => this.updateUserLocal({ bio }))
        );
    }

    updateName(name: string): Observable<any> {
        return this.http.post(`${this.userApiUrl}/update-name`, { name }).pipe(
            tap(() => this.updateUserLocal({ name }))
        );
    }

    deleteAccount(): Observable<any> {
        return this.http.delete(`${this.userApiUrl}/delete-account`);
    }

    changePassword(oldPassword: string, newPassword: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/change-password`, { oldPassword, newPassword });
    }

    /* 2FA Management */

    setupTwoFactor(): Observable<{ uri: string }> {
        return this.http.post<{ uri: string }>(`${this.apiUrl}/2fa/setup`, {});
    }

    enableTwoFactor(code: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/2fa/enable`, { code });
    }



    private mapUser(user: any): User {
        return {
            userId: user.userId || user.UserId,
            name: user.name || user.Name,
            email: user.email || user.Email,
            role: user.role || user.Role,
            isTwoFactorEnabled: user.isTwoFactorEnabled || user.IsTwoFactorEnabled,
            profilePhoto: user.profilePhoto || user.ProfilePhoto,
            bio: user.bio || user.Bio,
            completedRooms: user.completedRooms || user.CompletedRooms,
            points: user.points || user.Points,
            streak: user.streak || user.Streak
        };
    }

    private updateUserLocal(partialUser: Partial<User>) {
        const currentUser = this.userSubject.value;
        if (currentUser) {
            const updatedUser = { ...currentUser, ...partialUser };
            this.userSubject.next(updatedUser);
            localStorage.setItem('user', JSON.stringify(updatedUser));
        }
    }

    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        this.tokenSubject.next(null);
        this.userSubject.next(null);
        this.router.navigate(['/']);
    }

    private setSession(token: string, user: User) {
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));
        this.tokenSubject.next(token);
        this.userSubject.next(user);
    }

    getToken(): string | null {
        return this.tokenSubject.value;
    }
}

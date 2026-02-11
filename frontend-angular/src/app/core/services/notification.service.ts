import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Notification {
    notificationId: number;
    userId: number;
    title: string;
    message: string;
    isRead: boolean;
    createdAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private apiUrl = `${environment.apiUrl}/notifications`;

    private viewedNotificationSubject = new BehaviorSubject<Notification | null>(null);
    viewedNotification$ = this.viewedNotificationSubject.asObservable();

    constructor(private http: HttpClient) { }

    setViewingNotification(notif: Notification | null) {
        this.viewedNotificationSubject.next(notif);
    }

    getNotifications(): Observable<Notification[]> {
        return this.http.get<Notification[]>(this.apiUrl);
    }

    markAsRead(id: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/${id}/read`, {});
    }

    markAllAsRead(): Observable<any> {
        return this.http.post(`${this.apiUrl}/read-all`, {});
    }
}

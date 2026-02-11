import { Component, ElementRef, HostListener, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideBell,
  lucideHouse,
  lucideCircleHelp,
  lucideSettings,
  lucideLogOut,
  lucideShield,
  lucideX,
  lucideCheckCheck
} from '@ng-icons/lucide';

import { NotificationService, Notification } from '../../../core/services/notification.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, NgIconComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
  viewProviders: [provideIcons({
    lucideBell,
    lucideHouse,
    lucideCircleHelp,
    lucideSettings,
    lucideLogOut,
    lucideShield,
    lucideX,
    lucideCheckCheck
  })]
})
export class HeaderComponent implements OnInit {
  darkMode = true;
  notifications: Notification[] = [];
  unreadCount = 0;
  showNotifications = false;
  showProfileMenu = false;
  selectedNotification: Notification | null = null;

  @ViewChild('notificationsRef') notificationsRef!: ElementRef;
  @ViewChild('profileMenuRef') profileMenuRef!: ElementRef;

  constructor(
    public authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) { }

  ngOnInit() {
    const isDark = localStorage.getItem('darkMode') !== 'false';
    this.darkMode = isDark;
    if (isDark) {
      document.documentElement.classList.add('dark');
      document.body.classList.add('dark');
    }
    this.loadNotifications();
  }

  loadNotifications() {
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications = data;
        this.unreadCount = data.filter(n => !n.isRead).length;
      },
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  toggleDarkMode() {
    this.darkMode = !this.darkMode;
    localStorage.setItem('darkMode', String(this.darkMode));
    const html = document.documentElement;
    const body = document.body;
    if (this.darkMode) {
      html.classList.add('dark');
      body.classList.add('dark');
    } else {
      html.classList.remove('dark');
      body.classList.remove('dark');
    }
  }

  logout() {
    this.authService.logout();
  }

  navigate(path: string) {
    this.router.navigate([path]);
    this.showProfileMenu = false;
    this.showNotifications = false;
  }

  viewNotification(notif: Notification) {
    this.notificationService.setViewingNotification(notif);
    this.showNotifications = false;
    if (!notif.isRead) {
      this.markAsRead(notif.notificationId);
    }
  }

  markAsRead(id: number) {
    this.notificationService.markAsRead(id).subscribe({
      next: () => {
        const notif = this.notifications.find(n => n.notificationId === id);
        if (notif) {
          notif.isRead = true;
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        }
      }
    });
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
      }
    });
  }

  getUserInitials(): string {
    const user = this.authService.currentUserValue;
    if (!user?.name) return 'U';
    return user.name
      .split(' ')
      .map((n: string) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  @HostListener('document:mousedown', ['$event'])
  handleClickOutside(event: MouseEvent) {
    if (this.notificationsRef && this.notificationsRef.nativeElement && !this.notificationsRef.nativeElement.contains(event.target)) {
      this.showNotifications = false;
    }
    if (this.profileMenuRef && this.profileMenuRef.nativeElement && !this.profileMenuRef.nativeElement.contains(event.target)) {
      this.showProfileMenu = false;
    }
  }
}

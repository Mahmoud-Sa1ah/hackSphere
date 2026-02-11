import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideShield,
  lucideSparkles,
  lucideMail,
  lucideLock,
  lucideEye,
  lucideEyeOff,
  lucideSun,
  lucideMoon
} from '@ng-icons/lucide';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NgIconComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  viewProviders: [provideIcons({
    lucideShield,
    lucideSparkles,
    lucideMail,
    lucideLock,
    lucideEye,
    lucideEyeOff,
    lucideSun,
    lucideMoon
  })]
})
export class LoginComponent implements OnInit {
  email = '';
  password = '';
  otpCode = '';
  step: 'credentials' | '2fa' = 'credentials';
  tempUserId: number | null = null;
  loading = false;
  showPassword = false;
  darkMode = true;
  mounted = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.mounted = true;
    const isDark = localStorage.getItem('darkMode') !== 'false';
    this.darkMode = isDark;
    this.applyTheme(isDark);
  }

  toggleDarkMode() {
    this.darkMode = !this.darkMode;
    localStorage.setItem('darkMode', String(this.darkMode));
    this.applyTheme(this.darkMode);
  }

  applyTheme(isDark: boolean) {
    if (isDark) {
      document.documentElement.classList.add('dark');
      document.body.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
      document.body.classList.remove('dark');
    }
  }

  onSubmit() {
    if (this.loading) return;
    this.loading = true;

    if (this.step === 'credentials') {
      this.authService.login({ email: this.email, password: this.password }).subscribe({
        next: (response) => {
          this.loading = false;
          if (response.requiresTwoFactor) {
            this.step = '2fa';
            this.tempUserId = response.userId;
            this.toastr.info('Please enter your 2FA code');
          } else {
            this.toastr.success('Login successful!');
            this.router.navigate(['/dashboard']);
          }
        },
        error: (err) => {
          this.loading = false;
          this.toastr.error(err.error?.message || 'Login failed');
        }
      });
    } else {
      if (!this.tempUserId) return;
      this.authService.verifyTwoFactorLogin(this.tempUserId, this.otpCode).subscribe({
        next: (response) => {
          this.loading = false;
          this.toastr.success('Login successful!');
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          this.loading = false;
          this.toastr.error(err.error?.message || 'Invalid code');
        }
      });
    }
  }
}

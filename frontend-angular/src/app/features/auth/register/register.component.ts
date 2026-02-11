import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideMoon,
  lucideSun,
  lucideShield,
  lucideUser,
  lucideMail,
  lucideLock,
  lucideGraduationCap,
  lucideBriefcase,
  lucideSparkles,
  lucideEye,
  lucideEyeOff
} from '@ng-icons/lucide';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NgIconComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  viewProviders: [provideIcons({
    lucideMoon,
    lucideSun,
    lucideShield,
    lucideUser,
    lucideMail,
    lucideLock,
    lucideGraduationCap,
    lucideBriefcase,
    lucideSparkles,
    lucideEye,
    lucideEyeOff
  })]
})
export class RegisterComponent implements OnInit {
  name = '';
  email = '';
  password = '';
  showPassword = false;
  userType: 'learner' | 'professional' = 'learner';
  loading = false;
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
    this.applyTheme();
  }

  toggleDarkMode() {
    this.darkMode = !this.darkMode;
    localStorage.setItem('darkMode', String(this.darkMode));
    this.applyTheme();
  }

  applyTheme() {
    if (this.darkMode) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }

  validatePassword(pwd: string): boolean {
    const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
    return regex.test(pwd);
  }

  validateEmail(email: string): boolean {
    const allowedDomains = [
      'gmail.com', 'outlook.com', 'hotmail.com', 'yahoo.com',
      'icloud.com', 'protonmail.com', 'live.com', 'msn.com'
    ];
    const domain = email.split('@')[1]?.toLowerCase();
    return allowedDomains.includes(domain);
  }

  handleSubmit() {
    if (this.name.includes(' ') || this.email.includes(' ') || this.password.includes(' ')) {
      this.toastr.error('Username, email, and password cannot contain spaces');
      return;
    }

    if (!this.validateEmail(this.email)) {
      this.toastr.error('Please use a popular email provider (Gmail, Outlook, Yahoo, etc.)');
      return;
    }

    if (!this.validatePassword(this.password)) {
      this.toastr.error('Password must be at least 8 characters and include uppercase, lowercase, number, and special character.');
      return;
    }

    this.loading = true;
    const roleId = this.userType === 'learner' ? 2 : 3;

    this.authService.register({ name: this.name, email: this.email, password: this.password, roleId }).subscribe({
      next: (response) => {
        this.toastr.success('Registration successful!');
        this.router.navigate(['/dashboard']);
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Registration failed');
        this.loading = false;
      }
    });
  }

  toggleShowPassword() {
    this.showPassword = !this.showPassword;
  }
}

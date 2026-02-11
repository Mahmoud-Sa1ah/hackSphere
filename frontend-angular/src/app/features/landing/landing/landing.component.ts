import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideShield,
  lucideTarget,
  lucideTerminal,
  lucideCpu,
  lucideMenu,
  lucideMoon,
  lucideSun
} from '@ng-icons/lucide';
import { AuthService } from '../../../core/services/auth.service';
import { FooterComponent } from '../../../shared/components/footer/footer.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconComponent, FooterComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css',
  viewProviders: [provideIcons({
    lucideShield,
    lucideTarget,
    lucideTerminal,
    lucideCpu,
    lucideMenu,
    lucideMoon,
    lucideSun
  })]
})
export class LandingComponent implements OnInit {
  mobileMenuOpen = false;
  darkMode = true;
  currentYear = new Date().getFullYear();
  isLoggedIn = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit() {
    window.scrollTo(0, 0);

    // Check initial auth state
    this.isLoggedIn = !!this.authService.getToken();

    // Check dark mode preference
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

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  navigateTo(path: string) {
    this.router.navigate([path]);
  }
}

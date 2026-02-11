import { Component, ElementRef, HostListener, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideLayoutDashboard,
  lucideChevronLeft,
  lucideShield,
  lucideCode,
  lucideSearch,
  lucideZap,
  lucideLock,
  lucideFileSearch,
  lucideKey,
  lucideBrain,
  lucideBookOpen,
  lucideWrench,
  lucideBarChart,
  lucideFlaskConical,
  lucideTrophy
} from '@ng-icons/lucide';

interface SecurityTool {
  label: string;
  category: string;
  count: number;
  icon: string;
}

interface Resource {
  path: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, NgIconComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
  viewProviders: [provideIcons({
    lucideLayoutDashboard,
    lucideChevronLeft,
    lucideShield,
    lucideCode,
    lucideSearch,
    lucideZap,
    lucideLock,
    lucideFileSearch,
    lucideKey,
    lucideBrain,
    lucideBookOpen,
    lucideWrench,
    lucideBarChart,
    lucideFlaskConical,
    lucideTrophy
  })]
})
export class SidebarComponent {
  collapsed = signal(false);

  securityTools: SecurityTool[] = [
    { label: 'Web Security', category: 'Web', count: 10, icon: 'lucideShield' },
    { label: 'Network Security', category: 'Network', count: 8, icon: 'lucideCode' },
    { label: 'Reconnaissance', category: 'Reconnaissance', count: 6, icon: 'lucideSearch' },
    { label: 'Exploitation', category: 'Exploitation', count: 3, icon: 'lucideZap' },
    { label: 'Password Cracking', category: 'Password', count: 3, icon: 'lucideLock' },
    { label: 'Vulnerability Assessment', category: 'Vulnerability', count: 3, icon: 'lucideFileSearch' },
    { label: 'Post-Exploitation', category: 'Post-Exploitation', count: 5, icon: 'lucideKey' },
    { label: 'Code Analysis', category: 'Code Analysis', count: 7, icon: 'lucideCode' },
    { label: 'Container Security', category: 'Container', count: 5, icon: 'lucideShield' },
    { label: 'System Security', category: 'System', count: 6, icon: 'lucideShield' },
  ];

  resources: Resource[] = [
    { path: '/reports', label: 'Reports', icon: 'lucideBarChart' },
    { path: '/knowledge-base', label: 'Knowledge Base', icon: 'lucideBookOpen' },
    { path: '/tools/setup', label: 'Tools Setup', icon: 'lucideWrench' },
    { path: '/labs', label: 'Labs', icon: 'lucideFlaskConical' },
    { path: '/leaderboard', label: 'Leaderboard', icon: 'lucideTrophy' },
  ];

  constructor(private router: Router, private elementRef: ElementRef) { }

  toggleCollapse() {
    this.collapsed.update(v => !v);
  }

  handleCategoryClick(categoryLabel: string) {
    const toolCategory = this.securityTools.find(t => t.label === categoryLabel)?.category || categoryLabel;
    this.router.navigate(['/tools'], { queryParams: { category: toolCategory } });
  }

  @HostListener('document:mousedown', ['$event'])
  handleClickOutside(event: MouseEvent) {
    // Only auto-collapse if user clicks outside sidebar while it is expanded NOT by default?
    // The original logic was: if clicked outside AND !collapsed -> setCollapsed(true)
    // Wait, original logic:
    // if (sidebarRef.current && !sidebarRef.current.contains(event.target) && !collapsed) { setCollapsed(true) }
    // This creates an auto-collapse behavior.

    if (this.elementRef.nativeElement && !this.elementRef.nativeElement.contains(event.target) && !this.collapsed()) {
      this.collapsed.set(true);
    }
  }
}

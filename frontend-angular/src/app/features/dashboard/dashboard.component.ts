import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from './dashboard.service';
import { SignalRService } from '../../core/services/signalr.service';
import { DashboardData } from '../../core/models/dashboard-data.model';
import { NgxChartsModule, Color, ScaleType } from '@swimlane/ngx-charts';
import { provideIcons, NgIconComponent } from '@ng-icons/core';
import {
  lucideActivity,
  lucideTriangleAlert,
  lucideZap,
  lucideUsers,
  lucideTrendingUp,
  lucideRefreshCw,
  lucideDownload,
  lucideSettings,
  lucideShield,
  lucideMaximize2,
  lucideRadio
} from '@ng-icons/lucide';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, NgxChartsModule, NgIconComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  viewProviders: [provideIcons({
    lucideActivity,
    lucideTriangleAlert,
    lucideZap,
    lucideUsers,
    lucideTrendingUp,
    lucideRefreshCw,
    lucideDownload,
    lucideSettings,
    lucideShield,
    lucideMaximize2,
    lucideRadio
  })]
})
export class DashboardComponent implements OnInit, OnDestroy {
  data: DashboardData | null = null;
  loading = true;
  timeFilter = '7d';

  // Chart Data
  systemHealthChartData: any[] = [];
  colorScheme: Color = {
    name: 'custom',
    selectable: true,
    group: ScaleType.Linear,
    domain: ['#3B82F6']
  };

  constructor(
    private dashboardService: DashboardService,
    private signalRService: SignalRService,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.loadDashboard();
    this.setupSignalR();
  }

  ngOnDestroy() {
    // Ideally stop SignalR or unsubscribe here
    // this.signalRService.stopLauncherHub(); // Dont stop if other components use it
  }

  loadDashboard() {
    this.loading = true;
    this.dashboardService.getDashboardData(this.timeFilter).subscribe({
      next: (data) => {
        this.data = data;
        this.prepareChartData();
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load dashboard:', err);
        this.toastr.error('Failed to load dashboard data');
        this.data = this.getEmptyData();
        this.loading = false;
      }
    });
  }

  prepareChartData() {
    if (this.data?.systemHealthData) {
      this.systemHealthChartData = [
        {
          name: 'System Health',
          series: this.data.systemHealthData.map(d => ({
            name: d.time,
            value: d.value
          }))
        }
      ];
    }
  }

  setupSignalR() {
    this.signalRService.startLauncherHub({
      onScanComplete: (data: any) => {
        this.toastr.success(`Scan ${data.scanId} completed!`);
        this.loadDashboard();
      },
      onLauncherStatusChanged: (data: any) => {
        if (data.isOnline) {
          this.toastr.success('Launcher connected');
        } else {
          this.toastr.error('Launcher disconnected');
        }
        this.loadDashboard();
      }
    });

    this.signalRService.startNotificationHub({
      onNotification: () => {
        this.loadDashboard();
      }
    });
  }

  onTimeFilterChange(filter: string) {
    this.timeFilter = filter;
    this.loadDashboard();
  }

  handleExport() {
    if (this.data) {
      this.dashboardService.exportDashboardData(this.data, this.timeFilter);
      this.toastr.success('Dashboard data exported successfully');
    } else {
      this.toastr.error('No data to export');
    }
  }

  getEmptyData(): DashboardData {
    return {
      recentScans: [],
      activityFeed: [],
      unreadNotifications: 0,
      launcherStatus: { isOnline: false, status: 'Offline' },
      statistics: {
        totalScans: 0,
        totalLabs: 0,
        scansChange: 0,
        vulnerabilitiesFound: 0,
        vulnerabilitiesChange: 0,
        toolsUsed: 0,
        toolsUsedChange: 0,
        activeUsers: 0
      },
      toolCategoryUsage: [],
      systemHealthData: []
    };
  }

  get sortedToolUsage() {
    return this.data?.toolCategoryUsage || [];
  }

  // Helpers for template logic
  get totalScans() { return this.data?.statistics.totalScans || 0; }
  get scansChange() { return this.data?.statistics.scansChange || 0; }
  get vulnerabilities() { return this.data?.statistics.vulnerabilitiesFound || 0; }
  get vulnerabilitiesChange() { return this.data?.statistics.vulnerabilitiesChange || 0; }
  get toolsUsed() { return this.data?.statistics.toolsUsed || 0; }
  get toolsUsedChange() { return this.data?.statistics.toolsUsedChange || 0; }
  get activeUsers() { return this.data?.statistics.activeUsers || 0; }

  // Gradient Helper for template
  getGradient(index: number): string {
    const gradients = [
      'from-blue-500 to-cyan-500',
      'from-purple-500 to-pink-500',
      'from-orange-500 to-red-500',
      'from-green-500 to-emerald-500'
    ];
    return gradients[index % gradients.length];
  }

  calcWidth(usage: number): string {
    const maxUsage = Math.max(...(this.data?.toolCategoryUsage || []).map(c => c.usage), 1);
    return `${Math.min((usage / maxUsage) * 100, 100)}%`;
  }
}

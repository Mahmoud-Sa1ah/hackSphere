import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ToolService } from '../tool.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { ReportService } from '../../reports/report.service';
import { AuthService } from '../../../core/services/auth.service';
import { Tool } from '../../../core/models/tool.model';
import { ToolConfig, TOOL_CONFIGS, DEFAULT_CONFIG } from '../../../core/models/tool-config.model';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideArrowLeft,
  lucideShield,
  lucideActivity,
  lucideSearch,
  lucideEye,
  lucideLock,
  lucideTriangleAlert,
  lucideTerminal,
  lucideZap,
  lucideDatabase,
  lucidePlay,
  lucideGlobe,
  lucideHash,
  lucideWifi,
  lucideKey,
  lucideDownload,
  lucideChevronDown,
  lucideChevronUp
} from '@ng-icons/lucide';

@Component({
  selector: 'app-tool-console',
  standalone: true,
  imports: [CommonModule, FormsModule, NgIconComponent],
  templateUrl: './tool-console.component.html',
  styleUrl: './tool-console.component.css',
  viewProviders: [provideIcons({
    lucideArrowLeft,
    lucideShield,
    lucideActivity,
    lucideSearch,
    lucideEye,
    lucideLock,
    lucideTriangleAlert,
    lucideTerminal,
    lucideZap,
    lucideDatabase,
    lucidePlay,
    lucideGlobe,
    lucideHash,
    lucideWifi,
    lucideKey,
    lucideDownload,
    lucideChevronDown,
    lucideChevronUp
  })]
})
export class ToolConsoleComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('outputContainer') outputContainer!: ElementRef;

  tool: Tool | null = null;
  config: ToolConfig = DEFAULT_CONFIG;
  target = '';
  attackType = 'quick';
  customArgs = '';
  loading = false;
  currentScanId: number | null = null;
  output: string[] = [];
  toolId: number | null = null;
  aiSummary = '';
  aiNextSteps = '';
  isConsoleExpanded = false;

  stats = [
    { label: "Module Type", value: "Primary", iconName: "lucideShield", color: "text-blue-500", bg: "bg-blue-50" },
    { label: "Payloads", value: "0+", iconName: "lucideSearch", color: "text-cyan-500", bg: "bg-cyan-50" },
    { label: "Status", value: "Active", iconName: "lucideEye", color: "text-green-500", bg: "bg-green-50" },
    { label: "Access", value: "Root", iconName: "lucideLock", color: "text-purple-500", bg: "bg-purple-50" },
  ];

  toolVersion = '';
  localFilePath = '';
  userRole = '';
  isPro = false;
  isAdmin = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private toolService: ToolService,
    private signalRService: SignalRService,
    private toastr: ToastrService,
    private reportService: ReportService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.authService.user$.subscribe(user => {
      if (user) {
        this.userRole = user.role;
        this.isPro = user.role === 'Professional' || user.role === 'Admin';
        this.isAdmin = user.role === 'Admin';
      }
    });

    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.toolId = +id;
        this.loadTool();
        this.setupSignalR();
      }
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  ngOnDestroy() {
    // Clean up if needed
  }

  setupSignalR() {
    this.signalRService.startLauncherHub({
      onScanOutput: (data: any) => {
        if (data.output) {
          this.output.push(data.output);
        }
      },
      onScanComplete: (data: any) => {
        this.loading = false;
        this.toastr.success('Scan Completed!');
        if (data.summary) {
          this.aiSummary = data.summary;
          this.aiNextSteps = data.nextSteps || '';
        }
      }
    });
  }

  loadTool() {
    if (!this.toolId) return;

    this.toolService.getTool(this.toolId).subscribe({
      next: (tool) => {
        this.tool = tool;
        this.updateConfig(tool.category);
      },
      error: (err) => {
        console.error('Failed to load tool', err);
        this.toastr.error('Failed to load tool details');
        this.router.navigate(['/tools']);
      }
    });
  }

  updateConfig(category: string) {
    if (category.includes("Web")) this.config = TOOL_CONFIGS["Web Security"];
    else if (category.includes("Network") || category.includes("Port")) this.config = TOOL_CONFIGS["Network"];
    else if (category.includes("Exploit")) this.config = TOOL_CONFIGS["Exploitation"];
    else if (category.includes("Password") || category.includes("Brute")) this.config = TOOL_CONFIGS["Password"];
    else if (category.includes("Wireless")) this.config = TOOL_CONFIGS["Wireless"];
    else if (category.includes("Crypto")) this.config = TOOL_CONFIGS["Cryptography"];
    else this.config = DEFAULT_CONFIG;

    // Update stats dynamically
    this.stats = [
      { label: "Module Type", value: "Primary", iconName: "lucideShield", color: this.config.color, bg: this.config.bg },
      { label: "Payloads", value: this.config.payloads.length + "+", iconName: "lucideSearch", color: "text-cyan-500", bg: "bg-cyan-50" },
      { label: "Status", value: "Active", iconName: "lucideEye", color: "text-green-500", bg: "bg-green-50" },
      { label: "Access", value: "Root", iconName: "lucideLock", color: "text-purple-500", bg: "bg-purple-50" },
    ];
  }

  scrollToBottom() {
    try {
      if (this.outputContainer) {
        this.outputContainer.nativeElement.scrollTop = this.outputContainer.nativeElement.scrollHeight;
      }
    } catch (err) { }
  }

  handleLaunch() {
    if (!this.target) {
      this.toastr.error('Please enter a target');
      return;
    }

    if (!this.toolId) return;

    this.loading = true;
    this.output = ['Initializing tool execution...', 'Connecting to launcher service...', '----------------------------------------'];

    let toolArgs = '';
    if (this.attackType === 'quick') {
      toolArgs = ''; // Backend might handle default if empty, or we pass default flags
    } else {
      toolArgs = this.customArgs;
    }

    const request: any = {
      toolId: this.toolId,
      target: this.target.trim(),
      arguments: toolArgs.trim() || undefined
    };

    if (this.isPro && this.toolVersion) {
      request.toolVersion = this.toolVersion;
    }
    if (this.isPro && this.localFilePath) {
      request.localFilePath = this.localFilePath;
    }

    // Use scanTool for advanced features
    this.toolService.scanTool(request).subscribe({
      next: (res: any) => {
        if (res && res.scanId) {
          this.currentScanId = res.scanId;
        }
        this.toastr.success('Operation launched successfully!');
      },
      error: (err) => {
        this.loading = false;
        this.toastr.error(err.error?.message || 'Failed to launch tool');
        this.output.push(`ERROR: ${err.error?.message || 'Failed to launch tool'}`);
      }
    });
  }

  handleDownloadReport() {
    if (!this.currentScanId) return;
    this.reportService.downloadReport(this.currentScanId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `scan-${this.currentScanId}-report.pdf`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
        this.toastr.success('Report downloaded successfully');
      },
      error: (err) => {
        this.toastr.error('Failed to download report');
      }
    });
  }

  handleStop() {
    // Implement stop functionality if API supports it
    this.toastr.info('Stop functionality not fully implemented yet');
    this.loading = false; // Force stop UI
  }

  toggleConsole() {
    this.isConsoleExpanded = !this.isConsoleExpanded;
    if (this.isConsoleExpanded) {
      setTimeout(() => this.scrollToBottom(), 100);
    }
  }

  goBack() {
    this.router.navigate(['/tools']);
  }
}

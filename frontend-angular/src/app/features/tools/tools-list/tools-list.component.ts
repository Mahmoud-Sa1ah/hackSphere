import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToolService } from '../tool.service';
import { Tool } from '../../../core/models/tool.model';
import { ToastrService } from 'ngx-toastr';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideSearch, lucideX, lucidePlay, lucideDownload, lucideCircleCheck, lucideInfo } from '@ng-icons/lucide';

@Component({
  selector: 'app-tools-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NgIconComponent],
  templateUrl: './tools-list.component.html',
  styleUrl: './tools-list.component.css',
  viewProviders: [provideIcons({
    lucideSearch,
    lucideX,
    lucidePlay,
    lucideDownload,
    lucideCircleCheck,
    lucideInfo
  })]
})
export class ToolsListComponent implements OnInit {
  tools: Tool[] = [];
  filteredTools: Tool[] = [];
  loading = true;
  searchTerm = '';
  categoryFilter = '';

  constructor(
    private toolService: ToolService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.categoryFilter = params['category'] || '';
      if (this.tools.length > 0) {
        this.filterTools();
      }
    });
    this.loadTools();
  }

  loadTools() {
    this.loading = true;
    this.toolService.getTools().subscribe({
      next: (tools) => {
        this.tools = tools;
        this.filterTools();
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load tools', err);
        this.toastr.error('Failed to load tools');
        this.loading = false;
      }
    });
  }

  filterTools() {
    this.filteredTools = this.tools.filter(tool => {
      const matchSearch =
        tool.toolName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        tool.description.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        tool.category.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchCategory = !this.categoryFilter ||
        tool.category.toLowerCase() === this.categoryFilter.toLowerCase();

      return matchSearch && matchCategory;
    });
  }

  setCategoryFilter(category: string) {
    this.categoryFilter = category;
    this.filterTools();
  }

  handleRunTool(tool: Tool) {
    this.router.navigate(['/tools', tool.toolId]);
  }

  handleDownloadTool(tool: Tool) {
    if (tool.downloadUrl) {
      window.open(tool.downloadUrl, '_blank');
      this.toastr.info('Opening download page...');
    } else {
      this.toastr.warning('Download URL not available for this tool');
    }
  }

  handleDownloadZip(tool: Tool) {
    this.toastr.info(`Downloading ${tool.toolName}...`);
    this.toolService.downloadToolZip(tool.toolId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        const extension = tool.packageExtension || '.zip';
        link.download = `${tool.binaryName || tool.toolName}${extension}`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
        this.toastr.success('Download completed');
      },
      error: (err) => {
        console.error('Download failed', err);
        this.toastr.error('Failed to download tool package');
      }
    });
  }
}


import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminService, UserData, ToolData, KnowledgeArticle } from './services/admin.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideTrash2, lucideUser, lucideShield, lucideSend, lucideBookOpen, lucidePlus, lucidePencil, lucideEye, lucideEyeOff, lucideGlobe, lucideCheck, lucideX, lucideFileText, lucideImage } from '@ng-icons/lucide';
import { User } from '../../core/models/user.model';
import { Subscription } from 'rxjs';

import { RichTextEditorComponent } from '../../shared/components/rich-text-editor/rich-text-editor.component';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, NgIconComponent, RichTextEditorComponent],
  viewProviders: [provideIcons({ lucideTrash2, lucideUser, lucideShield, lucideSend, lucideBookOpen, lucidePlus, lucidePencil, lucideEye, lucideEyeOff, lucideGlobe, lucideCheck, lucideX, lucideFileText, lucideImage })],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  private userSub?: Subscription;
  users: UserData[] = [];
  tools: ToolData[] = [];
  knowledgeArticles: KnowledgeArticle[] = [];
  verificationRequests: any[] = [];
  loading = false;
  activeTab: 'users' | 'tools' | 'knowledge' | 'domains' = 'users';

  // Global modal state delegators
  uploadingImage = false;

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.userSub = this.authService.user$.subscribe(user => {
      this.currentUser = user;
      if (user) {
        this.loadData();
      }
    });
  }

  ngOnDestroy(): void {
    if (this.userSub) {
      this.userSub.unsubscribe();
    }
  }

  loadData() {
    this.loading = true;
    let loadedCount = 0;
    const checkDone = () => {
      loadedCount++;
      if (loadedCount >= 4) this.loading = false;
    };

    this.adminService.getUsers().subscribe({
      next: (data) => { this.users = data; checkDone(); },
      error: () => { this.toastr.error('Failed to load users'); checkDone(); }
    });

    this.adminService.getTools().subscribe({
      next: (data) => { this.tools = data; checkDone(); },
      error: () => { this.toastr.error('Failed to load tools'); checkDone(); }
    });

    this.adminService.getAllArticles().subscribe({
      next: (data) => { this.knowledgeArticles = data || []; checkDone(); },
      error: () => { this.knowledgeArticles = []; checkDone(); }
    });

    this.adminService.getPendingDomains().subscribe({
      next: (data) => { this.verificationRequests = data || []; checkDone(); },
      error: () => { this.verificationRequests = []; checkDone(); }
    });
  }

  handleDeleteUser(userId: number) {
    if (!confirm('Are you sure you want to delete this user?')) return;
    this.adminService.deleteUser(userId).subscribe({
      next: () => {
        this.toastr.success('User deleted successfully');
        this.users = this.users.filter(u => u.userId !== userId);
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to delete user')
    });
  }

  openNotificationDialog(user: UserData) {
    this.adminService.setUserForNotification(user);
  }

  /* Knowledge Base Delegation */
  openKnowledgeDialog(article?: KnowledgeArticle) {
    if (article) {
      this.adminService.setEditingArticle(article);
    } else {
      this.adminService.setShowKnowledgeDialog(true);
    }
  }

  handleImageUpload(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];

    if (!file.type.startsWith('image/')) {
      this.toastr.error('Please select an image file');
      return;
    }

    this.uploadingImage = true;
    const formData = new FormData();
    formData.append('file', file);

    this.adminService.uploadArticleImage(formData).subscribe({
      next: (res) => {
        // Since the editor is now in MainLayout, this image upload from here might be redundant
        // but we keep it for now if needed. 
        this.toastr.info('Global Editor should handle uploads now.');
        this.uploadingImage = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Failed to upload image');
        this.uploadingImage = false;
      }
    });
    input.value = '';
  }

  handleTogglePublish(article: KnowledgeArticle) {
    this.adminService.updateArticle(article.knowledgeBaseId, { isPublished: !article.isPublished }).subscribe({
      next: () => {
        this.toastr.success(`Article ${!article.isPublished ? 'published' : 'unpublished'} successfully`);
        article.isPublished = !article.isPublished;
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to update article')
    });
  }

  handleDeleteKnowledge(id: number) {
    if (!confirm('Are you sure?')) return;
    this.adminService.deleteArticle(id).subscribe({
      next: () => {
        this.toastr.success('Article deleted');
        this.knowledgeArticles = this.knowledgeArticles.filter(a => a.knowledgeBaseId !== id);
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to delete article')
    });
  }

  /* Tools */
  handleToolUpload(event: any, tool: ToolData) {
    const file = event.target.files[0];
    if (!file) return;

    const fileName = file.name.toLowerCase();
    const isZip = fileName.endsWith('.zip');
    const isPdf = fileName.endsWith('.pdf');

    if (!isZip && !isPdf) {
      this.toastr.error('Only .zip and .pdf files are allowed');
      return;
    }

    const expectedZipName = `${tool.toolName}.zip`.toLowerCase();
    const expectedPdfName = `${tool.toolName}.pdf`.toLowerCase();

    if (fileName !== expectedZipName && fileName !== expectedPdfName) {
      this.toastr.error(`File name must be "${tool.toolName}.zip" or "${tool.toolName}.pdf"`);
      return;
    }

    const formData = new FormData();
    formData.append('file', file);

    this.adminService.uploadToolPackage(tool.toolId, formData).subscribe({
      next: () => this.toastr.success('Tool uploaded successfully'),
      error: (err) => {
        console.error('Upload error details:', err);
        this.toastr.error(err.error?.message || 'Upload failed');
      }
    });
  }

  /* Domains */
  handleVerifyDomain(id: number, approve: boolean) {
    if (!confirm(`Are you sure you want to ${approve ? 'approve' : 'reject'}?`)) return;
    this.adminService.verifyDomain(id, approve).subscribe({
      next: () => {
        this.toastr.success(`Domain ${approve ? 'approved' : 'rejected'}`);
        this.verificationRequests = this.verificationRequests.filter(r => r.id !== id);
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to verify domain')
    });
  }
}

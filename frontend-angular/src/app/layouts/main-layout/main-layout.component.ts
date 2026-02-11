import { Component, ElementRef, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { HeaderComponent } from '../../shared/components/header/header.component';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { filter, Subscription } from 'rxjs';
import { NotificationService, Notification } from '../../core/services/notification.service';
import { AdminService, UserData, KnowledgeArticle } from '../../features/admin/services/admin.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideX, lucideBell, lucideBookOpen, lucidePlus } from '@ng-icons/lucide';
import { RichTextEditorComponent } from '../../shared/components/rich-text-editor/rich-text-editor.component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    HeaderComponent,
    SidebarComponent,
    FooterComponent,
    CommonModule,
    FormsModule,
    NgIconComponent,
    RichTextEditorComponent
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css',
  viewProviders: [provideIcons({ lucideX, lucideBell, lucideBookOpen, lucidePlus })]
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;
  private subscriptions = new Subscription();

  // Global Modals State
  viewedNotification: Notification | null = null;
  selectedUserForNotif: UserData | null = null;
  notifTitle = '';
  notifMessage = '';

  editingArticle: KnowledgeArticle | null = null;
  showKnowledgeDialog = false;
  knowledgeForm = {
    title: '',
    category: 'General',
    content: '',
    tags: '',
    isPublished: true
  };

  constructor(
    private router: Router,
    private notificationService: NotificationService,
    private adminService: AdminService,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.subscriptions.add(
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        if (this.scrollContainer) {
          this.scrollContainer.nativeElement.scrollTop = 0;
        }
      })
    );

    // Global Notification Viewer
    this.subscriptions.add(
      this.notificationService.viewedNotification$.subscribe(notif => {
        this.viewedNotification = notif;
      })
    );

    // Global Notification Sender
    this.subscriptions.add(
      this.adminService.userForNotification$.subscribe(user => {
        this.selectedUserForNotif = user;
        if (user) {
          this.notifTitle = '';
          this.notifMessage = '';
        }
      })
    );

    // Global Knowledge Editor
    this.subscriptions.add(
      this.adminService.editingArticle$.subscribe(article => {
        this.editingArticle = article;
        if (article) {
          this.knowledgeForm = {
            title: article.title,
            category: article.category,
            content: article.content,
            tags: article.tags,
            isPublished: article.isPublished
          };
        } else {
          this.knowledgeForm = { title: '', category: 'General', content: '', tags: '', isPublished: true };
        }
      })
    );

    this.subscriptions.add(
      this.adminService.showKnowledgeDialog$.subscribe(show => {
        this.showKnowledgeDialog = show;
      })
    );
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  // Global Handlers
  closeNotifViewer() {
    this.notificationService.setViewingNotification(null);
  }

  closeNotifSender() {
    this.adminService.setUserForNotification(null);
  }

  handleSendGlobalNotif() {
    if (!this.selectedUserForNotif || !this.notifMessage.trim()) return;
    const title = this.notifTitle.trim() || 'Admin Notification';
    this.adminService.sendNotification(this.selectedUserForNotif.userId, title, this.notifMessage).subscribe({
      next: () => {
        this.toastr.success('Notification sent successfully');
        this.closeNotifSender();
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to send notification')
    });
  }

  closeKnowledgeDialog() {
    this.adminService.setShowKnowledgeDialog(false);
  }

  handleSaveKnowledge() {
    if (!this.knowledgeForm.title.trim() || !this.knowledgeForm.content.trim()) {
      this.toastr.warning('Please fill in all required fields');
      return;
    }

    const obs = this.editingArticle
      ? this.adminService.updateArticle(this.editingArticle.knowledgeBaseId, this.knowledgeForm)
      : this.adminService.createArticle(this.knowledgeForm);

    obs.subscribe({
      next: () => {
        this.toastr.success(`Article ${this.editingArticle ? 'updated' : 'created'} successfully`);
        this.closeKnowledgeDialog();
        // Trigger a refresh if needed (could use another subject)
      },
      error: (err) => this.toastr.error(err.error?.message || 'Failed to save article')
    });
  }

  onEditorImageUpload(event: { file: File, callback: (url: string) => void }) {
    const formData = new FormData();
    formData.append('file', event.file);
    this.adminService.uploadArticleImage(formData).subscribe({
      next: (res) => event.callback(res.url),
      error: () => this.toastr.error('Image upload failed')
    });
  }
}

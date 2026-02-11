
import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { KnowledgeService, Article } from '../knowledge.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideBookOpen, lucideShield, lucideLock, lucideCode, lucideSearch, lucideZap, lucideKey, lucideArrowLeft, lucideCalendar, lucideTag } from '@ng-icons/lucide';
import { ToastrService } from 'ngx-toastr';

import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-article-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconComponent, DatePipe],
  templateUrl: './article-detail.component.html',
  styleUrl: './article-detail.component.css',
  viewProviders: [provideIcons({ lucideBookOpen, lucideShield, lucideLock, lucideCode, lucideSearch, lucideZap, lucideKey, lucideArrowLeft, lucideCalendar, lucideTag })]
})
export class ArticleDetailComponent implements OnInit {
  article: Article | null = null;
  loading = true;

  // Category color mapping
  categoryColors: { [key: string]: string } = {
    'Fundamentals': 'from-blue-500 to-cyan-500',
    'Web Security': 'from-red-500 to-orange-500',
    'Network Security': 'from-green-500 to-emerald-500',
    'Reconnaissance': 'from-purple-500 to-indigo-500',
    'Cryptography': 'from-yellow-500 to-amber-500',
    'Exploitation': 'from-pink-500 to-rose-500',
    'General': 'from-gray-500 to-slate-500'
  };

  constructor(
    private route: ActivatedRoute,
    private knowledgeService: KnowledgeService,
    private toastr: ToastrService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loadArticle(+id);
      }
    });
  }

  loadArticle(id: number) {
    this.loading = true;
    this.knowledgeService.getArticle(id).subscribe({
      next: (article) => {
        this.article = article;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load article', err);
        this.toastr.error('Failed to load article');
        this.loading = false;
      }
    });
  }

  getSafeContent(content: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(content);
  }

  getIconName(category: string): string {
    switch (category) {
      case 'Fundamentals': return 'lucideShield';
      case 'Web Security': return 'lucideLock';
      case 'Network Security': return 'lucideCode';
      case 'Reconnaissance': return 'lucideSearch';
      case 'Cryptography': return 'lucideKey';
      case 'Exploitation': return 'lucideZap';
      default: return 'lucideBookOpen';
    }
  }

  getGradient(category: string): string {
    return this.categoryColors[category] || this.categoryColors['General'];
  }
}

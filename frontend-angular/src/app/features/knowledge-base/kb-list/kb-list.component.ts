import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { KnowledgeService, Article } from '../knowledge.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideBookOpen, lucideTag, lucideClock, lucideSearch, lucideChevronRight } from '@ng-icons/lucide';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-kb-list',
    standalone: true,
    imports: [CommonModule, NgIconComponent, FormsModule, RouterLink],
    templateUrl: './kb-list.component.html',
    styleUrl: './kb-list.component.css',
    viewProviders: [provideIcons({ lucideBookOpen, lucideTag, lucideClock, lucideSearch, lucideChevronRight })]
})
export class KbListComponent implements OnInit {
    articles: Article[] = [];
    filteredArticles: Article[] = [];
    loading = true;
    searchQuery = '';
    selectedCategory: string | null = null;
    categories: string[] = [];

    constructor(
        private knowledgeService: KnowledgeService,
        private toastr: ToastrService
    ) { }

    ngOnInit() {
        this.loadArticles();
    }

    loadArticles() {
        this.loading = true;
        this.knowledgeService.getArticles().subscribe({
            next: (data) => {
                this.articles = data;
                this.categories = [...new Set(data.map(a => a.category).filter(Boolean))];
                this.filterArticles();
                this.loading = false;
            },
            error: (err) => {
                console.error('Failed to load KB articles', err);
                this.toastr.error('Failed to load knowledge base');
                this.loading = false;
            }
        });
    }

    filterArticles() {
        this.filteredArticles = this.articles.filter(article => {
            const matchSearch = !this.searchQuery ||
                article.title.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
                article.tags?.toLowerCase().includes(this.searchQuery.toLowerCase());

            const matchCategory = !this.selectedCategory || article.category === this.selectedCategory;

            return matchSearch && matchCategory;
        });
    }

    selectCategory(category: string | null) {
        this.selectedCategory = category;
        this.filterArticles();
    }

    stripHtml(html: string): string {
        if (!html) return '';
        const tmp = document.createElement('div');
        tmp.innerHTML = html;
        return tmp.textContent || tmp.innerText || '';
    }
}

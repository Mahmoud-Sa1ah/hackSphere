import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LabService } from '../lab.service';
import { Lab, LabResult } from '../../../core/models/lab.model';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideGraduationCap, lucideCircleCheck, lucideArrowRight, lucideSearch } from '@ng-icons/lucide';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-labs-list',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconComponent, FormsModule],
  templateUrl: './labs-list.component.html',
  styleUrl: './labs-list.component.css',
  viewProviders: [provideIcons({ lucideGraduationCap, lucideCircleCheck, lucideArrowRight, lucideSearch })]
})
export class LabsListComponent implements OnInit {
  labs: Lab[] = [];
  results: LabResult[] = [];
  filteredLabs: Lab[] = [];
  loading = true;
  searchQuery = '';
  selectedDifficulty: string | null = null;
  selectedCategory: string | null = null;

  difficulties = ['Easy', 'Medium', 'Hard'];
  categories: string[] = [];

  constructor(
    private labService: LabService,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;
    // Use forkJoin if we want parallel, but sequential is fine for now
    this.labService.getLabs().subscribe({
      next: (labs) => {
        this.labs = labs;
        this.categories = [...new Set(labs.map(l => l.category).filter(Boolean))];
        this.filterLabs();

        this.labService.getLabResults().subscribe({
          next: (results) => {
            this.results = results;
          },
          error: (err) => console.error('Failed to load results', err)
        });

        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load labs', err);
        this.toastr.error('Failed to load labs');
        this.loading = false;
      }
    });
  }

  isCompleted(labId: number): boolean {
    return this.results.some(r => r.labId === labId);
  }

  filterLabs() {
    this.filteredLabs = this.labs.filter(lab => {
      const matchesSearch = !this.searchQuery ||
        lab.title.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        lab.description.toLowerCase().includes(this.searchQuery.toLowerCase());

      const matchesDifficulty = !this.selectedDifficulty ||
        lab.difficulty?.toLowerCase() === this.selectedDifficulty.toLowerCase();

      const matchesCategory = !this.selectedCategory ||
        lab.category === this.selectedCategory;

      return matchesSearch && matchesDifficulty && matchesCategory;
    });
  }

  getDifficultyColor(difficulty: string): string {
    switch (difficulty?.toLowerCase()) {
      case 'easy':
        return 'bg-emerald-600/20 text-emerald-600 dark:text-emerald-400 border border-emerald-500/30';
      case 'medium':
        return 'bg-amber-600/20 text-amber-600 dark:text-amber-400 border border-amber-500/30';
      case 'hard':
        return 'bg-rose-600/20 text-rose-600 dark:text-rose-400 border border-rose-500/30';
      default:
        return 'bg-slate-600/20 text-slate-600 dark:text-slate-400 border border-slate-500/30';
    }
  }
}

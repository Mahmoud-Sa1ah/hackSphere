import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LabService } from '../lab.service';
import { Lab, LabResult, SolvedUser } from '../../../core/models/lab.model';
import { ToastrService } from 'ngx-toastr';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideArrowLeft,
  lucideGraduationCap,
  lucideTrophy,
  lucideCircleCheck,
  lucideTarget,
  lucideSend,
  lucideClock,
  lucideCalendar,
  lucideUser
} from '@ng-icons/lucide';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-lab-details',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NgIconComponent],
  templateUrl: './lab-details.component.html',
  styleUrl: './lab-details.component.css',
  viewProviders: [provideIcons({
    lucideArrowLeft,
    lucideGraduationCap,
    lucideTrophy,
    lucideCircleCheck,
    lucideTarget,
    lucideSend,
    lucideClock,
    lucideCalendar,
    lucideUser
  })]
})
export class LabDetailsComponent implements OnInit {
  lab: Lab | null = null;
  solvers: SolvedUser[] = [];
  myResult: LabResult | null = null;
  loading = true;
  submissionDetails = '';
  submitting = false;
  labId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private labService: LabService,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.labId = +id;
        this.loadLabDetails();
      }
    });
  }

  loadLabDetails() {
    if (!this.labId) return;
    this.loading = true;

    forkJoin({
      lab: this.labService.getLab(this.labId),
      solvers: this.labService.getSolvers(this.labId),
      results: this.labService.getLabResults()
    }).subscribe({
      next: (data: any) => {
        this.lab = data.lab;
        this.solvers = data.solvers;
        this.myResult = data.results.find((r: LabResult) => r.labId === this.labId) || null;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load lab details', err);
        this.toastr.error('Failed to load lab details');
        this.router.navigate(['/labs']);
      }
    });
  }

  handleSubmit() {
    if (!this.submissionDetails.trim()) {
      this.toastr.error('Please provide your solution');
      return;
    }

    if (!this.labId) return;

    this.submitting = true;
    this.labService.submitLab(this.labId, this.submissionDetails).subscribe({
      next: () => {
        this.toastr.success('Solution submitted! AI feedback will be available shortly.');
        this.submissionDetails = '';
        this.loadLabDetails();
        this.submitting = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Failed to submit solution');
        this.submitting = false;
      }
    });
  }

  getDifficultyColor(difficulty: string): string {
    switch (difficulty?.toLowerCase()) {
      case 'easy':
        return 'bg-emerald-600/20 text-emerald-400 border border-emerald-500/30';
      case 'medium':
        return 'bg-amber-600/20 text-amber-400 border border-amber-500/30';
      case 'hard':
        return 'bg-rose-600/20 text-rose-400 border border-rose-500/30';
      default:
        return 'bg-slate-600/20 text-slate-400 border border-slate-500/30';
    }
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideTrophy,
  lucideFlame,
  lucideTarget,
  lucideAward,
  lucideShare2,
  lucideSettings,
  lucideMapPin,
  lucideGraduationCap
} from '@ng-icons/lucide';
import { environment } from '../../../environments/environment';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';

interface GamificationStats {
  points: number;
  completedRooms: number;
  streak: number;
  rank: number;
  badges: any[];
}

interface CompletedLab {
  resultId: number;
  labId: number;
  completionTime: string;
  aiFeedback: string | null;
  lab: {
    labId: number;
    title: string;
    description: string;
    difficulty: string;
    category: string;
    points: number;
  }
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
  viewProviders: [provideIcons({
    lucideTrophy,
    lucideFlame,
    lucideTarget,
    lucideAward,
    lucideShare2,
    lucideSettings,
    lucideMapPin,
    lucideGraduationCap
  })]
})
export class ProfileComponent implements OnInit {
  stats: GamificationStats | null = null;
  completedLabs: CompletedLab[] = [];
  loading = true;
  activeTab: 'labs' | 'badges' = 'labs';

  constructor(
    public authService: AuthService,
    private http: HttpClient,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.fetchData();
  }

  fetchData() {
    const statsReq = this.http.get<GamificationStats>(`${environment.apiUrl}/gamification/my-stats`);
    const labsReq = this.http.get<CompletedLab[]>(`${environment.apiUrl}/labs/results`);

    forkJoin([statsReq, labsReq]).subscribe({
      next: ([statsRes, labsRes]) => {
        this.stats = statsRes;

        // Deduplicate labs
        const uniqueLabs = labsRes.reduce((acc: CompletedLab[], current: CompletedLab) => {
          if (!acc.find(item => item.labId === current.labId)) {
            acc.push(current);
          }
          return acc;
        }, []);
        this.completedLabs = uniqueLabs;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
        // Don't show error toast on 404/gamification not init for new users maybe?
        // But generally good to know.
      }
    });
  }

  setActiveTab(tab: 'labs' | 'badges') {
    this.activeTab = tab;
  }
}

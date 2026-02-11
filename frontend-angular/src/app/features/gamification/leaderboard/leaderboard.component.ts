import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideTrophy,
  lucideAward,
  lucideFlame,
  lucideTarget,
  lucideUser
} from '@ng-icons/lucide';
import { environment } from '../../../../environments/environment';

interface LeaderboardUser {
  userId: number;
  name: string;
  profilePhoto: string | null;
  points: number;
  completedRooms: number;
  streak: number;
  badges: any[];
}

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, NgIconComponent],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.css',
  viewProviders: [provideIcons({
    lucideTrophy,
    lucideAward,
    lucideFlame,
    lucideTarget,
    lucideUser
  })]
})
export class LeaderboardComponent implements OnInit {
  leaderboard: LeaderboardUser[] = [];
  loading = true;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.loadLeaderboard();
  }

  loadLeaderboard() {
    this.http.get<LeaderboardUser[]>(`${environment.apiUrl}/gamification/leaderboard`)
      .subscribe({
        next: (data) => {
          this.leaderboard = data;
          this.loading = false;
        },
        error: (err) => {
          console.error(err);
          this.loading = false;
        }
      });
  }
}

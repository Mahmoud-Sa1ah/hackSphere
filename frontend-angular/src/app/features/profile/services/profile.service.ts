import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface GamificationStats {
    points: number;
    completedRooms: number;
    streak: number;
    rank: number;
    badges: any[];
}

export interface CompletedLab {
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
    };
}

@Injectable({
    providedIn: 'root'
})
export class ProfileService {
    private apiUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    getMyStats(): Observable<GamificationStats> {
        return this.http.get<GamificationStats>(`${this.apiUrl}/gamification/my-stats`);
    }

    getLabResults(): Observable<CompletedLab[]> {
        return this.http.get<CompletedLab[]>(`${this.apiUrl}/labs/results`);
    }

    getDomainStatus(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/domain/status`);
    }

    uploadDomainProof(formData: FormData): Observable<any> {
        return this.http.post(`${this.apiUrl}/domain/upload`, formData);
    }
}

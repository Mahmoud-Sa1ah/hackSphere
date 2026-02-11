import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DashboardData } from '../../core/models/dashboard-data.model';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private apiUrl = '/api/dashboard';

    constructor(private http: HttpClient) { }

    getDashboardData(timeFilter: string = '7d'): Observable<DashboardData> {
        return this.http.get<DashboardData>(`${this.apiUrl}?timeFilter=${timeFilter}`);
    }

    exportDashboardData(data: DashboardData, timeFilter: string) {
        const exportData = {
            exportDate: new Date().toISOString(),
            timeFilter: timeFilter,
            statistics: data.statistics,
            toolCategoryUsage: data.toolCategoryUsage,
            recentScans: data.recentScans,
            activityFeed: data.activityFeed,
            systemHealth: data.systemHealthData
        };

        const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `dashboard-export-${timeFilter}-${new Date().toISOString().split('T')[0]}.json`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    }
}

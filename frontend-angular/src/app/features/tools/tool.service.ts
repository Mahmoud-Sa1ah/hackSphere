import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tool } from '../../core/models/tool.model';

@Injectable({
    providedIn: 'root'
})
export class ToolService {
    private apiUrl = '/api/tools';

    constructor(private http: HttpClient) { }

    getTools(): Observable<Tool[]> {
        return this.http.get<Tool[]>(this.apiUrl);
    }

    getTool(id: number): Observable<Tool> {
        return this.http.get<Tool>(`${this.apiUrl}/${id}`);
    }

    downloadToolZip(toolId: number): Observable<Blob> {
        return this.http.get(`${this.apiUrl}/${toolId}/download`, { responseType: 'blob' });
    }

    runTool(toolId: number, args: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/run`, { toolId, args });
    }

    scanTool(request: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/scan`, request);
    }

    getScans(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/scans`);
    }

    deleteScan(scanId: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/scans/${scanId}`);
    }
}

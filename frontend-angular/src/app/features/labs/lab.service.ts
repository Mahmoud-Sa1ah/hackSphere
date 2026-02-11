import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lab, LabResult } from '../../core/models/lab.model';

@Injectable({
    providedIn: 'root'
})
export class LabService {
    private apiUrl = '/api/labs';

    constructor(private http: HttpClient) { }

    getLabs(): Observable<Lab[]> {
        return this.http.get<Lab[]>(this.apiUrl);
    }

    getLab(id: number): Observable<Lab> {
        return this.http.get<Lab>(`${this.apiUrl}/${id}`);
    }

    getLabResults(): Observable<LabResult[]> {
        return this.http.get<LabResult[]>(`${this.apiUrl}/results`);
    }

    submitLab(labId: number, flag: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/${labId}/submit`, { flag });
    }

    getSolvers(labId: number): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/${labId}/solvers`);
    }
}

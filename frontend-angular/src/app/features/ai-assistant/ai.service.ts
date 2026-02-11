import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatMessage {
    role: 'user' | 'assistant';
    content: string;
    timestamp: Date;
}

@Injectable({
    providedIn: 'root'
})
export class AIService {
    private apiUrl = '/api/ai';

    constructor(private http: HttpClient) { }

    chat(dto: { message: string, fileData?: string, fileName?: string, fileType?: string }): Observable<{ response: string }> {
        return this.http.post<{ response: string }>(`${this.apiUrl}/chat`, dto);
    }

    analyzeScan(scanOutput: string): Observable<{ analysis: string }> {
        return this.http.post<{ analysis: string }>(`${this.apiUrl}/analyze`, { scanOutput });
    }
}

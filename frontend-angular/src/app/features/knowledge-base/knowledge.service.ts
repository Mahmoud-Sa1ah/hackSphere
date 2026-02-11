import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Article {
    knowledgeBaseId: number;
    title: string;
    category: string;
    content: string;
    tags: string;
    createdAt: Date;
    updatedAt?: Date;
}

@Injectable({
    providedIn: 'root'
})
export class KnowledgeService {
    private apiUrl = `${environment.apiUrl}/knowledge`;

    constructor(private http: HttpClient) { }

    getArticles(): Observable<Article[]> {
        return this.http.get<Article[]>(this.apiUrl);
    }

    getArticle(id: number): Observable<Article> {
        return this.http.get<Article>(`${this.apiUrl}/${id}`);
    }
}

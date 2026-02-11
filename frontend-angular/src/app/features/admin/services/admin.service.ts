import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface UserData {
    userId: number;
    name: string;
    email: string;
    role: string;
    createdAt: string;
}

export interface ToolData {
    toolId: number;
    toolName: string;
    category: string;
    description: string;
}

export interface KnowledgeArticle {
    knowledgeBaseId: number;
    title: string;
    category: string;
    content: string;
    tags: string;
    isPublished: boolean;
    createdAt: string;
    updatedAt?: string;
    createdBy?: number;
}

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private apiUrl = environment.apiUrl;

    private userForNotificationSubject = new BehaviorSubject<UserData | null>(null);
    userForNotification$ = this.userForNotificationSubject.asObservable();

    private editingArticleSubject = new BehaviorSubject<KnowledgeArticle | null>(null);
    editingArticle$ = this.editingArticleSubject.asObservable();

    private showKnowledgeDialogSubject = new BehaviorSubject<boolean>(false);
    showKnowledgeDialog$ = this.showKnowledgeDialogSubject.asObservable();

    constructor(private http: HttpClient) { }

    setUserForNotification(user: UserData | null) {
        this.userForNotificationSubject.next(user);
    }

    setEditingArticle(article: KnowledgeArticle | null) {
        this.editingArticleSubject.next(article);
        this.showKnowledgeDialogSubject.next(!!article || this.showKnowledgeDialogSubject.value);
    }

    setShowKnowledgeDialog(show: boolean) {
        this.showKnowledgeDialogSubject.next(show);
        if (!show) {
            this.editingArticleSubject.next(null);
        }
    }

    /* Users */
    getUsers(): Observable<UserData[]> {
        return this.http.get<UserData[]>(`${this.apiUrl}/admin/users`);
    }

    deleteUser(userId: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/admin/users/${userId}`);
    }

    sendNotification(userId: number, title: string, message: string): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/notifications`, {
            userId,
            title,
            message
        });
    }

    /* Tools */
    getTools(): Observable<ToolData[]> {
        return this.http.get<ToolData[]>(`${this.apiUrl}/tools`);
    }

    uploadToolPackage(toolId: number, formData: FormData): Observable<any> {
        return this.http.post(`${this.apiUrl}/tools/${toolId}/upload`, formData);
    }

    /* Knowledge Base */
    getAllArticles(): Observable<KnowledgeArticle[]> {
        return this.http.get<KnowledgeArticle[]>(`${this.apiUrl}/admin/knowledge/all`);
    }

    createArticle(article: Partial<KnowledgeArticle>): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/knowledge`, article);
    }

    updateArticle(id: number, article: Partial<KnowledgeArticle>): Observable<any> {
        return this.http.put(`${this.apiUrl}/admin/knowledge/${id}`, article);
    }

    deleteArticle(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/admin/knowledge/${id}`);
    }

    uploadArticleImage(formData: FormData): Observable<{ url: string }> {
        return this.http.post<{ url: string }>(`${this.apiUrl}/admin/knowledge/upload-image`, formData);
    }

    /* Domain Verification */
    getPendingDomains(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/admin/domains/pending`);
    }

    verifyDomain(id: number, approve: boolean): Observable<any> {
        return this.http.post(`${this.apiUrl}/admin/domains/${id}/verify`, { approve });
    }
}

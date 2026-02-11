import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AIService } from '../ai.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideSend, lucideBot, lucideUser, lucideLoader, lucidePaperclip, lucideFileText, lucideX } from '@ng-icons/lucide';
import { ToastrService } from 'ngx-toastr';

interface ChatMessage {
    role: 'user' | 'assistant';
    content: string;
    timestamp: Date;
}

@Component({
    selector: 'app-ai-chat',
    standalone: true,
    imports: [CommonModule, FormsModule, NgIconComponent],
    templateUrl: './ai-chat.component.html',
    styleUrl: './ai-chat.component.css',
    viewProviders: [provideIcons({ lucideSend, lucideBot, lucideUser, lucideLoader, lucidePaperclip, lucideFileText, lucideX })]
})
export class AIChatComponent implements OnInit, AfterViewChecked {
    @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

    messages: ChatMessage[] = [];
    newMessage = '';
    loading = false;
    selectedFile: File | null = null;
    fileBase64: string | null = null;
    @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

    recommendations = [
        'Analyze my last scan',
        'How to use Nmap effectively?',
        'Explain SQL Injection vulnerabilities',
        'Recommend next steps for my report',
        'Security best practices for web apps'
    ];

    constructor(
        private aiService: AIService,
        private toastr: ToastrService
    ) { }

    ngOnInit() {
        this.messages.push({
            role: 'assistant',
            content: 'Hello! I am your AI Security Assistant. How can I help you with your pentesting tasks today?',
            timestamp: new Date()
        });
    }

    ngAfterViewChecked() {
        this.scrollToBottom();
    }

    scrollToBottom(): void {
        try {
            this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
        } catch (err) { }
    }

    useRecommendation(text: string) {
        this.newMessage = text;
        this.sendMessage();
    }

    onFileSelected(event: any) {
        const file = event.target.files[0];
        if (file) {
            const allowedTypes = ['application/pdf', 'text/plain'];
            if (!allowedTypes.includes(file.type)) {
                this.toastr.error('Only PDF and Text files are supported');
                return;
            }

            this.selectedFile = file;
            const reader = new FileReader();
            reader.onload = () => {
                const base64 = reader.result as string;
                this.fileBase64 = base64.split(',')[1];
            };
            reader.readAsDataURL(file);
        }
    }

    removeFile() {
        this.selectedFile = null;
        this.fileBase64 = null;
        if (this.fileInput) {
            this.fileInput.nativeElement.value = '';
        }
    }

    sendMessage() {
        if (!this.newMessage.trim() && !this.selectedFile) return;

        const chatDto: any = { message: this.newMessage };

        if (this.selectedFile && this.fileBase64) {
            chatDto.fileData = this.fileBase64;
            chatDto.fileName = this.selectedFile.name;
            chatDto.fileType = this.selectedFile.type;
        }

        this.messages.push({
            role: 'user',
            content: this.newMessage + (this.selectedFile ? `\n[Attached File: ${this.selectedFile.name}]` : ''),
            timestamp: new Date()
        });

        this.newMessage = '';
        const fileToUpload = this.selectedFile;
        this.removeFile(); // Clear selection for next message
        this.loading = true;

        this.aiService.chat(chatDto).subscribe({
            next: (res: any) => {
                this.messages.push({
                    role: 'assistant',
                    content: res.response,
                    timestamp: new Date()
                });
                this.loading = false;
            },
            error: (err: any) => {
                this.toastr.error('Failed to get AI response');
                this.messages.push({
                    role: 'assistant',
                    content: 'Sorry, I encountered an error.',
                    timestamp: new Date()
                });
                this.loading = false;
            }
        });
    }
}

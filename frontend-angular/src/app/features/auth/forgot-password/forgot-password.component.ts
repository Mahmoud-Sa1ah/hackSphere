import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideShield } from '@ng-icons/lucide';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NgIconComponent],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
  viewProviders: [provideIcons({ lucideShield })]
})
export class ForgotPasswordComponent {
  email = '';
  isLoading = false;
  isSent = false;

  constructor(
    private authService: AuthService,
    private toastr: ToastrService
  ) { }

  handleSubmit() {
    this.isLoading = true;
    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.isSent = true;
        this.toastr.success('If the email exists, a reset link has been sent.');
        this.isLoading = false;
      },
      error: (err) => {
        this.toastr.error('An error occurred. Please try again.');
        this.isLoading = false;
      }
    });
  }
}

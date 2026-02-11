import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideShield } from '@ng-icons/lucide';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NgIconComponent],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css',
  viewProviders: [provideIcons({ lucideShield })]
})
export class ResetPasswordComponent implements OnInit {
  token: string | null = null;
  password = '';
  confirmPassword = '';
  isLoading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.token = this.route.snapshot.queryParams['token'];
  }

  onSubmit() {
    if (this.password !== this.confirmPassword) {
      this.toastr.error('Passwords do not match');
      return;
    }
    if (!this.token) {
      this.toastr.error('Invalid token');
      return;
    }

    this.isLoading = true;
    this.authService.resetPassword(this.token, this.password).subscribe({
      next: () => {
        this.toastr.success('Password reset successfully');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.toastr.error('Failed to reset password. Token may be invalid or expired.');
        this.isLoading = false;
      }
    });
  }
}

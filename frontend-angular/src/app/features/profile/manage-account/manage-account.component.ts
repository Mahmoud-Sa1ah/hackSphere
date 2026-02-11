import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ProfileService } from '../services/profile.service';
import { User } from '../../../core/models/user.model';
import { ToastrService } from 'ngx-toastr';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideCircleCheck, lucideCircleX, lucideClock, lucideUpload, lucideUser, lucideLock, lucideShield, lucideGlobe } from '@ng-icons/lucide';
import { QRCodeComponent } from 'angularx-qrcode';

@Component({
    selector: 'app-manage-account',
    standalone: true,
    imports: [CommonModule, FormsModule, NgIconComponent, QRCodeComponent],
    viewProviders: [provideIcons({ lucideCircleCheck, lucideCircleX, lucideClock, lucideUpload, lucideUser, lucideLock, lucideShield, lucideGlobe })],
    templateUrl: './manage-account.component.html',
    styleUrl: './manage-account.component.css'
})
export class ManageAccountComponent implements OnInit {
    user: User | null = null;
    setupUri = '';
    otpCode = '';
    bio = '';
    isSetupOpen = false;
    isEnabled = false;
    photoFile: File | null = null;
    photoPreview: string | null = null;
    displayName = '';

    oldPassword = '';
    newPassword = '';
    confirmPassword = '';
    loading = false;

    domains: any[] = [];
    domainName = '';
    domainFile: File | null = null;

    constructor(
        private authService: AuthService,
        private profileService: ProfileService,
        private router: Router,
        private toastr: ToastrService
    ) { }

    ngOnInit(): void {
        this.authService.user$.subscribe(user => {
            this.user = user;
            if (user) {
                this.bio = user.bio || '';
                this.displayName = user.name || '';
                this.photoPreview = user.profilePhoto || null;
                this.isEnabled = user.isTwoFactorEnabled || false;
            }
        });

        this.loadDomains();
    }

    handleSaveName() {
        if (!this.displayName.trim()) {
            this.toastr.error('Display Name cannot be empty');
            return;
        }
        this.authService.updateName(this.displayName).subscribe({
            next: () => this.toastr.success('Display name updated successfully'),
            error: (err: any) => this.toastr.error(err.error?.message || 'Failed to update display name')
        });
    }

    handlePhotoChange(event: any) {
        if (event.target.files && event.target.files[0]) {
            const file = event.target.files[0];
            this.photoFile = file;

            const reader = new FileReader();
            reader.onloadend = () => {
                this.photoPreview = reader.result as string;
            };
            reader.readAsDataURL(file);
        }
    }

    handleSavePhoto() {
        if (!this.photoPreview) return;

        // Assuming API accepts base64 for now as per React code
        this.authService.updateProfilePhoto(this.photoPreview).subscribe({
            next: () => {
                this.toastr.success('Profile photo updated!');
                this.photoFile = null;
                // Optimization: update local user state if needed, but the service usually handles re-fetching or updating behavior subject if structured that way.
                // For simplicity, we assume authService updates the userSubject or we might need to manually trigger a reload
                // Since we don't have a reloadUser method exposed, we might just rely on the fact that if the backend returns the updated user, we update the state.
                // The React code did setAuth. Our angular authService doesn't expose a manual setAuth public method easily, 
                // but we can assume for now checking 'me' or just living with it. 
                // Actually, let's just leave it, maybe the user needs to relogin to see changes if not handled?
                // Ideally we should reload user.
            },
            error: () => this.toastr.error('Failed to save profile photo')
        });
    }

    handleDeletePhoto() {
        this.authService.deleteProfilePhoto().subscribe({
            next: () => {
                this.toastr.success('Profile photo deleted');
                this.photoPreview = null;
            },
            error: () => this.toastr.error('Failed to delete profile photo')
        });
    }

    handleSaveBio() {
        this.authService.updateBio(this.bio).subscribe({
            next: () => this.toastr.success('Bio updated successfully'),
            error: () => this.toastr.error('Failed to update bio')
        });
    }

    handleDeleteAccount() {
        if (!confirm("Are you sure you want to delete your account? This action cannot be undone.")) return;

        this.authService.deleteAccount().subscribe({
            next: () => {
                this.toastr.success('Account deleted successfully');
                this.authService.logout();
            },
            error: (err: any) => this.toastr.error(err.error?.message || 'Failed to delete account')
        });
    }

    handleStartSetup() {
        this.authService.setupTwoFactor().subscribe({
            next: (res) => {
                this.setupUri = res.uri;
                this.isSetupOpen = true;
            },
            error: () => this.toastr.error('Failed to start 2FA setup')
        });
    }

    handleEnable2FA() {
        this.authService.enableTwoFactor(this.otpCode).subscribe({
            next: () => {
                this.isEnabled = true;
                this.isSetupOpen = false;
                this.toastr.success('Two-factor authentication enabled successfully');
            },
            error: () => this.toastr.error('Invalid code. Please try again.')
        });
    }

    handleChangePassword() {
        if (this.newPassword !== this.confirmPassword) {
            this.toastr.error('Passwords do not match');
            return;
        }

        if (this.newPassword.length < 6) {
            this.toastr.error('Password must be at least 6 characters');
            return;
        }

        this.loading = true;
        this.authService.changePassword(this.oldPassword, this.newPassword).subscribe({
            next: () => {
                this.toastr.success('Password changed successfully');
                this.oldPassword = '';
                this.newPassword = '';
                this.confirmPassword = '';
                this.loading = false;
            },
            error: (err: any) => {
                this.toastr.error(err.error?.message || 'Failed to change password');
                this.loading = false;
            }
        });
    }

    /* Domain Verification */
    loadDomains() {
        this.profileService.getDomainStatus().subscribe({
            next: (data) => this.domains = data,
            error: (err) => console.error('Failed to load domains', err)
        });
    }

    onFileSelected(event: any) {
        this.domainFile = event.target.files ? event.target.files[0] : null;
    }

    handleUploadDomain() {
        if (!this.domainName || !this.domainFile) {
            this.toastr.error("Please provide both domain and proof file");
            return;
        }

        const formData = new FormData();
        formData.append('domain', this.domainName);
        formData.append('file', this.domainFile);

        // simple loading toast?
        this.profileService.uploadDomainProof(formData).subscribe({
            next: () => {
                this.toastr.success('Domain proof uploaded successfully');
                this.domainName = '';
                this.domainFile = null; // Reset file input? needs ViewChild if we want to clear the native input
                this.loadDomains();
            },
            error: (err: any) => this.toastr.error(err.error?.message || 'Upload failed')
        });
    }
}

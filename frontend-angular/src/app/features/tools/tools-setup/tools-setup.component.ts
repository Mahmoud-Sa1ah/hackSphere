import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { lucideFolder, lucideFileText, lucideCircleAlert, lucideCircleCheck, lucideInfo } from '@ng-icons/lucide';

@Component({
  selector: 'app-tools-setup',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconComponent],
  templateUrl: './tools-setup.component.html',
  styleUrl: './tools-setup.component.css',
  viewProviders: [provideIcons({ lucideFolder, lucideFileText, lucideCircleAlert, lucideCircleCheck, lucideInfo })]
})
export class ToolsSetupComponent { }

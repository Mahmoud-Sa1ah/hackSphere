import { Component } from '@angular/core';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  lucideLinkedin,
  lucideTwitter,
  lucideFacebook,
  lucideYoutube,
  lucideInstagram,
  lucideMail,
  lucideMapPin,
  lucidePhone
} from '@ng-icons/lucide';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [NgIconComponent],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css',
  viewProviders: [provideIcons({
    lucideLinkedin,
    lucideTwitter,
    lucideFacebook,
    lucideYoutube,
    lucideInstagram,
    lucideMail,
    lucideMapPin,
    lucidePhone
  })]
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}

import { Component, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth';
import { ThemeService } from '../../../core/services/theme';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss'
})
export class SidebarComponent {
  authService = inject(AuthService);
  themeService = inject(ThemeService);

  @Output() linkClicked = new EventEmitter<void>();

  logout() {
    this.authService.logout();
  }
}

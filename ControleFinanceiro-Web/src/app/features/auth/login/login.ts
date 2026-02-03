import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  authService = inject(AuthService);
  router = inject(Router);

  username = '';
  senha = '';
  loading = false;
  error = '';

  onLogin() {
    this.error = '';

    if (!this.username) {
      this.error = 'Por favor, informe seu login';
      return;
    }

    if (!this.senha) {
      this.error = 'Por favor, informe sua senha';
      return;
    }

    this.loading = true;

    this.authService.login({ username: this.username, password: this.senha }).subscribe({
      next: () => {
        this.router.navigate(['/app/dashboard']);
      },
      error: (err: any) => {
        this.loading = false;
        if (err.status === 401) {
          this.error = 'Usuário ou senha incorretos';
        } else {
          this.error = 'Ocorreu um erro ao tentar entrar. Tente novamente.';
        }
        console.error('Login error', err);
      }
    });
  }
}

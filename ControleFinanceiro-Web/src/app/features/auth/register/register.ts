import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule, FormsModule, ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  authService = inject(AuthService);
  router = inject(Router);

  nome = '';
  username = '';
  email = '';
  senha = '';
  loading = false;
  error = '';
  success = '';

  onRegister() {
    this.error = '';
    this.success = '';

    if (!this.nome || !this.username || !this.email || !this.senha) {
      this.error = 'Por favor, preencha todos os campos';
      return;
    }

    this.loading = true;

    this.authService.register({
      nome: this.nome,
      username: this.username,
      email: this.email,
      password: this.senha
    }).subscribe({
      next: () => {
        this.success = 'Usuário cadastrado com sucesso! Redirecionando...';
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 2000);
      },
      error: (err: any) => {
        this.loading = false;
        if (err.status === 400 && err.error?.message) {
          this.error = err.error.message;
        } else {
          this.error = 'Ocorreu um erro ao realizar o cadastro. Tente novamente.';
        }
        console.error('Registration error', err);
      }
    });
  }
}

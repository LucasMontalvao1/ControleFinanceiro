import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule, FormsModule, ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  registerForm: FormGroup;
  loading = false;
  error = '';
  success = '';

  constructor() {
    this.registerForm = this.fb.group({
      nome: ['', [Validators.required, Validators.maxLength(100)]],
      username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  // Getters for form validation
  get f() { return this.registerForm.controls; }

  onRegister() {
    this.error = '';
    this.success = '';

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;

    this.authService.register(this.registerForm.value).subscribe({
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
        } else if (err.error?.errors) {
          // Flatten FluentValidation errors
          const validationErrors = Object.values(err.error.errors).flat();
          this.error = validationErrors[0] as string;
        } else {
          this.error = 'Ocorreu um erro ao realizar o cadastro. Tente novamente.';
        }
        console.error('Registration error', err);
      }
    });
  }
}

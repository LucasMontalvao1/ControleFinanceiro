import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

export interface AuthResponse {
  token?: string;
  Token?: string;
  nome?: string;
  Nome?: string;
  email?: string;
  Email?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = 'https://localhost:7058/api/auth';

  public currentUser = signal<string | null>(null);
  public isAuthenticated = signal<boolean>(false);

  constructor() {
    this.checkAuth();
  }

  register(request: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, request);
  }

  login(request: any): Observable<any> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        const res = response as any;
        const token = res.token || res.Token;
        const nome = res.nome || res.Nome;
        const email = res.email || res.Email;

        if (token) {
          localStorage.setItem('token', token);
          localStorage.setItem('username', nome || email || 'Usuário');
          this.currentUser.set(nome || email || 'Usuário');
          this.isAuthenticated.set(true);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
    this.router.navigate(['/auth/login']);
  }

  public checkAuth() {
    if (typeof window === 'undefined') return;

    const token = localStorage.getItem('token');
    const username = localStorage.getItem('username');

    if (token && token !== 'undefined' && token !== 'null') {
      this.currentUser.set(username || 'Usuário');
      this.isAuthenticated.set(true);
    } else {
      this.isAuthenticated.set(false);
    }
  }

  getToken(): string | null {
    if (typeof window === 'undefined') return null;
    const t = localStorage.getItem('token');
    return (t === 'undefined' || t === 'null') ? null : t;
  }
}

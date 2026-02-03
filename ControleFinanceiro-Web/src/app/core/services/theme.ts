import { Injectable, signal, effect } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  public darkMode = signal<boolean>(true);

  constructor() {
    // Carregar preferência salva ou usar dark mode por padrão
    if (typeof window !== 'undefined') {
      const savedTheme = localStorage.getItem('theme');
      if (savedTheme) {
        this.darkMode.set(savedTheme === 'dark');
      } else {
        // Default to dark mode if not set
        this.darkMode.set(true);
      }
    }

    // Efeito para aplicar a classe 'dark' no documento
    effect(() => {
      const isDark = this.darkMode();
      if (typeof document !== 'undefined') {
        if (isDark) {
          document.documentElement.classList.add('dark');
          localStorage.setItem('theme', 'dark');
        } else {
          document.documentElement.classList.remove('dark');
          localStorage.setItem('theme', 'light');
        }
      }
    });
  }

  toggleTheme() {
    this.darkMode.update(v => !v);
  }
}

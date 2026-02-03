import { ApplicationConfig, provideZoneChangeDetection, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import {
  LucideAngularModule,
  Wallet,
  LayoutDashboard,
  ArrowLeftRight,
  Tags,
  User,
  LogOut,
  Sun,
  Moon,
  Mail,
  Lock,
  UserPlus,
  LogIn,
  MoveRight,
  AlertCircle,
  CheckCircle,
  Plus,
  Pencil,
  Trash2,
  X,
  TrendingUp,
  TrendingDown,
  ArrowsUpFromLine,
  Repeat,
  Target,
  Folder,
  ChevronLeft,
  ChevronRight,
  Download,
  Search,
  Menu,
  BarChart2,
  PieChart
} from 'lucide-angular';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    importProvidersFrom(LucideAngularModule.pick({
      Wallet,
      LayoutDashboard,
      ArrowLeftRight,
      Tags,
      User,
      LogOut,
      Sun,
      Moon,
      Mail,
      Lock,
      UserPlus,
      LogIn,
      MoveRight,
      AlertCircle,
      CheckCircle,
      Plus,
      Pencil,
      Trash2,
      X,
      TrendingUp,
      TrendingDown,
      ArrowsUpFromLine,
      Repeat,
      Target,
      Folder,
      ChevronLeft,
      ChevronRight,
      Download,
      Search,
      Menu,
      BarChart2,
      PieChart
    }))
  ]
};

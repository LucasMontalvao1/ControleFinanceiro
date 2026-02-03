import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardSummary {
    totalEntradas: number;
    totalSaidas: number;
    saldo: number;
    gastosPorCategoria: Array<{ categoria: string; valor: number }>;
    evolucaoMensal: Array<{ data: string; entradas: number; saidas: number }>;
    evolucaoAnual: Array<{ mes: string; entradas: number; saidas: number; saldo: number }>;
    lancamentosRecentes: Array<{
        id: number;
        descricao: string;
        valor: number;
        data: string;
        tipo: string;
        categoriaId: number;
        categoriaNome: string;
    }>;
}

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7058/api/dashboard';

    getSummary(start?: string, end?: string): Observable<DashboardSummary> {
        let params: any = {};
        if (start) params.start = start;
        if (end) params.end = end;
        return this.http.get<DashboardSummary>(`${this.apiUrl}/summary`, { params });
    }
}

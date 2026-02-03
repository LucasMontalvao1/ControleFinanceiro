import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LancamentoRequest {
    descricao: string;
    valor: number;
    data: string; // ISO format
    tipo: string;
    categoriaId: number;
}

export interface LancamentoResponse {
    id: number;
    descricao: string;
    valor: number;
    data: string;
    tipo: string;
    categoriaId: number;
    categoriaNome: string;
}

@Injectable({
    providedIn: 'root'
})
export class LancamentoService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7058/api/lancamentos';

    getAll(start?: string, end?: string): Observable<LancamentoResponse[]> {
        let params: any = {};
        if (start) params.start = start;
        if (end) params.end = end;
        return this.http.get<LancamentoResponse[]>(this.apiUrl, { params });
    }

    getById(id: number): Observable<LancamentoResponse> {
        return this.http.get<LancamentoResponse>(`${this.apiUrl}/${id}`);
    }

    create(lancamento: LancamentoRequest): Observable<LancamentoResponse> {
        return this.http.post<LancamentoResponse>(this.apiUrl, lancamento);
    }

    update(id: number, lancamento: LancamentoRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, lancamento);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}

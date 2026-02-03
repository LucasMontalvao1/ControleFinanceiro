import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RecorrenteRequest {
    categoriaId: number;
    descricao: string;
    valor: number;
    diaVencimento: number;
    tipo: string;
    ativo: boolean;
}

export interface RecorrenteResponse extends RecorrenteRequest {
    id: number;
    categoriaNome: string;
}

@Injectable({
    providedIn: 'root'
})
export class RecorrenteService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:7058/api/recorrentes';

    getAll(): Observable<RecorrenteResponse[]> {
        return this.http.get<RecorrenteResponse[]>(this.apiUrl);
    }

    getById(id: number): Observable<RecorrenteResponse> {
        return this.http.get<RecorrenteResponse>(`${this.apiUrl}/${id}`);
    }

    create(request: RecorrenteRequest): Observable<number> {
        return this.http.post<number>(this.apiUrl, request);
    }

    update(id: number, request: RecorrenteRequest): Observable<boolean> {
        return this.http.put<boolean>(`${this.apiUrl}/${id}`, request);
    }

    delete(id: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
    }

    applyToMonth(mes: number, ano: number): Observable<number> {
        return this.http.post<number>(`${this.apiUrl}/apply/${mes}/${ano}`, {});
    }
}

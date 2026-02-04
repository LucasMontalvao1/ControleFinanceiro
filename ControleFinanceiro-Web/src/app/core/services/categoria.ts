import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CategoriaRequest {
    nome: string;
    tipo: string;
}

export interface CategoriaResponse {
    id: number;
    nome: string;
    tipo: string;
    isDefault: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class CategoriaService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/categorias`;

    getAll(): Observable<CategoriaResponse[]> {
        return this.http.get<CategoriaResponse[]>(this.apiUrl);
    }

    create(categoria: CategoriaRequest): Observable<CategoriaResponse> {
        return this.http.post<CategoriaResponse>(this.apiUrl, categoria);
    }

    update(id: number, categoria: CategoriaRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, categoria);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}

import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MetaRequest {
    categoriaId: number;
    valorLimite: number;
    mes: number;
    ano: number;
}

export interface MetaResponse extends MetaRequest {
    id: number;
    categoriaNome: string;
    valorRealizado: number;
}

@Injectable({
    providedIn: 'root'
})
export class MetaService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/metas`;

    getAll(mes: number, ano: number): Observable<MetaResponse[]> {
        return this.http.get<MetaResponse[]>(`${this.apiUrl}/${mes}/${ano}`);
    }

    createOrUpdate(request: MetaRequest): Observable<number> {
        return this.http.post<number>(this.apiUrl, request);
    }

    delete(id: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
    }
}

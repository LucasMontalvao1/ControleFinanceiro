import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReceiptItemDto {
    descricao: string;
    valor: number;
    categoriaSugerida: string;
    tipo: 'Entrada' | 'Saida';
}

export interface AiAnalyzeResponse {
    nomeLista: string;
    data: string;
    itens: ReceiptItemDto[];
    totalEstimado: number;
}

@Injectable({
    providedIn: 'root'
})
export class AiService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/ai`;

    analyzeImage(imageFile: File): Observable<AiAnalyzeResponse> {
        const formData = new FormData();
        formData.append('file', imageFile);
        return this.http.post<AiAnalyzeResponse>(`${this.apiUrl}/analyze`, formData);
    }
}

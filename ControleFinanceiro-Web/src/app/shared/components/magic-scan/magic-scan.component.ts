import { Component, EventEmitter, Output, signal, ViewChild, ElementRef, inject, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { AiService, AiAnalyzeResponse } from '../../../core/services/ai';
import { CategoriaResponse } from '../../../core/services/categoria';

@Component({
    selector: 'app-magic-scan',
    standalone: true,
    imports: [CommonModule, LucideAngularModule, FormsModule],
    templateUrl: './magic-scan.component.html',
    styleUrl: './magic-scan.component.scss'
})
export class MagicScanComponent implements OnInit, OnDestroy {
    private _categories: CategoriaResponse[] = [];
    @Input() set categories(value: CategoriaResponse[]) {
        this._categories = value;
        this.autoMatchPendingItems();
    }
    get categories(): CategoriaResponse[] {
        return this._categories;
    }

    @Output() close = new EventEmitter<void>();
    @Output() scanResult = new EventEmitter<any>();
    @Output() createCategory = new EventEmitter<{ nome: string, tipo: string }>();

    @ViewChild('videoElement') videoElement!: ElementRef<HTMLVideoElement>;
    @ViewChild('canvasElement') canvasElement!: ElementRef<HTMLCanvasElement>;

    private aiService = inject(AiService);

    capturedImage = signal<string | null>(null);
    analyzing = signal<boolean>(false);
    reviewing = signal<boolean>(false);

    scannedItems: any[] = [];
    nomeLista = '';
    dataLista = '';

    streamActive = false;
    private stream: MediaStream | null = null;

    ngOnInit() {
        this.initCamera();
    }

    onClose() {
        this.stopCamera();
        this.close.emit();
    }

    async initCamera() {
        try {
            this.stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: 'environment',
                    width: { ideal: 1280 },
                    height: { ideal: 720 }
                }
            });
            if (this.videoElement) {
                this.videoElement.nativeElement.srcObject = this.stream;
                this.streamActive = true;
            }
        } catch (err) {
            console.error('Erro ao acessar câmera:', err);
        }
    }

    capture() {
        const video = this.videoElement.nativeElement;
        const canvas = this.canvasElement.nativeElement;

        const maxWidth = 1280;
        let width = video.videoWidth;
        let height = video.videoHeight;

        if (width > maxWidth) {
            height = (maxWidth / width) * height;
            width = maxWidth;
        }

        canvas.width = width;
        canvas.height = height;

        const ctx = canvas.getContext('2d');
        ctx?.drawImage(video, 0, 0, width, height);

        this.capturedImage.set(canvas.toDataURL('image/jpeg', 0.8));
        this.stopCamera();
    }

    onFileSelected(event: any) {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e: any) => {
                this.capturedImage.set(e.target.result);
                this.stopCamera();
            };
            reader.readAsDataURL(file);
        }
    }

    retry() {
        this.capturedImage.set(null);
        this.reviewing.set(false);
        this.initCamera();
    }

    async analyze() {
        const dataUrl = this.capturedImage();
        if (!dataUrl) return;

        this.analyzing.set(true);

        try {
            const blob = await (await fetch(dataUrl)).blob();
            const file = new File([blob], 'receipt.jpg', { type: 'image/jpeg' });

            this.aiService.analyzeImage(file).subscribe({
                next: (result: any) => {
                    const data = result.value !== undefined ? result.value : (result.Value !== undefined ? result.Value : result);
                    const itens = data.itens || data.Itens || [];
                    this.nomeLista = data.nomeLista || data.NomeLista || 'Nova Lista';

                    // Normalize Date with robust detection
                    let rawData = (data.data || data.Data || '').toString().trim();
                    console.log('[MagicScan] Raw date detected:', rawData);

                    if (!rawData) {
                        this.dataLista = new Date().toISOString().split('T')[0];
                    } else {
                        const brDateMatch = rawData.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
                        if (brDateMatch) {
                            const [_, d, m, y] = brDateMatch;
                            this.dataLista = `${y}-${m.padStart(2, '0')}-${d.padStart(2, '0')}`;
                        } else if (rawData.includes('-') && rawData.length >= 10) {
                            this.dataLista = rawData.substring(0, 10);
                        } else {
                            console.warn('[MagicScan] Unrecognized date format:', rawData);
                            this.dataLista = new Date().toISOString().split('T')[0];
                        }
                    }

                    this.scannedItems = itens.map((item: any) => {
                        const descricao = item.descricao || item.Descricao || '';
                        const valor = item.valor || item.Valor || 0;
                        const tipoRaw = String(item.tipo || item.Tipo || 'Saida').toLowerCase();
                        const tipoLabel = (tipoRaw.includes('entrada') || tipoRaw.includes('receita')) ? 'Receita' : 'Despesa';
                        const categoriaSugerida = item.categoriaSugerida || item.CategoriaSugerida || '';

                        const matched = this.performMatch(tipoLabel, categoriaSugerida);

                        return {
                            descricao,
                            valor,
                            tipo: tipoLabel,
                            categoriaId: matched?.id || 0,
                            categoriaSugerida: categoriaSugerida,
                            matchFound: !!matched,
                            showAllCategories: false,
                            data: this.dataLista,
                            creating: false
                        };
                    });

                    this.analyzing.set(false);
                    this.reviewing.set(true);
                },
                error: (err) => {
                    console.error('Erro na análise:', err);
                    alert('Falha ao analisar imagem. Tente uma foto mais clara.');
                    this.analyzing.set(false);
                }
            });
        } catch (err) {
            console.error('Erro de processamento:', err);
            this.analyzing.set(false);
        }
    }

    confirm() {
        this.scanResult.emit({
            itens: this.scannedItems,
            nomeLista: this.nomeLista,
            data: this.dataLista
        });
        this.onClose();
    }

    removeItem(index: number) {
        this.scannedItems.splice(index, 1);
        if (this.scannedItems.length === 0) {
            this.retry();
        }
    }

    triggerCreateCategory(item: any) {
        if (!item.categoriaSugerida) return;

        if (item.creating) return;

        this.createCategory.emit({
            nome: item.categoriaSugerida,
            tipo: item.tipo
        });

        this.scannedItems.forEach(si => {
            if (si.categoriaSugerida === item.categoriaSugerida && si.tipo === item.tipo) {
                si.creating = true;
            }
        });
    }

    private performMatch(tipo: string, sugRaw: string): CategoriaResponse | null {
        if (!sugRaw) return null;
        const filteredCats = this.categories.filter(c => c.tipo === tipo);
        const sug = sugRaw.toLowerCase();

        let foundCat = filteredCats.find(c => c.nome.toLowerCase() === sug);

        if (!foundCat) {
            foundCat = filteredCats.find(c =>
                c.nome.toLowerCase().includes(sug) ||
                sug.includes(c.nome.toLowerCase())
            );
        }

        return foundCat || null;
    }

    private autoMatchPendingItems() {
        if (!this.scannedItems || this.scannedItems.length === 0) return;

        this.scannedItems.forEach(item => {
            if (!item.matchFound && item.categoriaSugerida) {
                const matched = this.performMatch(item.tipo, item.categoriaSugerida);
                if (matched) {
                    item.categoriaId = matched.id;
                    item.matchFound = true;
                    item.creating = false;
                }
            }
        });
    }

    updateItemTipo(item: any, tipo: 'Receita' | 'Despesa') {
        item.tipo = tipo;
        const matched = this.performMatch(tipo, item.categoriaSugerida);
        if (matched) {
            item.categoriaId = matched.id;
            item.matchFound = true;
        } else {
            item.categoriaId = 0;
            item.matchFound = false;
        }
    }

    private stopCamera() {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.streamActive = false;
        }
    }

    ngOnDestroy() {
        this.stopCamera();
    }
}

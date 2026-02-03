import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { CategoriaService, CategoriaResponse, CategoriaRequest } from '../../core/services/categoria';

@Component({
    selector: 'app-categorias',
    standalone: true,
    imports: [CommonModule, LucideAngularModule, FormsModule],
    templateUrl: './categorias.html',
    styleUrl: './categorias.scss'
})
export class CategoriasComponent implements OnInit {
    private categoriaService = inject(CategoriaService);

    categorias = signal<CategoriaResponse[]>([]);
    loading = signal<boolean>(false);
    showModal = signal<boolean>(false);

    // Form handling
    editMode = signal<boolean>(false);
    editingId = signal<number | null>(null);
    form = {
        nome: '',
        tipo: 'Receita'
    };

    ngOnInit() {
        this.loadCategorias();
    }

    loadCategorias() {
        this.loading.set(true);
        this.categoriaService.getAll().subscribe({
            next: (data: CategoriaResponse[]) => {
                this.categorias.set(data);
                this.loading.set(false);
            },
            error: (err: any) => {
                console.error('Error loading categories', err);
                this.loading.set(false);
            }
        });
    }

    openModal(categoria?: CategoriaResponse) {
        if (categoria) {
            this.editMode.set(true);
            this.editingId.set(categoria.id);
            this.form.nome = categoria.nome;
            this.form.tipo = categoria.tipo;
        } else {
            this.editMode.set(false);
            this.editingId.set(null);
            this.form.nome = '';
            this.form.tipo = 'Receita';
        }
        this.showModal.set(true);
    }

    closeModal() {
        this.showModal.set(false);
    }

    save() {
        if (!this.form.nome) return;

        const request: CategoriaRequest = {
            nome: this.form.nome,
            tipo: this.form.tipo
        };

        if (this.editMode()) {
            this.categoriaService.update(this.editingId()!, request).subscribe({
                next: () => {
                    this.loadCategorias();
                    this.closeModal();
                }
            });
        } else {
            this.categoriaService.create(request).subscribe({
                next: () => {
                    this.loadCategorias();
                    this.closeModal();
                }
            });
        }
    }

    delete(id: number) {
        if (confirm('Tem certeza que deseja excluir esta categoria?')) {
            this.categoriaService.delete(id).subscribe({
                next: () => this.loadCategorias()
            });
        }
    }

    getIconForType(tipo: string): string {
        return tipo === 'Receita' ? 'TrendingUp' : 'TrendingDown';
    }
}

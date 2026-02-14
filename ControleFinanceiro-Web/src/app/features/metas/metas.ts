import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { MetaService, MetaResponse, MetaRequest } from '../../core/services/meta';
import { CategoriaService, CategoriaResponse } from '../../core/services/categoria';

@Component({
    selector: 'app-metas',
    standalone: true,
    imports: [CommonModule, LucideAngularModule, FormsModule],
    templateUrl: './metas.html',
    styleUrl: './metas.scss'
})
export class MetasComponent implements OnInit {
    private metaService = inject(MetaService);
    private categoryService = inject(CategoriaService);

    currentDate = signal<Date>(new Date());
    metas = signal<MetaResponse[]>([]);
    categorias = signal<CategoriaResponse[]>([]);
    loading = signal<boolean>(true);
    showModal = signal<boolean>(false);

    form: MetaRequest = {
        categoriaId: 0,
        valorLimite: 0,
        mes: 0,
        ano: 0
    };

    currentMonthName = computed(() => {
        return this.currentDate().toLocaleString('pt-BR', { month: 'long', year: 'numeric' });
    });

    ngOnInit() {
        this.loadMetas();
        this.loadCategorias();
    }

    loadCategorias() {
        this.categoryService.getAll().subscribe(data => {
            this.categorias.set(data.filter(c => c.tipo === 'Despesa'));
        });
    }

    loadMetas() {
        this.loading.set(true);
        const mes = this.currentDate().getMonth() + 1;
        const ano = this.currentDate().getFullYear();

        this.metaService.getAll(mes, ano).subscribe({
            next: (data) => {
                this.metas.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                console.error('[Metas] Erro ao carregar:', err);
                this.loading.set(false);
            }
        });
    }

    changeMonth(delta: number) {
        const nextDate = new Date(this.currentDate());
        nextDate.setMonth(nextDate.getMonth() + delta);
        this.currentDate.set(nextDate);
        this.loadMetas();
    }

    calculatePercentage(meta: MetaResponse): number {
        if (meta.valorLimite <= 0) return 0;
        const p = (meta.valorRealizado / meta.valorLimite) * 100;
        return Math.min(p, 100);
    }

    isWarning(meta: MetaResponse): boolean {
        const p = (meta.valorRealizado / meta.valorLimite) * 100;
        return p >= 80 && p < 100;
    }

    isDanger(meta: MetaResponse): boolean {
        const p = (meta.valorRealizado / meta.valorLimite) * 100;
        return p >= 100;
    }

    openAddMetaModal() {
        this.form = {
            categoriaId: this.categorias().length > 0 ? this.categorias()[0].id : 0,
            valorLimite: 0,
            mes: this.currentDate().getMonth() + 1,
            ano: this.currentDate().getFullYear()
        };
        this.showModal.set(true);
    }

    openEditMetaModal(meta: MetaResponse) {
        this.form = {
            categoriaId: meta.categoriaId,
            valorLimite: meta.valorLimite,
            mes: meta.mes,
            ano: meta.ano
        };
        this.showModal.set(true);
    }

    closeModal() {
        this.showModal.set(false);
    }

    save() {
        if (this.form.valorLimite <= 0 || !this.form.categoriaId) {
            alert('Por favor, preencha todos os campos corretamente.');
            return;
        }

        this.metaService.createOrUpdate(this.form).subscribe({
            next: () => {
                this.loadMetas();
                this.closeModal();
            },
            error: () => alert('Erro ao salvar meta.')
        });
    }

    deleteMeta(id: number) {
        if (!confirm('Tem certeza que deseja excluir esta meta?')) return;

        this.metaService.delete(id).subscribe({
            next: () => {
                this.loadMetas();
            },
            error: () => alert('Erro ao excluir meta.')
        });
    }
}

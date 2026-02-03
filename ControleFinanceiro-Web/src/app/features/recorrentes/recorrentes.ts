import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { RecorrenteService, RecorrenteResponse, RecorrenteRequest } from '../../core/services/recorrente';
import { CategoriaService, CategoriaResponse } from '../../core/services/categoria';

@Component({
    selector: 'app-recorrentes',
    standalone: true,
    imports: [CommonModule, RouterModule, LucideAngularModule, FormsModule],
    templateUrl: './recorrentes.html',
    styleUrl: './recorrentes.scss'
})
export class RecorrentesComponent implements OnInit {
    private recorrenteService = inject(RecorrenteService);
    private categoriaService = inject(CategoriaService);

    currentDate = signal<Date>(new Date());
    recorrentes = signal<RecorrenteResponse[]>([]);
    categorias = signal<CategoriaResponse[]>([]);
    loading = signal<boolean>(true);
    showModal = signal<boolean>(false);
    editMode = signal<boolean>(false);
    editingId = signal<number | null>(null);

    form: RecorrenteRequest = {
        descricao: '',
        valor: 0,
        categoriaId: 0,
        diaVencimento: 1,
        tipo: 'Saida',
        ativo: true
    };

    currentMonthName = computed(() => {
        return this.currentDate().toLocaleString('pt-BR', { month: 'long', year: 'numeric' });
    });

    ngOnInit() {
        this.loadRecorrentes();
        this.loadCategorias();
    }

    loadCategorias() {
        this.categoriaService.getAll().subscribe(data => {
            this.categorias.set(data.filter(c => c.tipo === 'Despesa'));
        });
    }

    loadRecorrentes() {
        this.loading.set(true);
        this.recorrenteService.getAll().subscribe({
            next: (data) => {
                this.recorrentes.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                console.error('Erro ao carregar recorrentes:', err);
                this.loading.set(false);
            }
        });
    }

    toggleAtivo(item: RecorrenteResponse) {
        const originalStatus = item.ativo;
        item.ativo = !item.ativo;

        this.recorrenteService.update(item.id, item).subscribe({
            next: () => {
                // Success
            },
            error: () => {
                item.ativo = originalStatus;
                alert('Erro ao atualizar status.');
            }
        });
    }

    changeMonth(delta: number) {
        const nextDate = new Date(this.currentDate());
        nextDate.setMonth(nextDate.getMonth() + delta);
        this.currentDate.set(nextDate);
    }

    applyToMonth() {
        const month = this.currentDate().getMonth() + 1;
        const year = this.currentDate().getFullYear();

        if (confirm(`Deseja aplicar todas as recorrências ativas para ${this.currentMonthName()}?`)) {
            this.recorrenteService.applyToMonth(month, year).subscribe({
                next: (count) => {
                    if (count === 0) {
                        alert(`Todas as recorrências ativas já foram aplicadas em ${this.currentMonthName()}.`);
                    } else {
                        alert(`${count} transações aplicadas com sucesso em ${this.currentMonthName()}!`);
                    }
                },
                error: () => alert('Erro ao aplicar recorrências.')
            });
        }
    }

    openAddModal() {
        this.editMode.set(false);
        this.editingId.set(null);
        this.form = {
            descricao: '',
            valor: 0,
            categoriaId: this.categorias().length > 0 ? this.categorias()[0].id : 0,
            diaVencimento: 1,
            tipo: 'Saida',
            ativo: true
        };
        this.showModal.set(true);
    }

    openEditModal(item: RecorrenteResponse) {
        this.editMode.set(true);
        this.editingId.set(item.id);
        this.form = {
            descricao: item.descricao,
            valor: item.valor,
            categoriaId: item.categoriaId,
            diaVencimento: item.diaVencimento,
            tipo: item.tipo,
            ativo: item.ativo
        };
        this.showModal.set(true);
    }

    closeModal() {
        this.showModal.set(false);
    }

    save() {
        if (!this.form.descricao || this.form.valor <= 0 || !this.form.categoriaId) {
            alert('Por favor, preencha todos os campos corretamente.');
            return;
        }

        if (this.editMode()) {
            this.recorrenteService.update(this.editingId()!, this.form).subscribe({
                next: () => {
                    this.loadRecorrentes();
                    this.closeModal();
                },
                error: () => alert('Erro ao atualizar.')
            });
        } else {
            this.recorrenteService.create(this.form).subscribe({
                next: () => {
                    this.loadRecorrentes();
                    this.closeModal();
                },
                error: () => alert('Erro ao criar.')
            });
        }
    }

    confirmDelete(item: RecorrenteResponse) {
        if (confirm(`Deseja excluir permanentemente a recorrência "${item.descricao}"?`)) {
            this.recorrenteService.delete(item.id).subscribe({
                next: (success) => {
                    if (success) {
                        this.recorrentes.update(list => list.filter(r => r.id !== item.id));
                    } else {
                        alert('Não foi possível excluir. Tente novamente.');
                    }
                },
                error: () => alert('Erro ao excluir no servidor.')
            });
        }
    }
}

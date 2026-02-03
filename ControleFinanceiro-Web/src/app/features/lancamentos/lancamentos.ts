import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { LancamentoService, LancamentoResponse, LancamentoRequest } from '../../core/services/lancamento';
import { CategoriaService, CategoriaResponse } from '../../core/services/categoria';

@Component({
  selector: 'app-lancamentos',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule],
  templateUrl: './lancamentos.html',
  styleUrl: './lancamentos.scss'
})
export class LancamentosComponent implements OnInit {
  private lancamentoService = inject(LancamentoService);
  private categoriaService = inject(CategoriaService);

  lancamentos = signal<LancamentoResponse[]>([]);
  categorias = signal<CategoriaResponse[]>([]);
  loading = signal<boolean>(false);
  showModal = signal<boolean>(false);

  searchTerm = signal<string>('');
  currentDate = signal<Date>(new Date());

  // Reactive selected type for filtering Categories in Modal
  // It uses "Receita" or "Despesa" to match Categoria.Tipo
  selectedTipo = signal<'Receita' | 'Despesa'>('Despesa');

  // Form handling
  editMode = signal<boolean>(false);
  editingId = signal<number | null>(null);
  form = {
    descricao: '',
    valor: 0,
    data: new Date().toISOString().split('T')[0],
    categoriaId: 0
  };

  formattedMonth = computed(() => {
    const d = this.currentDate();
    const months = [
      'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
      'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
    ];
    return `${months[d.getMonth()]} ${d.getFullYear()}`;
  });

  filteredLancamentos = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const all = this.lancamentos();
    if (!term) return all;
    return all.filter(l =>
      l.descricao.toLowerCase().includes(term) ||
      l.categoriaNome.toLowerCase().includes(term)
    );
  });

  filteredCategorias = computed(() => {
    const tipo = this.selectedTipo();
    return this.categorias().filter(c => c.tipo === tipo);
  });

  ngOnInit() {
    this.loadData();
    this.categoriaService.getAll().subscribe(data => {
      this.categorias.set(data);
    });
  }

  // Map Backend "Entrada/Saida" to Frontend "Receita/Despesa"
  private mapBackendToFrontend(tipo: string): 'Receita' | 'Despesa' {
    return tipo === 'Entrada' ? 'Receita' : 'Despesa';
  }

  // Map Frontend "Receita/Despesa" to Backend "Entrada/Saida"
  private mapFrontendToBackend(tipo: string): string {
    return tipo === 'Receita' ? 'Entrada' : 'Saida';
  }

  loadData() {
    this.loading.set(true);
    const d = this.currentDate();
    const startOfMonth = new Date(d.getFullYear(), d.getMonth(), 1);
    const endOfMonth = new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59);

    this.lancamentoService.getAll(startOfMonth.toISOString(), endOfMonth.toISOString()).subscribe({
      next: (data: LancamentoResponse[]) => {
        // Map types during load for UI consistency
        const mappedData = data.map(l => ({
          ...l,
          tipo: this.mapBackendToFrontend(l.tipo)
        }));
        this.lancamentos.set(mappedData);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openModal(lancamento?: LancamentoResponse) {
    if (lancamento) {
      this.editMode.set(true);
      this.editingId.set(lancamento.id);
      // Lancamento.tipo is already mapped to Receita/Despesa here
      this.selectedTipo.set(lancamento.tipo as 'Receita' | 'Despesa');
      this.form = {
        descricao: lancamento.descricao,
        valor: lancamento.valor,
        data: new Date(lancamento.data).toISOString().split('T')[0],
        categoriaId: lancamento.categoriaId
      };
    } else {
      this.editMode.set(false);
      this.editingId.set(null);
      this.selectedTipo.set('Despesa');
      this.form = {
        descricao: '',
        valor: 0,
        data: new Date().toISOString().split('T')[0],
        categoriaId: 0
      };

      const firstCat = this.filteredCategorias()[0];
      if (firstCat) this.form.categoriaId = firstCat.id;
    }
    this.showModal.set(true);
  }

  onTipoChange(tipo: 'Receita' | 'Despesa') {
    this.selectedTipo.set(tipo);
    this.form.categoriaId = 0;

    // Auto-select first category of new type
    const firstCat = this.filteredCategorias()[0];
    if (firstCat) this.form.categoriaId = firstCat.id;
  }

  closeModal() {
    this.showModal.set(false);
  }

  save() {
    if (!this.form.descricao || this.form.valor <= 0 || !this.form.categoriaId) {
      alert('Por favor, preencha todos os campos obrigatórios.');
      return;
    }

    const request: LancamentoRequest = {
      descricao: this.form.descricao,
      valor: this.form.valor,
      data: new Date(this.form.data + 'T12:00:00').toISOString(),
      tipo: this.mapFrontendToBackend(this.selectedTipo()), // Map to Entrada/Saida for Backend
      categoriaId: this.form.categoriaId
    };

    const action = this.editMode()
      ? this.lancamentoService.update(this.editingId()!, request)
      : this.lancamentoService.create(request);

    (action as any).subscribe({
      next: () => {
        this.loadData();
        this.closeModal();
      },
      error: (err: any) => {
        console.error('Erro ao salvar lançamento:', err);
        alert('Erro ao salvar lançamento. Verifique se todos os campos estão corretos.');
      }
    });
  }

  delete(id: number) {
    if (confirm('Excluir este lançamento?')) {
      this.lancamentoService.delete(id).subscribe(() => {
        this.loadData();
        this.closeModal();
      });
    }
  }

  nextMonth() {
    const current = this.currentDate();
    this.currentDate.set(new Date(current.getFullYear(), current.getMonth() + 1, 1));
    this.loadData();
  }

  prevMonth() {
    const current = this.currentDate();
    this.currentDate.set(new Date(current.getFullYear(), current.getMonth() - 1, 1));
    this.loadData();
  }
}

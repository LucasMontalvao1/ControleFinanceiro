import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { LancamentoService, LancamentoResponse, LancamentoRequest } from '../../core/services/lancamento';
import { CategoriaService, CategoriaResponse } from '../../core/services/categoria';
import { AiService, AiAnalyzeResponse } from '../../core/services/ai';
import { MagicScanComponent } from '../../shared/components/magic-scan/magic-scan.component';

@Component({
  selector: 'app-lancamentos',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule, MagicScanComponent],
  templateUrl: './lancamentos.html',
  styleUrl: './lancamentos.scss'
})
export class LancamentosComponent implements OnInit {
  private lancamentoService = inject(LancamentoService);
  private categoriaService = inject(CategoriaService);
  private aiService = inject(AiService);

  lancamentos = signal<LancamentoResponse[]>([]);
  categorias = signal<CategoriaResponse[]>([]);
  loading = signal<boolean>(false);
  showModal = signal<boolean>(false);
  showMagicScan = signal<boolean>(false);

  searchTerm = signal<string>('');
  currentDate = signal<Date>(new Date());

  selectedTipo = signal<'Receita' | 'Despesa'>('Despesa');

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
    this.refreshCategories();
  }

  private refreshCategories() {
    this.categoriaService.getAll().subscribe(data => {
      const mappedData = data.map(c => ({
        ...c,
        tipo: this.mapBackendToFrontend(c.tipo)
      }));
      this.categorias.set(mappedData);
    });
  }

  private mapBackendToFrontend(tipo: string): 'Receita' | 'Despesa' {
    if (tipo === 'Entrada' || tipo === 'Receita') return 'Receita';
    return 'Despesa';
  }

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

  openMagicScan() {
    this.showMagicScan.set(true);
  }

  handleScanResult(result: any) {
    this.showMagicScan.set(false);

    if (!result.itens || result.itens.length === 0) return;

    if (result.itens.length === 1) {
      const item = result.itens[0];
      this.form.descricao = item.descricao;
      this.form.valor = item.valor;
      this.form.data = item.data || new Date().toISOString().split('T')[0];
      this.selectedTipo.set(item.tipo);
      this.form.categoriaId = item.categoriaId;

      this.editMode.set(false);
      this.editingId.set(null);
      this.showModal.set(true);
    } else {
      this.loading.set(true);
      const requests = result.itens.map((item: any) => ({
        descricao: item.descricao,
        valor: item.valor,
        data: item.data + 'T12:00:00',
        tipo: this.mapFrontendToBackend(item.tipo),
        categoriaId: item.categoriaId
      }));

      let savedCount = 0;
      const saveNext = (index: number) => {
        if (index >= requests.length) {
          this.loadData();
          alert(`${savedCount} lançamentos salvos com sucesso! 🚀`);
          return;
        }

        this.lancamentoService.create(requests[index]).subscribe({
          next: () => {
            savedCount++;
            saveNext(index + 1);
          },
          error: (err) => {
            console.error('Erro ao salvar item da lista:', err);
            saveNext(index + 1);
          }
        });
      };

      saveNext(0);
    }
  }

  handleCreateCategory(data: { nome: string, tipo: string }) {
    // Pre-check if category already exists locally to avoid unnecessary API calls or duplicates
    const alreadyExists = this.categorias().some(
      c => c.nome.toLowerCase() === data.nome.toLowerCase() && c.tipo === data.tipo
    );

    if (alreadyExists) {
      // Just refresh to trigger the setter in MagicScan if it wasn't matched yet
      this.refreshCategories();
      return;
    }

    this.categoriaService.create({
      nome: data.nome,
      tipo: data.tipo
    }).subscribe({
      next: (newCat) => {
        this.refreshCategories();
        // Success feedback is now handled by the button state and automatic linking in the UI
      },
      error: (err) => {
        console.error('Erro ao criar categoria via Magic Scan:', err);
        alert('Não foi possível criar a categoria automaticamente.');
      }
    });
  }

  onTipoChange(tipo: 'Receita' | 'Despesa') {
    this.selectedTipo.set(tipo);
    this.form.categoriaId = 0;
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
      tipo: this.mapFrontendToBackend(this.selectedTipo()),
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
        alert('Erro ao salvar lançamento.');
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

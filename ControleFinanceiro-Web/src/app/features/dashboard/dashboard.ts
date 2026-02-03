import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { DashboardService, DashboardSummary } from '../../core/services/dashboard';
import { NgApexchartsModule } from 'ng-apexcharts';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, LucideAngularModule, NgApexchartsModule, RouterModule],
    templateUrl: './dashboard.html',
    styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit {
    private dashboardService = inject(DashboardService);
    private router = inject(Router);

    summary = signal<DashboardSummary | null>(null);
    loading = signal<boolean>(true);
    hasData = signal<boolean>(false);
    currentDate = signal<Date>(new Date());

    // Chart configs
    dailyTrendChartOptions: any;
    categoryChartOptions: any;
    yearlyBalanceChartOptions: any;
    yearlyTrendChartOptions: any;

    formattedMonth = computed(() => {
        const d = this.currentDate();
        const months = [
            'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
            'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
        ];
        return `${months[d.getMonth()]} ${d.getFullYear()}`;
    });

    ngOnInit() {
        this.loadSummary();
    }

    loadSummary() {
        this.loading.set(true);
        const d = this.currentDate();
        const startOfMonth = new Date(d.getFullYear(), d.getMonth(), 1);
        const endOfMonth = new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59);

        this.dashboardService.getSummary(startOfMonth.toISOString(), endOfMonth.toISOString()).subscribe({
            next: (data: DashboardSummary) => {
                this.summary.set(data);
                this.hasData.set(data && (data.totalEntradas > 0 || data.totalSaidas > 0));
                this.setupCharts(data);
                this.loading.set(false);
            },
            error: (err: any) => {
                console.error('[Dashboard] Error loading dashboard:', err);
                this.loading.set(false);
            }
        });
    }

    private formatMonthName(isoMonth: string): string {
        if (!isoMonth) return 'N/A';
        const [year, month] = isoMonth.split('-');
        const months = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
        return `${months[parseInt(month) - 1]}/${year.substring(2)}`;
    }

    setupCharts(data: DashboardSummary) {
        if (!data) return;

        const dailyMeta = data.evolucaoMensal || [];
        const yearlyMeta = data.evolucaoAnual || [];

        // 1. DAILY EVOLUTION (Selected Month)
        this.dailyTrendChartOptions = {
            series: [
                { name: 'Receitas', data: dailyMeta.map(t => t.entradas), color: '#10b981' },
                { name: 'Despesas', data: dailyMeta.map(t => t.saidas), color: '#ef4444' }
            ],
            chart: {
                height: 350, type: 'bar', toolbar: { show: false }, background: 'transparent', foreColor: '#94a3b8'
            },
            plotOptions: { bar: { columnWidth: '60%', borderRadius: 4 } },
            dataLabels: {
                enabled: true,
                formatter: (val: number) => val > 0 ? val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 }) : ''
            },
            xaxis: {
                categories: dailyMeta.map(t => new Date(t.data).toLocaleDateString('pt-BR')),
                axisBorder: { show: false }, axisTicks: { show: false },
            },
            yaxis: { labels: { formatter: (val: number) => val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) } },
            tooltip: { theme: 'dark', y: { formatter: (val: number) => val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) } },
            grid: { borderColor: '#1e293b', strokeDashArray: 4 }
        };

        // 2. YEARLY TREND BAR (6 Months) - Grouped bars for Income x Expenses
        this.yearlyTrendChartOptions = {
            series: [
                { name: 'Entradas', data: yearlyMeta.map(y => y.entradas), color: '#10b981' },
                { name: 'Saídas', data: yearlyMeta.map(y => y.saidas), color: '#ef4444' }
            ],
            chart: {
                height: 350, type: 'bar', toolbar: { show: false }, background: 'transparent', foreColor: '#94a3b8'
            },
            plotOptions: { bar: { columnWidth: '50%', borderRadius: 4 } },
            dataLabels: {
                enabled: true,
                formatter: (val: number) => val > 0 ? val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 }) : ''
            },
            xaxis: {
                categories: yearlyMeta.map(y => this.formatMonthName(y.mes)),
                axisBorder: { show: false },
            },
            yaxis: { labels: { formatter: (val: number) => val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) } },
            tooltip: { theme: 'dark', y: { formatter: (val: number) => val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) } },
            grid: { borderColor: '#1e293b', strokeDashArray: 4 }
        };

        // 3. YEARLY BALANCE AREA (6 Months) - Smooth area chart
        this.yearlyBalanceChartOptions = {
            series: [{ name: 'Saldo', data: yearlyMeta.map(y => y.saldo) }],
            chart: {
                height: 350, type: 'area', toolbar: { show: false }, background: 'transparent', foreColor: '#94a3b8'
            },
            stroke: { curve: 'smooth', width: 3 },
            colors: ['#3b82f6'],
            dataLabels: {
                enabled: true,
                formatter: (val: number) => `R$ ${val.toLocaleString('pt-BR', { maximumFractionDigits: 0 })}`,
                style: { colors: ['#3b82f6'] },
                background: { enabled: false }
            },
            xaxis: {
                categories: yearlyMeta.map(y => this.formatMonthName(y.mes)),
                axisBorder: { show: false },
            },
            yaxis: { labels: { formatter: (val: number) => val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) } },
            tooltip: { theme: 'dark', y: { formatter: (val: number) => val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) } },
            fill: {
                type: 'gradient',
                gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.1, stops: [0, 90, 100] }
            },
            grid: { borderColor: '#1e293b', strokeDashArray: 4 }
        };

        // 4. CATEGORY DONUT
        const catLabels = data.gastosPorCategoria.map(c => c.categoria);
        const catValues = data.gastosPorCategoria.map(c => c.valor);
        this.categoryChartOptions = {
            series: catValues,
            chart: { height: 350, type: 'donut', background: 'transparent', foreColor: '#94a3b8' },
            labels: catLabels,
            colors: ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4'],
            plotOptions: {
                pie: {
                    donut: {
                        size: '70%',
                        labels: {
                            show: true,
                            total: {
                                show: true, label: 'Total', color: '#f8fafc',
                                formatter: () => data.totalSaidas.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
                            }
                        }
                    }
                }
            },
            dataLabels: {
                enabled: true,
                formatter: (val: any, opts: any) => opts.w.globals.series[opts.seriesIndex].toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 })
            },
            legend: { position: 'bottom', fontSize: '14px' },
            stroke: { show: false },
            tooltip: { theme: 'dark', y: { formatter: (val: number) => val.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) } }
        };
    }

    nextMonth() {
        const current = this.currentDate();
        this.currentDate.set(new Date(current.getFullYear(), current.getMonth() + 1, 1));
        this.loadSummary();
    }

    prevMonth() {
        const current = this.currentDate();
        this.currentDate.set(new Date(current.getFullYear(), current.getMonth() - 1, 1));
        this.loadSummary();
    }
}

import { CommonModule, DecimalPipe, PercentPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { forkJoin } from 'rxjs';
import { DashboardApiService } from '../../core/api/dashboard-api.service';
import {
  DashboardStatisticsDto,
  TaskCountByPriorityDto,
  TaskCountByStatusDto,
  UserProductivityDto
} from '../../core/models/dtos';
import { TaskItemStatusLabels, TaskPriorityColors, TaskPriorityLabels } from '../../core/models/enums';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    DecimalPipe,
    PercentPipe,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(DashboardApiService);

  readonly stats = signal<DashboardStatisticsDto | null>(null);
  readonly byStatus = signal<TaskCountByStatusDto[]>([]);
  readonly byPriority = signal<TaskCountByPriorityDto[]>([]);
  readonly productivity = signal<UserProductivityDto[]>([]);
  readonly loading = signal(true);

  readonly statusChartType: ChartType = 'doughnut';
  readonly priorityChartType: ChartType = 'bar';

  readonly statusChartData = computed<ChartData<'doughnut'>>(() => ({
    labels: this.byStatus().map(s => TaskItemStatusLabels[s.status]),
    datasets: [{
      data: this.byStatus().map(s => s.count),
      backgroundColor: ['#9E9E9E', '#1E88E5', '#FB8C00', '#43A047']
    }]
  }));

  readonly priorityChartData = computed<ChartData<'bar'>>(() => ({
    labels: this.byPriority().map(p => TaskPriorityLabels[p.priority]),
    datasets: [{
      label: 'Open tasks',
      data: this.byPriority().map(p => p.count),
      backgroundColor: this.byPriority().map(p => TaskPriorityColors[p.priority])
    }]
  }));

  readonly chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } }
  };

  readonly productivityColumns = ['fullName', 'assignedTasks', 'completedTasks', 'overdueTasks', 'completionRate'];

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin({
      stats: this.api.statistics(),
      byStatus: this.api.tasksByStatus(),
      byPriority: this.api.tasksByPriority(),
      productivity: this.api.productivity(10)
    }).subscribe({
      next: ({ stats, byStatus, byPriority, productivity }) => {
        this.stats.set(stats);
        this.byStatus.set(byStatus);
        this.byPriority.set(byPriority);
        this.productivity.set(productivity);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}

import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { Router, RouterLink } from '@angular/router';
import { debounceTime } from 'rxjs';
import { TasksApiService } from '../../../core/api/tasks-api.service';
import { TaskListItemDto } from '../../../core/models/dtos';
import {
  TaskItemStatus,
  TaskItemStatusLabels,
  TaskItemStatusOrder,
  TaskPriority,
  TaskPriorityColors,
  TaskPriorityLabels
} from '../../../core/models/enums';

@Component({
  selector: 'app-tasks-list',
  imports: [
    CommonModule,
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressBarModule
  ],
  templateUrl: './tasks-list.component.html',
  styleUrl: './tasks-list.component.scss'
})
export class TasksListComponent implements OnInit {
  private readonly api = inject(TasksApiService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly displayedColumns = ['title', 'status', 'priority', 'assigneeName', 'dueDate', 'createdAt', 'actions'];

  readonly statuses = TaskItemStatusOrder;
  readonly priorities: TaskPriority[] = [TaskPriority.Low, TaskPriority.Medium, TaskPriority.High, TaskPriority.Critical];
  readonly statusLabels: Record<number, string> = TaskItemStatusLabels;
  readonly priorityLabels: Record<number, string> = TaskPriorityLabels;
  readonly priorityColors: Record<number, string> = TaskPriorityColors;

  readonly filters = this.fb.nonNullable.group({
    search: [''],
    status: [null as TaskItemStatus | null],
    priority: [null as TaskPriority | null],
    isOverdue: [false]
  });

  readonly items = signal<TaskListItemDto[]>([]);
  readonly total = signal(0);
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly sortBy = signal<string | undefined>(undefined);
  readonly sortDescending = signal(false);
  readonly loading = signal(false);

  readonly totalPages = computed(() => Math.ceil(this.total() / this.pageSize()));

  ngOnInit(): void {
    this.filters.valueChanges
      .pipe(debounceTime(250))
      .subscribe(() => { this.pageNumber.set(1); this.load(); });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const f = this.filters.getRawValue();
    this.api.list({
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDescending: this.sortDescending(),
      search: f.search || undefined,
      status: f.status ?? undefined,
      priority: f.priority ?? undefined,
      isOverdue: f.isOverdue || undefined
    }).subscribe({
      next: page => {
        this.items.set(page.items);
        this.total.set(page.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSort(s: Sort): void {
    if (!s.active || s.direction === '') {
      this.sortBy.set(undefined);
    } else {
      this.sortBy.set(s.active);
      this.sortDescending.set(s.direction === 'desc');
    }
    this.load();
  }

  onPage(e: PageEvent): void {
    this.pageNumber.set(e.pageIndex + 1);
    this.pageSize.set(e.pageSize);
    this.load();
  }

  open(task: TaskListItemDto): void {
    this.router.navigate(['/tasks', task.id]);
  }
}

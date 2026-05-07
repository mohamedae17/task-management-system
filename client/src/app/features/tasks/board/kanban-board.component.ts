import { CdkDragDrop, DragDropModule, transferArrayItem } from '@angular/cdk/drag-drop';
import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { TasksApiService } from '../../../core/api/tasks-api.service';
import { TaskListItemDto } from '../../../core/models/dtos';
import {
  TaskItemStatus,
  TaskItemStatusLabels,
  TaskItemStatusOrder,
  TaskPriorityColors,
  TaskPriorityLabels
} from '../../../core/models/enums';
import { SignalRService } from '../../../core/notifications/signalr.service';

@Component({
  selector: 'app-kanban-board',
  imports: [
    CommonModule,
    DatePipe,
    DragDropModule,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule
  ],
  templateUrl: './kanban-board.component.html',
  styleUrl: './kanban-board.component.scss'
})
export class KanbanBoardComponent implements OnInit, OnDestroy {
  private readonly api = inject(TasksApiService);
  private readonly router = inject(Router);
  private readonly signalr = inject(SignalRService);
  private readonly subs = new Subscription();

  readonly columns = TaskItemStatusOrder;
  readonly columnLabels: Record<number, string> = TaskItemStatusLabels;
  readonly priorityLabels: Record<number, string> = TaskPriorityLabels;
  readonly priorityColors: Record<number, string> = TaskPriorityColors;

  readonly tasks = signal<TaskListItemDto[]>([]);
  readonly loading = signal(false);

  readonly columnIds = computed(() => this.columns.map(s => `column-${s}`));

  ngOnInit(): void {
    this.refresh();
    void this.signalr.connect();
    this.subs.add(this.signalr.taskChanged$.subscribe(e => {
      if (['created', 'updated', 'status-changed', 'assigned', 'deleted'].includes(e.changeType)) {
        this.refresh();
      }
    }));
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  refresh(): void {
    this.loading.set(true);
    // Pull a generous page so the whole board renders. Production would page per column.
    this.api.list({ pageNumber: 1, pageSize: 200 }).subscribe({
      next: page => { this.tasks.set(page.items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  byColumn(status: TaskItemStatus): TaskListItemDto[] {
    return this.tasks().filter(t => t.status === status);
  }

  drop(event: CdkDragDrop<TaskListItemDto[]>, target: TaskItemStatus): void {
    const previous = event.previousContainer.data;
    const current = event.container.data;
    const movedTask = previous[event.previousIndex];

    if (event.previousContainer === event.container) {
      // Reordering within a column — purely cosmetic in this version (no per-column ordering yet).
      return;
    }

    transferArrayItem(previous, current, event.previousIndex, event.currentIndex);
    // Optimistically reflect the new status in local state.
    this.tasks.set(this.tasks().map(t =>
      t.id === movedTask.id ? { ...t, status: target } : t
    ));

    this.api.changeStatus(movedTask.id, target).subscribe({
      error: () => this.refresh()
    });
  }

  open(task: TaskListItemDto): void {
    this.router.navigate(['/tasks', task.id]);
  }
}

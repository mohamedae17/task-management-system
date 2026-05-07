import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { CommentsApiService } from '../../../core/api/comments-api.service';
import { LabelsApiService } from '../../../core/api/labels-api.service';
import { TasksApiService } from '../../../core/api/tasks-api.service';
import { UsersApiService } from '../../../core/api/users-api.service';
import { CommentDto, LabelDto, TaskDto, UserSummaryDto } from '../../../core/models/dtos';
import {
  TaskItemStatus,
  TaskItemStatusLabels,
  TaskItemStatusOrder,
  TaskPriority,
  TaskPriorityColors,
  TaskPriorityLabels
} from '../../../core/models/enums';
import { SignalRService } from '../../../core/notifications/signalr.service';

@Component({
  selector: 'app-task-detail',
  imports: [
    CommonModule,
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.scss'
})
export class TaskDetailComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly tasksApi = inject(TasksApiService);
  private readonly commentsApi = inject(CommentsApiService);
  private readonly usersApi = inject(UsersApiService);
  private readonly labelsApi = inject(LabelsApiService);
  private readonly signalr = inject(SignalRService);
  private readonly subs = new Subscription();

  readonly statuses = TaskItemStatusOrder;
  readonly statusLabels: Record<number, string> = TaskItemStatusLabels;
  readonly priorities: TaskPriority[] = [TaskPriority.Low, TaskPriority.Medium, TaskPriority.High, TaskPriority.Critical];
  readonly priorityLabels: Record<number, string> = TaskPriorityLabels;
  readonly priorityColors: Record<number, string> = TaskPriorityColors;

  readonly task = signal<TaskDto | null>(null);
  readonly comments = signal<CommentDto[]>([]);
  readonly assignableUsers = signal<UserSummaryDto[]>([]);
  readonly availableLabels = signal<LabelDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly isNew = signal(false);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    status: [TaskItemStatus.Todo],
    priority: [TaskPriority.Medium],
    dueDate: [null as Date | null],
    assigneeId: [null as string | null],
    labelIds: [[] as string[]]
  });

  readonly commentForm = this.fb.nonNullable.group({
    content: ['', [Validators.required, Validators.maxLength(4000)]]
  });

  ngOnInit(): void {
    this.loadAssignablesAndLabels();

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.isNew.set(true);
      return;
    }

    this.loadTask(id);
    this.loadComments(id);

    void this.signalr.connect().then(() => this.signalr.subscribeToTask(id));
    this.subs.add(this.signalr.taskChanged$.subscribe(e => {
      if (e.taskId === id && e.changeType === 'comment-added') this.loadComments(id);
      if (e.taskId === id && (e.changeType === 'updated' || e.changeType === 'status-changed')) this.loadTask(id);
    }));
  }

  ngOnDestroy(): void {
    const id = this.task()?.id;
    if (id) void this.signalr.unsubscribeFromTask(id);
    this.subs.unsubscribe();
  }

  loadTask(id: string): void {
    this.loading.set(true);
    this.tasksApi.get(id).subscribe({
      next: t => {
        this.task.set(t);
        this.form.patchValue({
          title: t.title,
          description: t.description ?? '',
          status: t.status,
          priority: t.priority,
          dueDate: t.dueDate ? new Date(t.dueDate) : null,
          assigneeId: t.assigneeId ?? null,
          labelIds: t.labels.map(l => l.id)
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadComments(id: string): void {
    this.commentsApi.list(id, 1, 100).subscribe({
      next: page => this.comments.set(page.items)
    });
  }

  loadAssignablesAndLabels(): void {
    this.usersApi.assignable().subscribe({ next: u => this.assignableUsers.set(u) });
    this.labelsApi.list().subscribe({ next: l => this.availableLabels.set(l) });
  }

  save(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    const payload = {
      title: v.title,
      description: v.description || null,
      status: v.status,
      priority: v.priority,
      dueDate: v.dueDate ? v.dueDate.toISOString() : null,
      assigneeId: v.assigneeId,
      parentTaskId: null,
      labelIds: v.labelIds
    };

    this.saving.set(true);
    if (this.isNew()) {
      this.tasksApi.create(payload).subscribe({
        next: t => {
          this.saving.set(false);
          this.snack.open('Task created', 'OK', { duration: 2500 });
          this.router.navigate(['/tasks', t.id]);
        },
        error: () => this.saving.set(false)
      });
    } else if (this.task()) {
      this.tasksApi.update({ taskId: this.task()!.id, ...payload }).subscribe({
        next: t => {
          this.task.set(t);
          this.saving.set(false);
          this.snack.open('Task saved', 'OK', { duration: 2500 });
        },
        error: () => this.saving.set(false)
      });
    }
  }

  delete(): void {
    const t = this.task();
    if (!t) return;
    if (!confirm(`Delete task "${t.title}"?`)) return;
    this.tasksApi.delete(t.id).subscribe({
      next: () => {
        this.snack.open('Task deleted', 'OK', { duration: 2500 });
        this.router.navigate(['/tasks']);
      }
    });
  }

  postComment(): void {
    if (this.commentForm.invalid || !this.task()) return;
    this.commentsApi.create(this.task()!.id, this.commentForm.controls.content.value).subscribe({
      next: c => {
        this.comments.set([...this.comments(), c]);
        this.commentForm.reset({ content: '' });
      }
    });
  }

  deleteComment(c: CommentDto): void {
    if (!confirm('Delete this comment?')) return;
    this.commentsApi.delete(c.id).subscribe({
      next: () => this.comments.set(this.comments().filter(x => x.id !== c.id))
    });
  }
}

import { Routes } from '@angular/router';

export const TASKS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./list/tasks-list.component').then(m => m.TasksListComponent)
  },
  {
    path: 'board',
    loadComponent: () => import('./board/kanban-board.component').then(m => m.KanbanBoardComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./detail/task-detail.component').then(m => m.TaskDetailComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./detail/task-detail.component').then(m => m.TaskDetailComponent)
  }
];

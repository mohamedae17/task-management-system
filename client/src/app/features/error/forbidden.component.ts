import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  template: `
    <div class="container">
      <mat-icon>block</mat-icon>
      <h1>Access denied</h1>
      <p>You don't have permission to view this page.</p>
      <a mat-flat-button color="primary" routerLink="/dashboard">Back to dashboard</a>
    </div>
  `,
  styles: [`
    .container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 64px 24px;
      text-align: center;
      mat-icon { font-size: 64px; width: 64px; height: 64px; color: var(--mat-sys-error); }
      h1 { margin: 0; }
    }
  `]
})
export class ForbiddenComponent {}

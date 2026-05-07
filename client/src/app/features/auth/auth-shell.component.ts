import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-shell',
  imports: [RouterOutlet, MatIconModule],
  template: `
    <div class="auth-shell">
      <div class="auth-card">
        <div class="auth-brand">
          <mat-icon>checklist</mat-icon>
          <span>Task Management</span>
        </div>
        <router-outlet />
      </div>
    </div>
  `,
  styles: [`
    .auth-shell {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background:
        radial-gradient(ellipse at top, color-mix(in srgb, var(--mat-sys-primary) 18%, transparent), transparent 60%),
        var(--mat-sys-surface);
      padding: 24px;
    }
    .auth-card {
      width: 100%;
      max-width: 440px;
      background: var(--mat-sys-surface-container);
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 16px;
      padding: 32px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.12);
    }
    .auth-brand {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      margin-bottom: 24px;
      font-weight: 600;
      font-size: 1.2rem;
      color: var(--mat-sys-primary);
      mat-icon { font-size: 28px; width: 28px; height: 28px; }
    }
  `]
})
export class AuthShellComponent {}

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-forgot-password',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <h2>Reset your password</h2>
    <p class="subtitle">Enter your email and we'll send a reset link.</p>

    @if (sent()) {
      <div class="auth-info">
        If an account with that email exists, a reset link has been sent.
      </div>
      <a mat-flat-button color="primary" routerLink="/auth/login" style="width: 100%; margin-top: 16px;">
        Back to sign in
      </a>
    } @else {
      <form [formGroup]="form" (ngSubmit)="submit()" class="form">
        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" required>
        </mat-form-field>
        <button mat-flat-button color="primary" type="submit" [disabled]="loading() || form.invalid">
          @if (loading()) { <mat-progress-spinner mode="indeterminate" diameter="20" /> } @else { Send reset link }
        </button>
      </form>

      <div class="register-prompt">
        Remember your password?
        <a routerLink="/auth/login">Sign in</a>
      </div>
    }
  `,
  styleUrl: '../login/login.component.scss',
  styles: [`
    .auth-info {
      background: color-mix(in srgb, var(--mat-sys-primary) 12%, transparent);
      color: var(--mat-sys-on-surface);
      padding: 12px 14px;
      border-radius: 8px;
    }
  `]
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly loading = signal(false);
  readonly sent = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]]
  });

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.auth.forgotPassword(this.form.controls.email.value)
      .pipe(
        catchError(() => of(void 0)),
        finalize(() => this.loading.set(false))
      )
      .subscribe(() => this.sent.set(true));
  }
}

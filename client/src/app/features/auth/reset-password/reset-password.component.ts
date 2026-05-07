import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-reset-password',
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
    <h2>Set a new password</h2>
    <p class="subtitle">Use the token sent to your email.</p>

    <form [formGroup]="form" (ngSubmit)="submit()" class="form">
      <mat-form-field appearance="outline">
        <mat-label>User Id</mat-label>
        <input matInput formControlName="userId" required>
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Reset token</mat-label>
        <textarea matInput formControlName="token" rows="3" required></textarea>
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>New password</mat-label>
        <input matInput type="password" formControlName="newPassword" required>
      </mat-form-field>

      <button mat-flat-button color="primary" type="submit" [disabled]="loading() || form.invalid">
        @if (loading()) { <mat-progress-spinner mode="indeterminate" diameter="20" /> } @else { Reset password }
      </button>
    </form>

    <div class="register-prompt">
      <a routerLink="/auth/login">Back to sign in</a>
    </div>
  `,
  styleUrl: '../login/login.component.scss'
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  readonly loading = signal(false);

  readonly form = this.fb.nonNullable.group({
    userId: [this.route.snapshot.queryParams['userId'] ?? '', [Validators.required]],
    token: [this.route.snapshot.queryParams['token'] ?? '', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]]
  });

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    const { userId, token, newPassword } = this.form.getRawValue();
    this.auth.resetPassword(userId, token, newPassword)
      .pipe(
        catchError(err => { throw err; }),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: () => {
          this.snack.open('Password reset. Sign in with your new password.', 'OK', { duration: 4000 });
          this.router.navigateByUrl('/auth/login');
        }
      });
  }
}

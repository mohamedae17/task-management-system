import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { AuthService } from '../../core/auth/auth.service';
import { UsersApiService } from '../../core/api/users-api.service';

@Component({
  selector: 'app-profile',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="tm-page" style="max-width: 720px;">
      <header class="tm-page-header">
        <mat-icon style="font-size: 32px; width: 32px; height: 32px;">account_circle</mat-icon>
        <h1>My profile</h1>
      </header>

      <mat-tab-group>
        <mat-tab label="Profile">
          <mat-card style="margin-top: 16px;">
            <mat-card-content>
              <form [formGroup]="profileForm" (ngSubmit)="saveProfile()" class="form">
                <div class="row">
                  <mat-form-field appearance="outline">
                    <mat-label>First name</mat-label>
                    <input matInput formControlName="firstName" required>
                  </mat-form-field>
                  <mat-form-field appearance="outline">
                    <mat-label>Last name</mat-label>
                    <input matInput formControlName="lastName" required>
                  </mat-form-field>
                </div>
                <mat-form-field appearance="outline">
                  <mat-label>Avatar URL</mat-label>
                  <input matInput formControlName="avatarUrl" placeholder="https://...">
                </mat-form-field>

                <div class="meta">
                  <strong>Email:</strong> {{ user()?.email }}
                </div>
                <div class="meta">
                  <strong>Roles:</strong>
                  <mat-chip-set>
                    @for (r of user()?.roles ?? []; track r) {
                      <mat-chip>{{ r }}</mat-chip>
                    }
                  </mat-chip-set>
                </div>

                <div class="actions">
                  <button mat-flat-button color="primary" type="submit" [disabled]="savingProfile() || profileForm.invalid">
                    @if (savingProfile()) { <mat-progress-spinner diameter="20" mode="indeterminate" /> } @else { Save profile }
                  </button>
                </div>
              </form>
            </mat-card-content>
          </mat-card>
        </mat-tab>

        <mat-tab label="Change password">
          <mat-card style="margin-top: 16px;">
            <mat-card-content>
              <form [formGroup]="passwordForm" (ngSubmit)="changePassword()" class="form">
                <mat-form-field appearance="outline">
                  <mat-label>Current password</mat-label>
                  <input matInput type="password" formControlName="currentPassword" required>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>New password</mat-label>
                  <input matInput type="password" formControlName="newPassword" required>
                  <mat-hint>8+ chars, upper, lower, digit, symbol</mat-hint>
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Confirm new password</mat-label>
                  <input matInput type="password" formControlName="confirm" required>
                  @if (passwordForm.errors?.['mismatch']) {
                    <mat-error>Passwords do not match</mat-error>
                  }
                </mat-form-field>

                <div class="actions">
                  <button mat-flat-button color="primary" type="submit" [disabled]="savingPassword() || passwordForm.invalid">
                    @if (savingPassword()) { <mat-progress-spinner diameter="20" mode="indeterminate" /> } @else { Change password }
                  </button>
                </div>
              </form>
            </mat-card-content>
          </mat-card>
        </mat-tab>
      </mat-tab-group>
    </div>
  `,
  styles: [`
    .form { display: flex; flex-direction: column; gap: 4px; }
    .row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .meta { margin: 8px 0; }
    .actions { display: flex; justify-content: flex-end; margin-top: 8px; }
  `]
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly usersApi = inject(UsersApiService);
  private readonly snack = inject(MatSnackBar);

  readonly user = this.auth.user;
  readonly savingProfile = signal(false);
  readonly savingPassword = signal(false);

  readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    avatarUrl: ['']
  });

  readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [
      Validators.required,
      Validators.minLength(8),
      Validators.pattern(/(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+/)
    ]],
    confirm: ['', [Validators.required]]
  }, { validators: form => form.value.newPassword === form.value.confirm ? null : { mismatch: true } });

  ngOnInit(): void {
    const u = this.user();
    if (u) {
      this.profileForm.patchValue({ firstName: u.firstName, lastName: u.lastName, avatarUrl: u.avatarUrl ?? '' });
    }
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.savingProfile.set(true);
    const v = this.profileForm.getRawValue();
    this.usersApi.updateMe({
      firstName: v.firstName,
      lastName: v.lastName,
      avatarUrl: v.avatarUrl || null
    }).subscribe({
      next: u => {
        this.auth.setUser(u);
        this.savingProfile.set(false);
        this.snack.open('Profile updated', 'OK', { duration: 2500 });
      },
      error: () => this.savingProfile.set(false)
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;
    this.savingPassword.set(true);
    const { currentPassword, newPassword } = this.passwordForm.getRawValue();
    this.auth.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.snack.open('Password changed. Please sign in again.', 'OK', { duration: 4000 });
        this.passwordForm.reset();
      },
      error: () => this.savingPassword.set(false)
    });
  }
}

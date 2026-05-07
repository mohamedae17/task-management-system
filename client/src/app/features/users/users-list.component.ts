import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime } from 'rxjs';
import { UsersApiService } from '../../core/api/users-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { UserDto } from '../../core/models/dtos';
import { AppRole } from '../../core/models/enums';

@Component({
  selector: 'app-users-list',
  imports: [
    CommonModule,
    DatePipe,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatProgressBarModule,
    MatDividerModule,
    MatTooltipModule
  ],
  template: `
    <div class="tm-page">
      <header class="tm-page-header">
        <mat-icon style="font-size: 32px; width: 32px; height: 32px;">group</mat-icon>
        <h1>Users</h1>
      </header>

      <form [formGroup]="filters" class="filters">
        <mat-form-field appearance="outline" subscriptSizing="dynamic" class="search">
          <mat-label>Search</mat-label>
          <input matInput formControlName="search" placeholder="Email or name">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" subscriptSizing="dynamic">
          <mat-label>Role</mat-label>
          <mat-select formControlName="role">
            <mat-option [value]="''">All roles</mat-option>
            @for (r of allRoles; track r) {
              <mat-option [value]="r">{{ r }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
      </form>

      @if (loading()) { <mat-progress-bar mode="indeterminate" /> }

      <div class="table-host">
        <table mat-table [dataSource]="items()">
          <ng-container matColumnDef="fullName">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let u">{{ u.fullName }}</td>
          </ng-container>
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef>Email</th>
            <td mat-cell *matCellDef="let u">{{ u.email }}</td>
          </ng-container>
          <ng-container matColumnDef="roles">
            <th mat-header-cell *matHeaderCellDef>Roles</th>
            <td mat-cell *matCellDef="let u">
              <mat-chip-set>
                @for (r of u.roles; track r) {
                  <mat-chip>{{ r }}</mat-chip>
                }
              </mat-chip-set>
            </td>
          </ng-container>
          <ng-container matColumnDef="lastLoginAt">
            <th mat-header-cell *matHeaderCellDef>Last login</th>
            <td mat-cell *matCellDef="let u">{{ u.lastLoginAt ? (u.lastLoginAt | date: 'short') : '—' }}</td>
          </ng-container>
          <ng-container matColumnDef="isActive">
            <th mat-header-cell *matHeaderCellDef>Active</th>
            <td mat-cell *matCellDef="let u">
              <mat-slide-toggle
                [checked]="u.isActive"
                [disabled]="!isAdmin"
                (change)="toggleActive(u, $event.checked)" />
            </td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let u">
              <button mat-icon-button [matMenuTriggerFor]="menu" [disabled]="!isAdmin">
                <mat-icon>more_vert</mat-icon>
              </button>
              <mat-menu #menu>
                @for (r of allRoles; track r) {
                  <button mat-menu-item (click)="setRole(u, r)">
                    <mat-icon>{{ u.roles.includes(r) ? 'check' : '' }}</mat-icon>
                    Set role: {{ r }}
                  </button>
                }
              </mat-menu>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
        </table>
      </div>

      <mat-paginator
        [length]="total()"
        [pageSize]="pageSize()"
        [pageSizeOptions]="[10, 20, 50]"
        [pageIndex]="pageNumber() - 1"
        (page)="onPage($event)" />
    </div>
  `,
  styles: [`
    .filters { display: flex; gap: 12px; margin-bottom: 16px; .search { flex: 1 1 320px; } }
    .table-host {
      background: var(--mat-sys-surface-container);
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 8px;
      overflow: hidden;
    }
    table { width: 100%; }
  `]
})
export class UsersListComponent implements OnInit {
  private readonly api = inject(UsersApiService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly snack = inject(MatSnackBar);

  readonly displayedColumns = ['fullName', 'email', 'roles', 'lastLoginAt', 'isActive', 'actions'];
  readonly allRoles = [AppRole.Admin, AppRole.Manager, AppRole.Employee];
  readonly isAdmin = this.auth.hasRole(AppRole.Admin);

  readonly filters = this.fb.nonNullable.group({
    search: [''],
    role: ['']
  });

  readonly items = signal<UserDto[]>([]);
  readonly total = signal(0);
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly loading = signal(false);

  ngOnInit(): void {
    this.filters.valueChanges.pipe(debounceTime(250)).subscribe(() => {
      this.pageNumber.set(1);
      this.load();
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const v = this.filters.getRawValue();
    this.api.list({
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      search: v.search || undefined,
      role: v.role || undefined
    }).subscribe({
      next: page => {
        this.items.set(page.items);
        this.total.set(page.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPage(e: PageEvent): void {
    this.pageNumber.set(e.pageIndex + 1);
    this.pageSize.set(e.pageSize);
    this.load();
  }

  toggleActive(u: UserDto, isActive: boolean): void {
    this.api.setActive(u.id, isActive).subscribe({
      next: () => {
        this.snack.open(`User ${isActive ? 'activated' : 'deactivated'}.`, 'OK', { duration: 2500 });
        this.load();
      }
    });
  }

  setRole(u: UserDto, role: string): void {
    this.api.setRoles(u.id, [role]).subscribe({
      next: () => { this.snack.open(`Role updated to ${role}.`, 'OK', { duration: 2500 }); this.load(); }
    });
  }
}

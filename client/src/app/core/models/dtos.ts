import { NotificationType, TaskItemStatus, TaskPriority } from './enums';

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string | null;
  isActive: boolean;
  emailConfirmed: boolean;
  lastLoginAt?: string | null;
  createdAt: string;
  roles: string[];
}

export interface UserSummaryDto {
  id: string;
  fullName: string;
  email?: string | null;
  avatarUrl?: string | null;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
  tokenType: string;
  user: UserDto;
}

export interface LabelDto {
  id: string;
  name: string;
  colorHex: string;
  description?: string | null;
}

export interface TaskListItemDto {
  id: string;
  title: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string | null;
  assigneeId?: string | null;
  assigneeName?: string | null;
  createdAt: string;
}

export interface TaskDto {
  id: string;
  title: string;
  description?: string | null;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string | null;
  startedAt?: string | null;
  completedAt?: string | null;
  assigneeId?: string | null;
  assigneeName?: string | null;
  creatorId: string;
  creatorName: string;
  parentTaskId?: string | null;
  commentCount: number;
  attachmentCount: number;
  labels: LabelDto[];
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateTaskCommand {
  title: string;
  description?: string | null;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate?: string | null;
  assigneeId?: string | null;
  parentTaskId?: string | null;
  labelIds?: string[];
}

export interface UpdateTaskCommand extends CreateTaskCommand {
  taskId: string;
}

export interface CommentDto {
  id: string;
  taskId: string;
  authorId: string;
  authorName: string;
  authorAvatarUrl?: string | null;
  content: string;
  isEdited: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface NotificationDto {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  isRead: boolean;
  readAt?: string | null;
  relatedEntityType?: string | null;
  relatedEntityId?: string | null;
  actionUrl?: string | null;
  createdAt: string;
}

export interface DashboardStatisticsDto {
  totalTasks: number;
  openTasks: number;
  completedTasks: number;
  overdueTasks: number;
  dueThisWeek: number;
  myAssignedTasks: number;
  myOpenTasks: number;
  unreadNotifications: number;
  totalUsers: number;
  activeUsers: number;
}

export interface TaskCountByStatusDto {
  status: TaskItemStatus;
  statusName: string;
  count: number;
}

export interface TaskCountByPriorityDto {
  priority: TaskPriority;
  priorityName: string;
  count: number;
}

export interface UserProductivityDto {
  userId: string;
  fullName: string;
  assignedTasks: number;
  completedTasks: number;
  overdueTasks: number;
  completionRate: number;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

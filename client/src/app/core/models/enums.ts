export enum TaskItemStatus {
  Todo = 1,
  InProgress = 2,
  Review = 3,
  Completed = 4
}

export const TaskItemStatusLabels: Record<TaskItemStatus, string> = {
  [TaskItemStatus.Todo]: 'To Do',
  [TaskItemStatus.InProgress]: 'In Progress',
  [TaskItemStatus.Review]: 'Review',
  [TaskItemStatus.Completed]: 'Completed'
};

export const TaskItemStatusOrder: TaskItemStatus[] = [
  TaskItemStatus.Todo,
  TaskItemStatus.InProgress,
  TaskItemStatus.Review,
  TaskItemStatus.Completed
];

export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export const TaskPriorityLabels: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'Low',
  [TaskPriority.Medium]: 'Medium',
  [TaskPriority.High]: 'High',
  [TaskPriority.Critical]: 'Critical'
};

export const TaskPriorityColors: Record<TaskPriority, string> = {
  [TaskPriority.Low]: '#43A047',
  [TaskPriority.Medium]: '#1E88E5',
  [TaskPriority.High]: '#FB8C00',
  [TaskPriority.Critical]: '#E53935'
};

export enum NotificationType {
  TaskAssigned = 1,
  TaskUpdated = 2,
  TaskCompleted = 3,
  TaskDueSoon = 4,
  TaskOverdue = 5,
  CommentAdded = 6,
  Mentioned = 7,
  SystemAnnouncement = 8
}

export enum AppRole {
  Admin = 'Admin',
  Manager = 'Manager',
  Employee = 'Employee'
}

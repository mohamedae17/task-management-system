import { NotificationDto } from '../../core/models/dtos';

export interface NotificationsState {
  items: NotificationDto[];
  unreadCount: number;
  loading: boolean;
}

export const initialNotificationsState: NotificationsState = {
  items: [],
  unreadCount: 0,
  loading: false
};

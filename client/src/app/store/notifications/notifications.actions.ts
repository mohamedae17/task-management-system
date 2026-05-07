import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { NotificationDto } from '../../core/models/dtos';

export const NotificationsActions = createActionGroup({
  source: 'Notifications',
  events: {
    'Load': emptyProps(),
    'Load Success': props<{ items: NotificationDto[]; unreadCount: number }>(),
    'Load Failure': props<{ error: string }>(),

    'Received Live': props<{ item: NotificationDto }>(),

    'Mark Read': props<{ id: string }>(),
    'Mark Read Success': props<{ id: string }>(),

    'Mark All Read': emptyProps(),
    'Mark All Read Success': emptyProps()
  }
});

import { createFeature, createReducer, on } from '@ngrx/store';
import { NotificationsActions } from './notifications.actions';
import { initialNotificationsState } from './notifications.state';

export const notificationsFeature = createFeature({
  name: 'notifications',
  reducer: createReducer(
    initialNotificationsState,
    on(NotificationsActions.load, state => ({ ...state, loading: true })),
    on(NotificationsActions.loadSuccess, (state, { items, unreadCount }) =>
      ({ ...state, items, unreadCount, loading: false })),
    on(NotificationsActions.loadFailure, state => ({ ...state, loading: false })),
    on(NotificationsActions.receivedLive, (state, { item }) => ({
      ...state,
      items: [item, ...state.items.filter(i => i.id !== item.id)].slice(0, 100),
      unreadCount: item.isRead ? state.unreadCount : state.unreadCount + 1
    })),
    on(NotificationsActions.markReadSuccess, (state, { id }) => {
      const target = state.items.find(i => i.id === id);
      const wasUnread = target && !target.isRead;
      return {
        ...state,
        items: state.items.map(i => i.id === id ? { ...i, isRead: true, readAt: new Date().toISOString() } : i),
        unreadCount: wasUnread ? Math.max(0, state.unreadCount - 1) : state.unreadCount
      };
    }),
    on(NotificationsActions.markAllReadSuccess, state => ({
      ...state,
      items: state.items.map(i => ({ ...i, isRead: true, readAt: new Date().toISOString() })),
      unreadCount: 0
    }))
  )
});

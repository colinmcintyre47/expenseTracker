import { Alert } from '../types';
import client from './client';

export const alertsApi = {
  getAll: (unreadOnly = false) =>
    client.get<Alert[]>('/alerts', { params: { unreadOnly } }).then(r => r.data),

  markRead: (id: number) =>
    client.put(`/alerts/${id}/read`),

  markAllRead: () =>
    client.put('/alerts/read-all'),
};

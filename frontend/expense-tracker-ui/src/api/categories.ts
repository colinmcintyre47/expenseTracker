import { Category } from '../types';
import client from './client';

export const categoriesApi = {
  getAll: () =>
    client.get<Category[]>('/categories').then(r => r.data),

  create: (data: { name: string; color: string; icon: string }) =>
    client.post<Category>('/categories', data).then(r => r.data),

  update: (id: number, data: { name: string; color: string; icon: string }) =>
    client.put<Category>(`/categories/${id}`, data).then(r => r.data),

  delete: (id: number) =>
    client.delete(`/categories/${id}`),
};

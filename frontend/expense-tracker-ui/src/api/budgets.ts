import { Budget, CreateBudgetRequest } from '../types';
import client from './client';

export const budgetsApi = {
  getAll: (year?: number, month?: number) =>
    client.get<Budget[]>('/budgets', { params: { year, month } }).then(r => r.data),

  create: (data: CreateBudgetRequest) =>
    client.post<Budget>('/budgets', data).then(r => r.data),

  update: (id: number, monthlyLimit: number) =>
    client.put<Budget>(`/budgets/${id}`, { monthlyLimit }).then(r => r.data),

  delete: (id: number) =>
    client.delete(`/budgets/${id}`),
};

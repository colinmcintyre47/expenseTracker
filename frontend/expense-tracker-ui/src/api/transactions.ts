import { PagedResult, Transaction, TransactionFilter } from '../types';
import client from './client';

export const transactionsApi = {
  getAll: (filter: TransactionFilter = {}) =>
    client.get<PagedResult<Transaction>>('/transactions', { params: filter }).then(r => r.data),

  getById: (id: number) =>
    client.get<Transaction>(`/transactions/${id}`).then(r => r.data),

  updateCategory: (id: number, categoryId: number) =>
    client.put<Transaction>(`/transactions/${id}/category`, { categoryId }).then(r => r.data),

  delete: (id: number) =>
    client.delete(`/transactions/${id}`),
};

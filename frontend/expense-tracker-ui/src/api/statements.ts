import { UploadedStatement, UploadResult } from '../types';
import client from './client';

export const statementsApi = {
  getAll: () =>
    client.get<UploadedStatement[]>('/statements').then(r => r.data),

  upload: (file: File, bankName: string, statementYear?: number) => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('bankName', bankName);
    if (statementYear) formData.append('statementYear', String(statementYear));
    // Use multipart/form-data for file upload (axios sets this header automatically with FormData)
    return client.post<UploadResult>('/statements/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then(r => r.data);
  },
};

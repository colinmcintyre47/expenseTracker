import { AuthResponse, LoginRequest, RegisterRequest } from '../types';
import client from './client';

export const authApi = {
  register: (data: RegisterRequest) =>
    client.post<AuthResponse>('/auth/register', data).then(r => r.data),

  login: (data: LoginRequest) =>
    client.post<AuthResponse>('/auth/login', data).then(r => r.data),
};

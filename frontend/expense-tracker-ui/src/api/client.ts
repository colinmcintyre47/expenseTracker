import axios from 'axios';

/**
 * Centralized Axios instance.
 *
 * All API calls go through this client so that:
 * 1. The base URL is configured in one place
 * 2. JWT tokens are automatically added to every request
 * 3. 401 responses automatically redirect to login
 */
const apiClient = axios.create({
  baseURL: '/api',  // Vite proxies /api/* to the backend (see vite.config.ts)
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Request interceptor — adds the JWT token to every outgoing request.
 * The token is read from localStorage where AuthContext stores it.
 */
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Response interceptor — handles authentication errors globally.
 * If the server returns 401 Unauthorized, clear auth state and redirect to login.
 */
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid — force re-login
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;

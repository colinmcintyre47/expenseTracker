import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

/*
 * Vite configuration.
 * The proxy setting forwards /api/* requests to the backend during development,
 * so we don't need to hardcode the backend URL in every API call.
 * In production, configure your web server (nginx, etc.) to do the same.
 */
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Forward all /api requests to the ASP.NET Core backend
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false, // Allow self-signed certificates in development
      },
    },
  },
});

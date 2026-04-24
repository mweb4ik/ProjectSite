import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const api = axios.create({
    baseURL: `${API_URL}/api`, 
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 10000 
});

// JWT-токен ко всем запросам 
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    
    if (token && typeof token === 'string' && token.trim() !== '') {
        config.headers.Authorization = `Bearer ${token}`;
        console.log('[API] Token found, setting Authorization header');
    } else {
        delete config.headers.Authorization;
         console.warn('[API] No valid token found, Authorization header removed');
    }
    
    return config;
});

// Глобальная обработка ответов сервера
api.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            console.error('[API] 401 Unauthorized - токен недействителен');
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = '/';
        }
        return Promise.reject(error);
    }
);

export async function getUserWithRetry(retries = 3) {
  try {
    return await api.get('/auth/me');
  } catch (e) {
    if (!e.response) {
      if (retries > 0) {
        await new Promise(r => setTimeout(r, 2000));
        return getUserWithRetry(retries - 1);
      }
      throw e;
    }

    if (e.response.status === 401) {
      throw e; // Обработается в интерцепторе 
    }

    if (e.response.status >= 500 && retries > 0) {
      await new Promise(r => setTimeout(r, 2000));
      return getUserWithRetry(retries - 1);
    }

    throw e;
  }
}

export default api;
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const api = axios.create({
    baseURL: `${API_URL}/api`, 
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 10000 
});

// Добавляем токен к запросам
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    
    if (token && typeof token === 'string' && token.trim() !== '') {
        config.headers.Authorization = `Bearer ${token}`;
    } else {
        delete config.headers.Authorization;
    }
    
    return config;
});

// Глобальная обработка ошибок
api.interceptors.response.use(
    response => response,
    error => {
        // Если 401 и это НЕ запрос проверки статуса (/auth/me), то выкидываем
        // Запрос /auth/me обрабатывается внутри компонента (HomePage/LoginPage)
        const isMeRequest = error.config?.url?.includes('/auth/me');
        
        if (error.response?.status === 401 && !isMeRequest) {
            console.error('[API] 401 Unauthorized - сессия завершена');
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            // Редиректим только если мы не на странице логина
            if (window.location.pathname !== '/') {
                window.location.href = '/';
            }
        }
        return Promise.reject(error);
    }
);

export async function getUserWithRetry(retries = 3) {
  try {
    return await api.get('/auth/me');
  } catch (e) {
    // Если ошибка сети - пробуем снова
    if (!e.response) {
      if (retries > 0) {
        await new Promise(r => setTimeout(r, 2000));
        return getUserWithRetry(retries - 1);
      }
      throw e;
    }

    // Если 401 (редирект или нет)
    if (e.response.status === 401) {
      throw e; 
    }

    // Если ошибка сервера (5xx) - пробуем снова
    if (e.response.status >= 500 && retries > 0) {
      await new Promise(r => setTimeout(r, 2000));
      return getUserWithRetry(retries - 1);
    }

    throw e;
  }
}

export default api;
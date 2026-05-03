import axios from 'axios';

export const api = axios.create({
    baseURL:'http://localhost:5124', 
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 10000 
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  // 🔥 Блокируем literal "undefined", "null" и пустые строки
  if (token && typeof token === 'string' && token !== 'undefined' && token !== 'null' && token.trim() !== '') {
    config.headers.Authorization = `Bearer ${token}`;
  } else {
    delete config.headers.Authorization;
  }
  return config;
});

// Глобальная обработка ошибок (оставляем твою логику, она ок)
api.interceptors.response.use(
    response => response,
    error => {
        const isMeRequest = error.config?.url?.includes('/auth/me');
        if (error.response?.status === 401 && !isMeRequest) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
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
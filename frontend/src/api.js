import axios from 'axios';

// Единый HTTP-клиент: базовый URL, JSON-заголовки и общий таймаут.
export const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL + '/api',
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 10000 
});

api.interceptors.request.use((config) => {
  // Перед каждым запросом подставляем JWT, если он валиден и не пустой.
  const token = localStorage.getItem('token');
  if (token && typeof token === 'string' && token !== 'undefined' && token !== 'null' && token.trim() !== '') {
    config.headers.Authorization = `Bearer ${token}`;
  } else {
    delete config.headers.Authorization;
  }
  return config;
});

// Глобальная обработка 401: очищаем локальную сессию и возвращаем пользователя на главную.
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
    // Сетевая ошибка без response: выполняем ограниченное число повторных попыток.
    if (!e.response) {
      if (retries > 0) {
        await new Promise(r => setTimeout(r, 2000));
        return getUserWithRetry(retries - 1);
      }
      throw e;
    }

    // Ошибка авторизации обрабатывается вызывающей стороной.
    if (e.response.status === 401) {
      throw e; 
    }

    // Временные серверные ошибки (5xx) пробуем повторно с паузой.
    if (e.response.status >= 500 && retries > 0) {
      await new Promise(r => setTimeout(r, 2000));
      return getUserWithRetry(retries - 1);
    }

    throw e;
  }
}

export default api;

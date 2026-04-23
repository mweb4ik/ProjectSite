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
        // Явно устанавливаем заголовок, перезаписывая значение, если оно было
        config.headers.Authorization = `Bearer ${token}`;

        console.log('[API] Token found, setting Authorization header');
    } else {
        delete config.headers.Authorization;
         console.warn('[API] No valid token found, Authorization header removed');
    }
    
    return config;
});

export async function getUserWithRetry(retries = 3) {
  try {
    return await api.get('/auth/me');
  } catch (e) {
    // если нет ответа — сеть/сервер умер
    if (!e.response) {
      if (retries > 0) {
        await new Promise(r => setTimeout(r, 2000));
        return getUserWithRetry(retries - 1);
      }
      throw e;
    }

  // если не авторизован 
if (e.response.status === 401) {
  console.error(' [API] 401 Unauthorized! Проверь токен и настройки JWT на сервере.');
  console.error('Ответ сервера:', e.response.data);
  
  // Временно закомментируй удаление токена и редирект
  // localStorage.removeItem('token');
  // localStorage.removeItem('user');
  // window.location.href = '/'; 
  
  throw e;
}

    // только 5xx ретраим
    if (e.response.status >= 500 && retries > 0) {
      await new Promise(r => setTimeout(r, 2000));
      return getUserWithRetry(retries - 1);
    }

    throw e;
  }
}

export default api;
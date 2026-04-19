
import axios from 'axios';

//  URL из .env или оставляем localhost для локальной разработки
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

// настроенный экземпляр axios
export const api = axios.create({
    baseURL: `${API_URL}/api`,  // Все запросы автоматически получат этот префикс
    headers: {
        'Content-Type': 'application/json',
    },
     timeout: 10000 
});

//  JWT-токен ко всем запросам 
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
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

    // если не авторизован — не ретраим
    if (e.response.status === 401) {
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
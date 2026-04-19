
import axios from 'axios';

//  URL из .env или оставляем localhost для локальной разработки
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

// настроенный экземпляр axios
export const api = axios.create({
    baseURL: `${API_URL}/api`,  // Все запросы автоматически получат этот префикс
    headers: {
        'Content-Type': 'application/json',
    },
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
    if (retries > 0) {
      await new Promise(r => setTimeout(r, 2000));
      return getUserWithRetry(retries - 1);
    }
    throw e;
  }
}
export default api;
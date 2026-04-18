
import axios from 'axios';

//  URL из .env или оставляем localhost для локальной разработки
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

// настроенный экземпляр axios
export const api = axios.create({
    baseURL: `${API_URL}/api`,  // Все запросы автоматически получат этот префикс
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true, // Если бэкенд использует куки
});

//  JWT-токен ко всем запросам 
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default api;
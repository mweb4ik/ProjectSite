<template>
  <div id="app">
    <div v-if="!showLogin && !showRegister" class="home">
      <h1>Познай Внутреннее устройство компьютера</h1>
      <img src="/images/pc.png" alt="Компьютер" />
      <div class="auth-buttons">
        <button class="btn btn-primary" @click="showLogin = true">Войти</button>
        <button class="btn btn-secondary" @click="showRegister = true">Зарегистрироваться</button>
      </div>
    </div>

    <div v-if="currentUser" class="user-info">
      <p>Пользователь: {{ currentUser.Username }}!</p>
      <p>Роль: {{ currentUser.Role }}</p>
    </div>

    <div v-if="showLogin" class="auth-card">
      <h2>Вход в систему</h2>
      <div v-if="error" class="error-msg">{{ error }}</div>
      <div class="form-group">
        <label>Login (Email или Username)</label>
        <input v-model="form.Login" type="text" placeholder="email или username" @keyup.enter="submitLogin" />
      </div>
      <div class="form-group">
        <label>Пароль</label>
        <input v-model="form.Password" type="password" placeholder="••••••••" @keyup.enter="submitLogin" />
      </div>
      <button class="btn btn-primary full" :disabled="loading" @click="submitLogin">
        {{ loading ? 'Вход...' : 'Войти' }}
      </button>
      <p class="switch-link">Нет аккаунта? <a @click="switchTo('register')">Зарегистрироваться</a></p>
      <button class="btn btn-ghost" @click="closeAuth">← Назад</button>
      <button class="btn btn-ghost" @click="goToForgot">Забыли пароль?</button>
    </div>

    <div v-if="showRegister" class="auth-card">
      <h2>Регистрация</h2>
      <div v-if="error" class="error-msg">{{ error }}</div>
      <div class="form-group">
        <label>Username</label>
        <input v-model="form.Username" type="text" placeholder="nickname" @keyup.enter="submitRegister" />
      </div>
      <div class="form-group">
        <label>Email</label>
        <input v-model="form.Email" type="email" placeholder="your@email.com" @keyup.enter="submitRegister" />
      </div>
      <div class="form-group">
        <label>Пароль <span class="hint">(минимум 6 символов)</span></label>
        <input v-model="form.Password" type="password" placeholder="••••••••" @keyup.enter="submitRegister" />
      </div>
      <div class="form-group">
        <label>Подтвердите пароль <span class="hint">(минимум 6 символов)</span></label>
        <input v-model="form.ConfirmPassword" type="password" placeholder="••••••••" @keyup.enter="submitRegister" />
      </div>
      <button class="btn btn-primary full" :disabled="loading" @click="submitRegister">
        {{ loading ? 'Регистрация...' : 'Создать аккаунт' }}
      </button>
      <p class="switch-link">Уже есть аккаунт? <a @click="switchTo('login')">Войти</a></p>
      <button class="btn btn-ghost" @click="closeAuth">← Назад</button>
    </div>

    <button class="btn btn-ghost" @click="enterAsGuest">Войти как гость</button>
  </div>
</template>

<script>
import { api } from '@/api';
import '@/assets/styles/pages/auth.css'

export default {
  name: 'LoginPage',
  data() {
    return {
      showLogin: false,
      showRegister: false,
      loading: false,
      error: '',
      form: { Username: '', Login: '', Email: '', Password: '', ConfirmPassword: '' },
      currentUser: null,
      isRefreshing: false 
    };
  },
  mounted() {
    const token = localStorage.getItem('accessToken');
    const user = localStorage.getItem('user');

    if (user && token) {
      try {
        this.currentUser = JSON.parse(user);
        this.verifyToken();
      } catch {
        this.clearAuth();
      }
    }
  },
  methods: {
    clearAuth() {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      this.currentUser = null;
    },

    getAuthHeader() {
      const token = localStorage.getItem('accessToken');
      return token ? { Authorization: `Bearer ${token}` } : {};
    },

    async refreshAccessToken() {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) return false;

      try {
        const res = await api.post('/auth/refresh', { refreshToken });
        
        localStorage.setItem('accessToken', res.data.accessToken);
        localStorage.setItem('refreshToken', res.data.refreshToken);
        
        if (res.data.username) {
          this.currentUser = {
            Username: res.data.username,
            Email: res.data.email,
            Role: res.data.role
          };
          localStorage.setItem('user', JSON.stringify(this.currentUser));
        }
        
        return true;
      } catch (err) {
        console.error('Refresh failed:', err);
        this.clearAuth();
        this.$router.push('/'); 
        return false;
      }
    },


    async apiCall(endpoint, method = 'get', data = null) {
      // Первый попытка
      try {
        const config = {
          method,
          url: endpoint,
          headers: this.getAuthHeader(),
          data
        };
        const res = await api.request(config);
        return res.data;
      } catch (err) {
        // Если 401 — рефреш
        if (err.response?.status === 401 && !this.isRefreshing) {
          this.isRefreshing = true;
          
          const refreshed = await this.refreshAccessToken();
          this.isRefreshing = false;
          
          if (refreshed) {
            const config = {
              method,
              url: endpoint,
              headers: this.getAuthHeader(),
              data
            };
            const res = await api.request(config);
            return res.data;
          }
        }
        throw err;
      }
    },

    switchTo(page) {
      this.error = '';
      this.form = { Username: '', Login: '', Email: '', Password: '', ConfirmPassword: '' };
      this.showLogin = page === 'login';
      this.showRegister = page === 'register';
    },
    closeAuth() {
      this.error = '';
      this.form = { Username: '', Login: '', Email: '', Password: '', ConfirmPassword: '' };
      this.showLogin = false;
      this.showRegister = false;
    },
    goToForgot() {
      this.showLogin = false;
      this.showRegister = false;
      this.$router.push('/forgot-password');
    },

    async submitLogin() {
      if (this.loading) return;
      if (!this.form.Login || !this.form.Password) {
        this.error = 'Заполните все поля';
        return;
      }
      this.loading = true;
      this.error = '';
      try {
        const res = await api.post('/auth/login', {
          Login: this.form.Login,
          Password: this.form.Password
        });
        
        localStorage.setItem('accessToken', res.data.accessToken);
        localStorage.setItem('refreshToken', res.data.refreshToken);
        localStorage.setItem('user', JSON.stringify({
          Username: res.data.username,
          Email: res.data.email,
          Role: res.data.role
        }));
        this.currentUser = {
          Username: res.data.username,
          Email: res.data.email,
          Role: res.data.role
        };
        
        this.$router.push('/home');
      } catch (err) {
        this.error = err.response?.data?.message || 'Ошибка входа';
      } finally {
        this.loading = false;
      }
    },

    async submitRegister() {
      this.loading = true;
      this.error = '';
      if (!this.validatePassword()) {
        this.loading = false;
        return;
      }
      if (!this.form.Username || !this.form.Email || !this.form.Password) {
        this.loading = false;
        this.error = 'Заполните все поля';
        return;
      }
      try {
        const res = await api.post('/auth/register', {
          Username: this.form.Username,
          Email: this.form.Email,
          Password: this.form.Password
        });
        
        localStorage.setItem('accessToken', res.data.accessToken);
        localStorage.setItem('refreshToken', res.data.refreshToken);
        localStorage.setItem('user', JSON.stringify({
          Username: res.data.username,
          Email: res.data.email,
          Role: res.data.role || 'standard'
        }));
        this.currentUser = {
          Username: res.data.username,
          Email: res.data.email,
          Role: res.data.role || 'standard'
        };
        
        this.$router.push('/home');
      } catch (err) {
        this.error = err.response?.data?.message || 'Ошибка регистрации';
      } finally {
        this.loading = false;
      }
    },

    enterAsGuest() {
      const guestUser = { Username: 'Guest', Email: 'guest@local', Role: 'guest' };
      localStorage.setItem('user', JSON.stringify(guestUser));
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      this.currentUser = guestUser;
      this.$router.push('/home');
    },

    async logout() {
      try {
        await api.post('/auth/logout', null, { headers: this.getAuthHeader() });
      } catch (e) {
        console.warn('Logout error (ignored):', e);
      } finally {
        this.clearAuth();
        this.$router.push('/');
      }
    },

    validatePassword() {
      const p = this.form.Password;
      const cp = this.form.ConfirmPassword;
      if (p.length < 6) { this.error = 'Пароль должен быть минимум 6 символов'; return false; }
      if (!/[A-Z]/.test(p)) { this.error = 'Пароль должен содержать хотя бы 1 заглавную букву'; return false; }
      if (!/[0-9]/.test(p)) { this.error = 'Пароль должен содержать хотя бы 1 цифру'; return false; }
      if (p !== cp) { this.error = 'Пароли не совпадают'; return false; }
      return true;
    }
  }
};
</script>
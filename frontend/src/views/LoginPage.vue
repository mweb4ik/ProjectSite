<template>
  <div id="app">
    <!-- Главная страница с кнопками входа -->
    <div v-if="!showLogin && !showRegister" class="home">
      <h1>Познай Внутреннее устройство компьютера</h1>
      <img src="/images/pc.png" alt="Компьютер" />
      <div class="auth-buttons">
        <button class="btn btn-primary" @click="showLogin = true">Войти</button>
        <button class="btn btn-secondary" @click="showRegister = true">Зарегистрироваться</button>
      </div>
    </div>

    <!-- Отображение данных текущего пользователя (если уже вошел) -->
    <div v-if="currentUser" class="user-info">
      <p>Пользователь: {{ currentUser.Username }}!</p>
      <p>Роль: {{ currentUser.Role }}</p>
    </div>

    <!-- Форма входа -->
    <div v-if="showLogin" class="auth-card">
      <h2>Вход в систему</h2>
      <div v-if="error" class="error-msg">{{ error }}</div>

      <div class="form-group">
        <label>Login (Email или Username)</label>
        <!-- Добавлен id и name для корректной работы браузеров -->
        <input 
          id="login-input" 
          name="login"
          v-model="form.Login" 
          type="text" 
          placeholder="email или username" 
          @keyup.enter="submitLogin" 
        />
      </div>
      <div class="form-group">
        <label>Пароль</label>
        <input 
          id="password-input" 
          name="password"
          v-model="form.Password" 
          type="password" 
          placeholder="••••••••" 
          @keyup.enter="submitLogin" 
        />
      </div>

      <button class="btn btn-primary full" :disabled="loading" @click="submitLogin">
        {{ loading ? 'Вход...' : 'Войти' }}
      </button>

      <p class="switch-link">
        Нет аккаунта?
        <a @click="switchTo('register')">Зарегистрироваться</a>
      </p>

      <button class="btn btn-ghost" @click="closeAuth">← Назад</button>
      <button class="btn-ghost link" @click="goToForgot">Забыли пароль?</button>
    </div>

    <!-- Форма регистрации -->
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

      <p class="switch-link">
        Уже есть аккаунт?
        <a @click="switchTo('login')">Войти</a>
      </p>

      <button class="btn btn-ghost" @click="closeAuth">← Назад</button>
    </div>

    <!-- Кнопка входа как гость -->
    <button class="btn btn-ghost" @click="enterAsGuest">
      Войти как гость
    </button>
  </div>
</template>

<script>
import { api } from '@/api';

export default {
  name: 'LoginPage',
  data() {
    return {
      showLogin: false,
      showRegister: false,
      loading: false,
      error: '',
      form: {
        Username: '',
        Login: '',
        Email: '',
        Password: '',
        ConfirmPassword: ''
      },
      currentUser: null
    }
  },
  mounted() {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (token && userStr) {
      try {
        this.currentUser = JSON.parse(userStr);

        if (this.$route.path === '/') {
           this.$router.push('/home').catch(() => {});
        }
      } catch (e) {
        console.error('Ошибка парсинга пользователя', e);
        localStorage.clear();
      }
    }
  },
  methods: {
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
      this.$router.push({ name: 'forgot-password' }).catch(() => {});
    },
    
    // Логика входа
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

        console.log('=== ПОЛНЫЙ ОТВЕТ СЕРВЕРА ===');
        console.log(res.data); 
        console.log('Поле Token:', res.data.Token);
        console.log('Поле token:', res.data.token);
        console.log('===========================');
   
        const token = res.data.Token || res.data.token || res.data.accessToken;
        
        if (!token || token === 'undefined' || token === 'null') {
          throw new Error('Сервер вернул пустой токен. См. консоль (F12).');
        }

        localStorage.setItem('token', String(token));
        localStorage.setItem('user', JSON.stringify({
          Username: res.data.Username || res.data.username,
          Email: res.data.Email || res.data.email,
          Role: res.data.Role || res.data.role
        }));

        console.log('[LOGIN] Success, redirecting...');
        this.$router.push('/home');
        
      } catch (err) {
        console.error('[LOGIN] Error:', err);
        this.error = err.response?.data?.message || err.message || 'Ошибка входа';
      } finally {
        this.loading = false;
      }
    },

    // Логика регистрации
    async submitRegister() {
      this.loading = true;
      this.error = '';
      
      if (!this.validatePassword()) {
        this.loading = false;
        return;
      }
      
      if (!this.form.Username || !this.form.Password || !this.form.Email) {
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

        const token = String(res.data.Token);
        if (!token || token === 'undefined') {
          throw new Error('Сервер вернул некорректный токен');
        }

        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify({
          Username: res.data.Username,
          Email: res.data.Email,
          Role: res.data.Role
        }));

        this.currentUser = {
          Username: res.data.Username,
          Email: res.data.Email,
          Role: res.data.Role
        };

        this.$router.push('/home');
      } catch (err) {
        this.error = err.response?.data?.message || 'Ошибка регистрации';
      } finally {
        this.loading = false;
      }
    },

    enterAsGuest() {
      const guestUser = {
        Username: 'Guest',
        Email: 'guest@local',
        Role: 'guest'
      };
      localStorage.setItem('user', JSON.stringify(guestUser));
      localStorage.removeItem('token'); // У гостя нет токена
      this.$router.push('/home');
    },

    validatePassword() {
      const p = this.form.Password;
      const cp = this.form.ConfirmPassword;

      if (p.length < 6) {
        this.error = 'Пароль должен быть минимум 6 символов';
        return false;
      }
      if (!/[A-Z]/.test(p)) {
        this.error = 'Пароль должен содержать хотя бы 1 заглавную букву';
        return false;
      }
      if (!/[0-9]/.test(p)) {
        this.error = 'Пароль должен содержать хотя бы 1 цифру';
        return false;
      }
      if (p !== cp) {
        this.error = 'Пароли не совпадают';
        return false;
      }
      return true;
    }
  }
}
</script>
<template>
  <div id="app">

    <!-- HOME -->
    <div v-if="!mode" class="home">
      <h1>Познай внутреннее устройство компьютера</h1>
      <img src="/images/pc.png" alt="PC" />

      <div class="auth-buttons">
        <button class="btn btn-primary" @click="mode = 'login'">Войти</button>
        <button class="btn btn-secondary" @click="mode = 'register'">Регистрация</button>
      </div>
    </div>

    <!-- USER INFO -->
    <div v-if="currentUser" class="user-info">
      <p>Пользователь: {{ currentUser.Username }}</p>
      <p>Роль: {{ currentUser.Role }}</p>
    </div>

    <!-- LOGIN -->
    <div v-if="mode === 'login'" class="auth-card">
      <h2>Вход</h2>
      <div v-if="error" class="error-msg">{{ error }}</div>

      <input v-model="form.Login" placeholder="Email или username" @keyup.enter="login" />
      <input v-model="form.Password" type="password" placeholder="Пароль" @keyup.enter="login" />

      <button :disabled="loading" @click="login">
        {{ loading ? 'Вход...' : 'Войти' }}
      </button>

      <p @click="mode = 'register'">Нет аккаунта?</p>
      <button @click="reset">Назад</button>
    </div>

    <!-- REGISTER -->
    <div v-if="mode === 'register'" class="auth-card">
      <h2>Регистрация</h2>
      <div v-if="error" class="error-msg">{{ error }}</div>

      <input v-model="form.Username" placeholder="Username" />
      <input v-model="form.Email" placeholder="Email" />
      <input v-model="form.Password" type="password" placeholder="Пароль" />
      <input v-model="form.ConfirmPassword" type="password" placeholder="Повтор пароля" />

      <button :disabled="loading" @click="register">
        {{ loading ? 'Создание...' : 'Создать аккаунт' }}
      </button>

      <p @click="mode = 'login'">Уже есть аккаунт?</p>
      <button @click="reset">Назад</button>
    </div>

    <!-- GUEST -->
    <button class="btn btn-ghost" @click="guest">
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
      mode: null, // login | register | null
      loading: false,
      error: '',
      currentUser: null,

      form: {
        Login: '',
        Username: '',
        Email: '',
        Password: '',
        ConfirmPassword: ''
      }
    };
  },

  mounted() {
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');

    if (token && user && token !== 'undefined') {
      try {
        this.currentUser = JSON.parse(user);
        this.$router.push('/home').catch(() => {});
      } catch {
        localStorage.clear();
      }
    }
  },

  methods: {

    reset() {
      this.mode = null;
      this.error = '';
      this.form = {
        Login: '',
        Username: '',
        Email: '',
        Password: '',
        ConfirmPassword: ''
      };
    },

    // LOGIN
    async login() {
      if (this.loading) return;

      if (!this.form.Login || !this.form.Password) {
        this.error = 'Заполните поля';
        return;
      }

      this.loading = true;
      this.error = '';

      try {
        const { data } = await api.post('/auth/login', {
          Login: this.form.Login,
          Password: this.form.Password
        });

        const token = data.Token ?? data.token ?? data.accessToken;

        if (!token) throw new Error('Token пустой');

        localStorage.setItem('token', token);

        const user = {
          Username: data.Username ?? data.username,
          Email: data.Email ?? data.email,
          Role: data.Role ?? data.role
        };

        localStorage.setItem('user', JSON.stringify(user));

        this.currentUser = user;
        console.log("Успешная авторизация! Токен сохранен:", localStorage.getItem('token'));
        this.$router.push('/home');

      } catch (e) {
        this.error = e.response?.data?.message || e.message || 'Ошибка входа';
      } finally {
        this.loading = false;
      }
    },

    // REGISTER
    async register() {
      if (this.loading) return;

      if (this.form.Password !== this.form.ConfirmPassword) {
        this.error = 'Пароли не совпадают';
        return;
      }

      this.loading = true;
      this.error = '';

      try {
        const { data } = await api.post('/auth/register', {
          Username: this.form.Username,
          Email: this.form.Email,
          Password: this.form.Password
        });

        const token = data.Token;
        if (!token) throw new Error('Token пустой');

        localStorage.setItem('token', token);
       localStorage.setItem('user', JSON.stringify({
  Username: data.Username ?? data.username,
  Email: data.Email ?? data.email,
  Role: data.Role ?? data.role ?? 'User' 
        }));

        this.currentUser = data;
        console.log("Успешная авторизация! Токен сохранен:", localStorage.getItem('token'));
        this.$router.push('/home');

      } catch (e) {
        this.error = e.response?.data?.message || 'Ошибка регистрации';
      } finally {
        this.loading = false;
      }
    },

    // GUEST
    guest() {
      localStorage.setItem('user', JSON.stringify({
        Username: 'Guest',
        Role: 'guest'
      }));

      localStorage.removeItem('token');
      this.$router.push('/home');
    }
  }
};
</script>
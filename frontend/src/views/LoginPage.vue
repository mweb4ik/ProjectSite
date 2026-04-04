<template>
  <div id="app">
    <!-- Главная с кнопками -->
    <div v-if="!showLogin && !showRegister" class="home">
      <h1>Познай Внутреннее устройство компьютера</h1>
      <img src="/images/pc.png" alt="Компьютер" />
      <div class="auth-buttons">
        <button class="btn btn-primary" @click="showLogin = true">Войти</button>
        <button class="btn btn-secondary" @click="showRegister = true">Зарегистрироваться</button>
      </div>
    </div>
    <div v-if="currentUser" class="user-info">
      <p>Пользоветель: {{ currentUser.Username }}!</p>
      <p>Роль: {{ currentUser.Role }}</p>
    </div>
    <!-- Форма входа -->
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
      <p class="switch-link">
        Нет аккаунта?
        <a @click="switchTo('register')">Зарегистрироваться</a>
      </p>
      <button class="btn btn-ghost" @click="closeAuth">← Назад</button>
      <p class="switch-link">
        <a @click="router.push('/forgot-password')">Забыли пароль?</a>
      </p>
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
      <button class="btn btn-primary full" :disabled="loading" @click="submitRegister">
        {{ loading ? 'Регистрация...' : 'Создать аккаунт' }}
      </button>
      <p class="switch-link">
        Уже есть аккаунт?
        <a @click="switchTo('login')">Войти</a>
      </p>
      <button class="btn btn-ghost" @click="closeAuth">← Назад</button>
    </div>
  </div>
</template>

<script>
import { useRouter } from 'vue-router'

const API = 'http://localhost:5124/api/auth'

export default {
  name: 'LoginPage',
  setup() {
    const router = useRouter()
    return { router }
  },
  data() {
    return {
      showLogin: false,
      showRegister: false,
      showForgotPassword: false,
      loading: false,
      error: '',
      form: {
        Username: '',
        Login: '',
        Email: '',
        Password: ''
      },
      currentUser: null 
    }
  },
  mounted() {
  const token = localStorage.getItem('token')
  const user = localStorage.getItem('user')

  if (token && user) {
    this.currentUser = JSON.parse(user)
    this.router.push('/home')
  }
},
  methods: {
    switchTo(page) {
      this.error = ''
      this.form = { Username: '', Login: '', Email: '', Password: '' }
      this.showLogin = page === 'login'
      this.showRegister = page === 'register'
      this.showForgotPassword = page === 'password-reset'
    },
    closeAuth() {
      this.error = ''
      this.form = { Username: '', Login: '', Email: '', Password: '' }
      this.showLogin = false
      this.showRegister = false
      this.showForgotPassword = false
    },
    async submitLogin() {
  if (!this.form.Login || !this.form.Password) {
    this.error = 'Заполните все поля'
    return
  }

  this.loading = true
  this.error = ''

  try {
    const res = await fetch(`${API}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        Login: this.form.Login,
        Password: this.form.Password
      })
    })

    const data = await res.json()  

    if (!res.ok) {
      this.error = data.message || 'Ошибка входа'
      return
    }

    localStorage.setItem('token', data.token)
    localStorage.setItem('user', JSON.stringify({
      Username: data.Username,
      Email: data.Email,
      Role: data.Role
    }))

    this.currentUser = {
      Username: data.Username,
      Email: data.Email,
      Role: data.Role
    }

    this.router.push('/home')

  } catch {
    this.error = 'Сервер недоступен. Убедитесь, что backend запущен.'
  } finally {
    this.loading = false
  }
},
async submitRegister() {
  if (!this.form.Username || !this.form.Password || !this.form.Email) {
    this.error = 'Заполните все поля'
    return
  }

  this.loading = true
  this.error = ''

  try {
    const res = await fetch(`${API}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        Username: this.form.Username,
        Email: this.form.Email,
        Password: this.form.Password
      })
    })

    const data = await res.json() 

    if (!res.ok) {
      this.error = data.message || 'Ошибка входа'
      return
    }

    localStorage.setItem('token', data.token)
    localStorage.setItem('user', JSON.stringify({
      Username: data.Username,
      Email: data.Email,
      Role: data.Role
    }))

    this.currentUser = {
      Username: data.Username,
      Email: data.Email,
      Role: data.Role
    }

    this.router.push('/home')

  } catch {
    this.error = 'Сервер недоступен. Убедитесь, что backend запущен.'
  } finally {
    this.loading = false
  }
}
  }
}
</script>
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
      <p>Пользователь: {{ currentUser.Username }}!</p>
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

    <!-- Гость -->
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
        ConfirmPassword:''
      },
      currentUser: null
    }
  },
  mounted() {
    const token = localStorage.getItem('token')
    const user = localStorage.getItem('user')

    if (token && user) {
      this.currentUser = JSON.parse(user)
      this.$router.push('/home')
    }
    
  },
  methods: {
    switchTo(page) {
      this.error = ''
      this.form = { Username: '', Login: '', Email: '', Password: '' }
      this.showLogin = page === 'login'
      this.showRegister = page === 'register'
    },
    closeAuth() {
      this.error = ''
      this.form = { Username: '', Login: '', Email: '', Password: '' }
      this.showLogin = false
      this.showRegister = false
    },
    goToForgot() {
  this.showLogin = false
  this.showRegister = false
  this.$router.push({ name: 'forgot-password' }).catch(() => {})
},
    async submitLogin() {
  if (!this.form.Login || !this.form.Password) {
    this.error = 'Заполните все поля'
    return
  }
  this.loading = true
  this.error = ''

  try {
    const res = await api.post('/auth/login', {
      Login: this.form.Login,
      Password: this.form.Password
    })

    localStorage.setItem('token', res.data.token)
    localStorage.setItem('user', JSON.stringify({
      Username: res.data.Username,
      Email: res.data.Email,
      Role: res.data.Role
    }))

    this.currentUser = {
      Username: res.data.Username,
      Email: res.data.Email,
      Role: res.data.Role
    }

    this.$router.push('/home')
  } catch (err) {
    this.error = err.response?.data?.message || 'Ошибка входа'
  } finally {
    this.loading = false
  }
},
    async submitRegister() {
  this.loading = true
  this.error = ''
  
  if (!this.validatePassword()) return
  
  if (!this.form.Username || !this.form.Password || !this.form.Email) {
    this.error = 'Заполните все поля'
    return
  }
  
  try {
    // ✅ Используем api вместо fetch
    const res = await api.post('/auth/register', {
      Username: this.form.Username,
      Email: this.form.Email,
      Password: this.form.Password
    })

    localStorage.setItem('token', res.data.token)
    localStorage.setItem('user', JSON.stringify({
      Username: res.data.Username,
      Email: res.data.Email,
      Role: res.data.Role
    }))

    this.currentUser = {
      Username: res.data.Username,
      Email: res.data.Email,
      Role: res.data.Role
    }

    this.$router.push('/home')
  } catch (err) {
    this.error = err.response?.data?.message || 'Ошибка регистрации'
  } finally {
    this.loading = false
  }
},
    enterAsGuest() {
      const guestUser = {
        Username: 'Guest',
        Email: 'guest@local',
        Role: 'guest'
      }
      localStorage.setItem('user', JSON.stringify(guestUser))
      localStorage.removeItem('token')
      this.$router.push('/home')
    },
    validatePassword() {
  const p = this.form.Password
  const cp = this.form.ConfirmPassword

  if (p.length < 6) {
    this.error = 'Пароль должен быть минимум 6 символов'
    return false
  }

  if (!/[A-Z]/.test(p)) {
    this.error = 'Пароль должен содержать хотя бы 1 заглавную букву'
    return false
  }

  if (!/[0-9]/.test(p)) {
    this.error = 'Пароль должен содержать хотя бы 1 цифру'
    return false
  }

  if (p !== cp) {
    this.error = 'Пароли не совпадают'
    return false
  }

  return true
}
  }
}

</script>
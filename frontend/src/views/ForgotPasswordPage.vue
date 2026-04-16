<template>
  <div id="app">
    <header class="header">
      <h1 class="highlight">Восстановление пароля</h1>
    </header>

    <main class="content">
      <div v-if="loading" class="skeleton-wrapper">
        <div class="skeleton-text" style="width: 70%; height: 25px;"></div>
        <div class="skeleton-text" style="width: 60%; height: 25px;"></div>
        <div class="skeleton-btn" style="height: 45px; margin-top: 20px;"></div>
      </div>

      <div v-else class="auth-card">
        <p>Введите ваш email для восстановления пароля</p>
        <input v-model="Email" placeholder="example@email.com" class="input-field" />
        <button class="btn btn-primary full" @click="submitForgot" :disabled="loading">
          {{ loading ? 'Отправка...' : 'Подтвердить' }}
        </button>
        <p v-if="error" class="error-msg">{{ error }}</p>
      </div>
    </main>
  </div>
</template>

<script>
const API = 'http://localhost:5124/api/auth'

export default {
  name: 'ForgotPasswordPage',
  data() {
    return {
      Email: '',
      loading: false,
      error: ''
    }
  },
  methods: {
    async submitForgot() {
      if (!this.Email) {
        this.error = 'Введите email'
        return
      }

      this.loading = true
      this.error = ''

      try {
        const res = await fetch(`${API}/forgot-password`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ Email:this.Email  })
        })

        if (!res.ok) {
          this.error = 'Ошибка отправки'
          return
        }

        alert('Письмо отправлено')

      } catch (err) {
        console.error(err)
        this.error = 'Сервер недоступен'
      } finally {
        this.loading = false
      }
    }
  }
}
</script>

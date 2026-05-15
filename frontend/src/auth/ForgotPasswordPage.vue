<template>
  <div id="app">
    <div class="auth-card">
      <h2>Восстановление пароля</h2>

      <div v-if="error" class="error-msg">{{ error }}</div>

      <div class="form-group">
        <label>Email</label>
        <input
          v-model="email"
          type="email"
          placeholder="example@mail.com"
          @keyup.enter="submitForgot"
        />
      </div>

      <button
        class="btn btn-primary full"
        :disabled="loading"
        @click="submitForgot"
      >
        {{ loading ? 'Отправка...' : 'Отправить' }}
      </button>

      <p v-if="message" class="success-msg">{{ message }}</p>

      <button class="btn btn-ghost" @click="goBack">
        ← Назад
      </button>
    </div>
  </div>
</template>

<script>
import api from '@/api'
import '@/assets/styles/pages/auth.css'

export default {
  name: 'ForgotPasswordPage',

  data() {
    return {
      email: '',
      loading: false,
      error: '',
      message: ''
    }
  },

  methods: {
    goBack() {
      this.$router.push('/')
    },

    async submitForgot() {
      this.error = ''
      this.message = ''

      if (!this.email.trim()) {
        this.error = 'Введите email'
        return
      }

      this.loading = true

      try {
        const res = await api.post('/auth/forgot-password', {
          Email: this.email
        })

        console.log('[FORGOT]', res.data)

        this.message = 'Ссылка создана (проверь backend консоль)'

      } catch (err) {
        this.error =
          err.response?.data?.message ||
          'Ошибка запроса'
      } finally {
        this.loading = false
      }
    }
  }
}
</script>
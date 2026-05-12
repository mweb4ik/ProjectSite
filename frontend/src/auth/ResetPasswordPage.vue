<template>
  <div id="app">
    <div class="auth-card">
      <h2>Сброс пароля</h2>

      <div v-if="error" class="error-msg">{{ error }}</div>

      <div class="form-group">
        <label>Новый пароль</label>
        <input
          v-model="newPassword"
          type="password"
          placeholder="••••••••"
          @keyup.enter="submitReset"
        />
      </div>

      <button
        class="btn btn-primary full"
        :disabled="loading"
        @click="submitReset"
      >
        {{ loading ? 'Сброс...' : 'Сбросить пароль' }}
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
  name: 'ResetPasswordPage',

  data() {
    return {
      token: '',
      newPassword: '',
      loading: false,
      error: '',
      message: ''
    }
  },

  mounted() {
    this.token = this.$route.query.token || ''
  },

  methods: {
    goBack() {
      this.$router.push('/')
    },

    async submitReset() {
      this.error = ''
      this.message = ''

      if (!this.token) {
        this.error = 'Нет токена'
        return
      }

      if (!this.newPassword.trim()) {
        this.error = 'Введите пароль'
        return
      }

      this.loading = true

      try {
        const res = await api.post('/auth/reset-password', {
          Token: this.token,
          NewPassword: this.newPassword
        })

        console.log('[RESET]', res.data)

        this.message = 'Пароль изменён'

        setTimeout(() => {
          this.$router.push('/')
        }, 1500)

      } catch (err) {
        this.error =
          err.response?.data?.message ||
          'Ошибка сброса'
      } finally {
        this.loading = false
      }
    }
  }
}
</script>
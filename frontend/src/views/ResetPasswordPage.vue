<template>
  <div id="app">
    <header class="header">
      <h1 class="highlight">Сброс пароля</h1>
    </header>

    <main class="content">
      <div v-if="loading" class="skeleton-wrapper">
        <div class="skeleton-text" style="width: 70%; height: 25px;"></div>
        <div class="skeleton-text" style="width: 60%; height: 25px;"></div>
        <div class="skeleton-btn" style="height: 45px; margin-top: 20px;"></div>
      </div>

      <div v-else class="auth-card">
        <p>Введите новый пароль для вашего аккаунта</p>
        <input v-model="form.Token" placeholder="Токен из письма" class="input-field" />
        <input v-model="form.NewPassword" placeholder="Новый пароль" class="input-field" />
        <button class="btn btn-primary full" @click="submitReset" :disabled="loading">
          {{ loading ? 'Отправка...' : 'Сбросить пароль' }}
        </button>
        <p v-if="error" class="error-msg">{{ error }}</p>
      </div>
    </main>
  </div>
</template>

<script>
const API = 'http://localhost:5124/api/auth'

export default {
  name: 'ResetPasswordPage',
  data() {
    return {
      loading: false,
      error: '',
      form: {
        Token: '',
        NewPassword: ''
      }
    }
  },
  mounted() {
  const token = this.$route.query.token
  if (token && token.trim() !== '') {
    this.form.Token = token

  }
},
  methods: {
    async submitReset() {
      if (!this.form.Token || !this.form.NewPassword) {
        this.error = 'Заполните все поля'
        return
      }

      this.loading = true
      this.error = ''

      try {
        const res = await fetch(`${API}/reset-password`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            Token: this.form.Token,
            NewPassword: this.form.NewPassword
          })
        })

        if (!res.ok) {
          this.error = 'Ошибка сброса'
          return
        }
        const data = await res.json()
        alert('Пароль успешно обновлён')
        this.$router.push('/home')
        localStorage.setItem('token', data.token)
        localStorage.setItem('user', JSON.stringify(data))
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


<template>
  <div class="profile-page">
    <AppHeader :user="user" @logout="logout" />
    <header class="profile-header">
      <h1>👤 Личный кабинет</h1>
    </header>

    <main class="profile-content">
      <div v-if="loading" class="skeleton-wrapper">
        <div class="skeleton-text"></div>
        <div class="skeleton-text"></div>
        <div class="skeleton-text"></div>
      </div>

      <div v-else class="profile-card">
        <p><span class="label">Username:</span> {{ user.Username }}</p>
        <p><span class="label">Email:</span> {{ user.Email }}</p>
        <p><span class="label">Role:</span> {{ user.Role }}</p>

        <button class="btn-logout" @click="logout">Выйти</button>
      </div>
    </main>
  </div>
</template>

<script>
const API = 'http://localhost:5124/api/auth'
import AppHeader from '@/components/AppHeader.vue'
export default {
  name: 'ProfilePage',
  data() {
    return {
      user: { Username: '', Email: '', Role: '' },
      loading: true
    }
  },
  async mounted() {
    const token = localStorage.getItem('token')

    if (!token) {
      this.$router.push('/')
      return
    }

    const savedUser = localStorage.getItem('user')
    if (savedUser) {
      try {
        this.user = JSON.parse(savedUser)
      } catch {}
    }

    try {
      const res = await fetch(`${API}/me`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      if (!res.ok) throw new Error('Не удалось получить данные пользователя')
      const data = await res.json()
      this.user.Username = data.Username || data.username
      this.user.Email = data.Email || data.email
      this.user.Role = data.Role || data.role
      localStorage.setItem('user', JSON.stringify(this.user))
    } catch (err) {
      console.error(err)
      this.user = { Username: 'Гость', Email: 'guest@local', Role: 'guest' }
    } finally {
      this.loading = false
    }
  },
  methods: {
    logout() {
      localStorage.removeItem('user')
      localStorage.removeItem('token')
      this.$router.push('/')
    }
  }
}
</script>


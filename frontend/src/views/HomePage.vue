<template>
  <div id="app">
    <header class="header">
      <h1>Познай Внутреннее устройство компьютера</h1>
      <div class="user-info">
        <span class="user-email">{{ user.email }}</span>
        <span class="role-badge" :class="user.role">{{ user.role }}</span>
        <button class="btn btn-outline" @click="logout">Выйти</button>
      </div>
    </header>

    <main class="content">
      <img src="/images/pc.png" alt="Компьютер" class="hero-img" />
      <p class="welcome-text">Добро пожаловать, <span class="highlight">{{ user.email }}</span>!</p>
      <p class="subtitle">Выберите компонент для изучения</p>
    </main>
  </div>
</template>

<script>
import { useRouter } from 'vue-router'

export default {
  name: 'HomePage',
  setup() {
    const router = useRouter()
    return { router }
  },
  data() {
    return {
      user: { email: '', role: '' }
    }
  },
  mounted() {
    const token = localStorage.getItem('token')
    if (!token) {
      this.router.push('/')
      return
    }
    const saved = localStorage.getItem('user')
    if (saved) this.user = JSON.parse(saved)
  },
  methods: {
    logout() {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      this.router.push('/')
    }
  }
}
</script>

<style scoped>
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
  padding: 24px 0;
  border-bottom: 1px solid rgba(0, 163, 255, 0.2);
  margin-bottom: 40px;
}
.header h1 {
  font-family: 'Orbitron', sans-serif;
  font-size: clamp(16px, 2.5vw, 28px);
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}
.user-email { color: #aaa; font-size: 14px; }

.hero-img {
  max-width: 50%;
  border: 2px solid #00A3FF;
  border-radius: 12px;
  margin: 0 auto 30px;
  display: block;
  box-shadow: 0 0 30px rgba(0, 163, 255, 0.5);
  transition: all 0.3s ease;
}
.hero-img:hover {
  transform: translateY(4px);
  box-shadow: 0 0 30px rgba(0, 255, 157, 0.1);
}

.welcome-text {
  font-size: 22px;
  margin-bottom: 10px;
}
.subtitle { color: #888; font-size: 16px; }
.highlight { color: #00FF9D; font-weight: 600; }
</style>

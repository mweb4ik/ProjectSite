<template>
  <div id="app">
    <AppHeader :user="user" @logout="logout" />
   
    <main class="content">
      <!-- Скелетон загрузки -->
      <div v-if="loading" class="skeleton-wrapper">
        <div class="skeleton-img"></div>
        <div class="skeleton-text"></div>
        <div class="skeleton-text small"></div>
        <div class="skeleton-buttons">
          <div v-for="i in 6" :key="i" class="skeleton-btn"></div>
        </div>
      </div>

      <!-- Основной контент -->
      <div v-else>
        <img src="/images/pc.png" alt="Компьютер" class="hero-img" />
        <p class="welcome-text">
          Добро пожаловать, <span class="highlight">{{ user.Username }}</span>
        </p>
        <p class="subtitle">Выберите компонент для изучения</p>

        <div class="buttons-grid">
          <button class="btn-component green" @click="goTo('videocard')">📟 Видеокарта</button>
          <button class="btn-component red" @click="goTo('processor')">🔲 Процессор</button>
          <button class="btn-component blue" @click="goTo('motherboard')">🀆 Материнская плата</button>
          <button class="btn-component yellow" @click="goTo('cooling')">𖣘 Охлаждение</button>
          <button class="btn-component yellow" @click="goTo('ram')">𝐑𝐚𝐦 Оперативная память</button>
          <button class="btn-component dark" @click="goTo('storage')">🗄️ Накопитель</button>
        </div>

        <div class="sections-grid">
          <p class="subtitle">Или выберите раздел:</p>
          <button class="btn-section" @click="goTo('lab')">⚡ Лаборатория разгона</button>
          <button class="btn-section" @click="goTo('quiz')">🧠 Викторина</button>
          <button class="btn-section" @click="goTo('builder')">🛠️ Сборка ПК</button>
          <button class="btn-section" @click="goTo('bios')">💾 BIOS / UEFI</button>
          <button class="btn-section" @click="goTo('profile')">👤 Личный кабинет</button>
          <button class="btn-section" @click="goTo('admin')">🛡️ Админ панель</button>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
import { useRouter } from 'vue-router';
import AppHeader from '@/components/AppHeader.vue';
import { components } from '@/data/components';
import { getUserWithRetry } from '@/api';

export default {
  name: 'HomePage',
  components: { AppHeader },

  setup() {
    const router = useRouter();
    return { router };
  },

  data() {
    return {
      user: { Email: '', Role: '', Username: 'Загрузка...' },
      loading: true
    };
  },

  computed: {
    roleClass() {
      if (!this.user.Role) return 'guest-badge';
      switch(this.user.Role.toLowerCase()) {
        case 'admin': return 'admin-badge';
        case 'standard': return 'user-badge';
        default: return 'guest-badge';
      }
    }
  },

  async mounted() {

    const token = localStorage.getItem('token');
    
    if (!token || token === 'undefined' || token === 'null' || token.trim() === '') {
      console.warn('[HOME] No valid token found, redirecting to login');
      this.router.push('/');
      return;
    }

    console.log('[HOME] Token found, length:', token.length);

    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      try {
        const parsed = JSON.parse(savedUser);
        this.user = {
          Email: parsed.Email || '',
          Role: parsed.Role || '',
          Username: parsed.Username || 'Пользователь'
        };
      } catch (e) {
        console.error('[HOME] Error parsing saved user', e);
      }
    }

    try {
      const res = await getUserWithRetry();
      
      this.user = {
        Email: res.data.Email || this.user.Email,
        Role: res.data.Role || this.user.Role,
        Username: res.data.Username || this.user.Username
      };

      localStorage.setItem('user', JSON.stringify(this.user));
      console.log('[HOME] Auth successful, user data updated');
      
    } catch (e) {
      console.error('[HOME] Auth check failed:', e.response?.status, e.response?.data);
      if (e.response?.status === 401) {
        console.warn('[HOME] Token invalid (401). User kept on page for debugging. Please check Network tab.');
        // localStorage.removeItem('token');
        // localStorage.removeItem('user');
        // this.router.push('/');
      } else if (!e.response) {
        console.warn('[HOME] Network error (server down?). Keeping local data.');
      }
    } finally {
      this.loading = false;
    }
  },

  methods: {
    logout() {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      this.router.push('/');
    },

    goTo(page) {
      if (components[page]) {
        this.router.push('/component/' + page);
      } else {
        this.router.push('/' + page);
      }
    },
  }
}
</script>
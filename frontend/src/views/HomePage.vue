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
          Добро пожаловать, {{ user.Username }}
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

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import AppHeader from '@/components/AppHeader.vue';
import { getUserWithRetry } from '@/api';

const router = useRouter();

// Состояние компонента
const user = ref({ Email: '', Role: '', Username: 'Загрузка...', Id: '' });
const loading = ref(true);

// Загрузка данных пользователя при монтировании
onMounted(async () => {
  const savedUser = localStorage.getItem('user');
  if (savedUser) {
    try {
      const parsed = JSON.parse(savedUser);
      user.value = {
        Id: parsed.Id || '',
        Email: parsed.Email || '',
        Role: parsed.Role || '',
        Username: parsed.Username || 'Пользователь'
      };
    } catch (e) {
      console.error('[HOME] Ошибка парсинга сохранённого пользователя', e);
    }
  }

  try {
    const res = await getUserWithRetry();
    
    user.value = {
      Id: res.data.Id || user.value.Id,
      Email: res.data.Email || user.value.Email,
      Role: res.data.Role || user.value.Role,
      Username: res.data.Username || user.value.Username
    };

    localStorage.setItem('user', JSON.stringify(user.value));
    console.log('[HOME] Авторизация успешна, данные пользователя обновлены');
    
  } catch (e) {
    console.error('[HOME] Проверка авторизации не удалась:', e.response?.status, e.response?.data);
  } finally {
    loading.value = false;
  }
});

const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/');
};

const goTo = (page) => {
  const componentPages = ['videocard', 'processor', 'motherboard', 'cooling', 'ram', 'storage'];
  
  if (componentPages.includes(page)) {
    router.push('/component/' + page);
  } else {
    router.push('/' + page);
  }
};
</script>
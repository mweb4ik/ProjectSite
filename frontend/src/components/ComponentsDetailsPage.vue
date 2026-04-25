<template>
  <div class="details-page">
    <AppHeader :user="user" @logout="logout" />
    
    <main class="content" v-if="component">
      <button @click="router.back()" class="btn-back">← Назад</button>
      
      <div class="card-large">
        <div class="info-block">
          <h1>{{ component.name }}</h1>
          <span class="badge">{{ formatCategory(component.category) }}</span>
          <p class="price">{{ component.price }} {{ component.currency }}</p>
        </div>

        <div class="specs-grid">
          <div class="spec-item">
            <strong>Характеристики:</strong>
            <p>{{ component.specifications }}</p>
          </div>
          
          <!-- Динамические подсказки -->
          <div v-if="component.socket" class="spec-item highlight">
            <strong>Сокет:</strong>
            <p>{{ component.socket }}</p>
            <small class="hint">Требуется материнская плата с этим сокетом</small>
          </div>

          <div v-if="component.powerConsumption" class="spec-item">
            <strong>Энергопотребление:</strong>
            <p>{{ component.powerConsumption }} Вт</p>
          </div>
        </div>

        <div class="actions">
          <button class="btn-add" @click="addToBuilder(component)">
            Добавить в сборку
          </button>
        </div>
      </div>
    </main>
    
    <div v-else class="loader">Загрузка...</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import AppHeader from '@/components/AppHeader.vue';
import api from '@/api';

const route = useRoute();
const router = useRouter();
const component = ref(null);
const user = ref(JSON.parse(localStorage.getItem('user') || 'null'));

const loadComponent = async () => {
  try {
    const res = await api.get(`/components/${route.params.id}`);
    component.value = res.data;
  } catch (e) {
    console.error(e);
  }
};

const formatCategory = (cat) => cat ? cat.charAt(0).toUpperCase() + cat.slice(1) : '';

const addToBuilder = (item) => {
  // Пока просто лог, потом реализуем сохранение в localStorage или Pinia
  alert(`Компонент "${item.name}" добавлен в сборку (демо)`);
  // router.push('/builder'); 
};

const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/');
};

onMounted(loadComponent);
</script>


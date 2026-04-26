<template>
  <div class="components-page">
    <AppHeader :user="user" @logout="logout" />

    <main class="content">
      <h1 class="page-title">{{ formatCategoryTitle(currentCategory) }}</h1>

      <div class="search-bar">
        <input 
          v-model="searchQuery" 
          @input="handleSearch" 
          type="text" 
          placeholder="Поиск (RTX, Intel)..." 
          class="search-input"
        />
      </div>

      <div v-if="loading" class="loader">Загрузка...</div>
      <div v-else-if="components.length === 0" class="empty-state">Ничего не найдено</div>

      <div v-else class="components-grid">
        <div v-for="item in components" :key="item.id" class="component-card">
          <div class="card-image-box">
            <img 
              :src="getImageUrl(item.imageUrl)" 
              :alt="item.name"
              class="card-img"
              @error="($event) => $event.target.style.display='none'"
            />
          </div>
          
          <div class="card-content">
            <div class="card-header">
              <span class="badge">{{ item.category }}</span>
              <span class="price">{{ item.price }} {{ item.currency }}</span>
            </div>
            
            <h3 class="card-title">{{ item.name }}</h3>
            
            <div class="card-specs">
              <p><strong>Хар-ки:</strong> {{ item.specifications }}</p>
              <p v-if="item.socket"><strong>Сокет:</strong> {{ item.socket }}</p>
              <p v-if="item.powerConsumption"><strong>Вт:</strong> {{ item.powerConsumption }}</p>
            </div>

            <button class="btn-select" @click="selectComponent(item)">Выбрать</button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import AppHeader from '@/components/AppHeader.vue';
import api from '@/api';

const route = useRoute();
const router = useRouter();

const user = ref(JSON.parse(localStorage.getItem('user') || 'null'));
const components = ref([]);
const loading = ref(false);
const searchQuery = ref('');
const currentCategory = ref('all');

// Формирование URL картинки 
const getImageUrl = (relativePath) => {
  if (!relativePath) return null;
  const baseUrl = api.defaults.baseURL;
  const rootUrl = baseUrl.replace(/\/api$/, '');
  const cleanPath = relativePath.replace(/^wwwroot\//, '');
  return `${rootUrl}/${cleanPath}`;
};

const fetchComponents = async () => {
  loading.value = true;
  try {
    let url = '/components';
    const params = {};

    switch (true) {
      case (currentCategory.value && currentCategory.value !== 'all' && !searchQuery.value.trim()):
        url = '/components/categories';
        params.category = currentCategory.value;
        break;
      case (!currentCategory.value || currentCategory.value === 'all') && searchQuery.value.trim():
        url = '/components';
        params.name = searchQuery.value;
        break;
      case (currentCategory.value && currentCategory.value !== 'all' && searchQuery.value.trim()):
         url = '/components/categories'; 
         params.category = currentCategory.value;
         params.name = searchQuery.value;
         break;
      default:
        break;
    }

    const response = await api.get(url, { params });
    let data = response.data;
    
    // Клиентский фильтр для комбо-поиска
    if (currentCategory.value && currentCategory.value !== 'all' && searchQuery.value.trim()) {
       const catLower = currentCategory.value.toLowerCase();
       data = data.filter(item => String(item.category).toLowerCase() === catLower);
    }

    components.value = data;
  } catch (error) {
    console.error('Ошибка:', error);
  } finally {
    loading.value = false;
  }
};

let searchTimeout = null;
const handleSearch = () => {
  if (searchTimeout) clearTimeout(searchTimeout);
  searchTimeout = setTimeout(fetchComponents, 500);
};

const selectComponent = (item) => {
  router.push(`/components-details/${item.id}`);
};

const formatCategoryTitle = (cat) => {
  if (!cat || cat === 'all') return 'Все компоненты';
  return cat.charAt(0).toUpperCase() + cat.slice(1);
};

const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/');
};

onMounted(() => {
  currentCategory.value = route.params.type || 'all';
  fetchComponents();
});

watch(() => route.params.type, (newType) => {
  currentCategory.value = newType || 'all';
  searchQuery.value = ''; 
  fetchComponents();
});
</script>


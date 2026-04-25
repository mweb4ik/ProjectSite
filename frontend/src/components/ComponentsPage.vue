<template>
  <div class="components-page">
    <AppHeader :user="user" @logout="logout" />

    <main class="content">
      <h1 class="page-title">
        {{ formatCategoryTitle(currentCategory) }}
      </h1>

      <!-- Поиск -->
      <div class="search-bar">
        <input 
          v-model="searchQuery" 
          @input="handleSearch" 
          type="text" 
          placeholder="Поиск по названию (например, RTX, Intel)..." 
          class="search-input"
        />
      </div>

      <!-- Индикатор загрузки -->
      <div v-if="loading" class="loader">
        Загрузка компонентов...
      </div>

      <!-- Сообщение, если ничего не найдено -->
      <div v-else-if="components.length === 0" class="empty-state">
         Компоненты не найдены. Попробуйте изменить запрос.
      </div>

      <!-- Сетка компонентов -->
      <div v-else class="components-grid">
        <div v-for="item in components" :key="item.id" class="component-card">
          <div class="card-header">
            <span class="badge">{{ item.category }}</span>
            <span class="price">{{ item.price }} {{ item.currency }}</span>
          </div>
          
          <h3 class="card-title">{{ item.name }}</h3>
          
          <div class="card-specs">
            <p><strong>Характеристики:</strong> {{ item.specifications }}</p>
            <p v-if="item.socket"><strong>Сокет:</strong> {{ item.socket }}</p>
            <p v-if="item.powerConsumption"><strong>Потребление:</strong> {{ item.powerConsumption }} Вт</p>
          </div>

          <button class="btn-select" @click="selectComponent(item)">
            Выбрать
          </button>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import AppHeader from '@/components/AppHeader.vue';
import api from '@/api'; // Твой настроенный axios инстанс

const route = useRoute();
const router = useRouter();

// Состояние
const user = ref(JSON.parse(localStorage.getItem('user') || 'null'));
const components = ref([]);
const loading = ref(false);
const searchQuery = ref('');
const currentCategory = ref('all');

// --- ТВОЯ ЗАДАЧА №1: Реализовать функцию получения данных ---
const fetchComponents = async () => {
  loading.value = true;
  
  try {
    let url = '/components'; // Базовый URL
    const params = {};       // Параметры запроса

    switch (true) {
      
      // СЛУЧАЙ 1: Есть только категория (без поиска)
      case (currentCategory.value && currentCategory.value !== 'all' && !searchQuery.value.trim()):
        url = '/components/categories';
        params.category = currentCategory.value;
        console.log(`[API] Запрос категории: ${currentCategory.value}`);
        break;

      // 2: Есть только поиск (по всем категориям)
      case (!currentCategory.value || currentCategory.value === 'all') && searchQuery.value.trim():
        url = '/components';
        params.name = searchQuery.value;
        console.log(`[API] Глобальный поиск: ${searchQuery.value}`);
        break;

      // 3: Есть и категория, и поиск (поиск внутри категории)
      case (currentCategory.value && currentCategory.value !== 'all' && searchQuery.value.trim()):
        // Вариант А: Если бэкенд умеет фильтровать категорию по имени (нужно доработать бэк)
        // url = '/components/categories'; 
        // params.category = currentCategory.value;
        // params.name = searchQuery.value;
        
        // Вариант Б (сейчас): Ищем по имени, а потом фильтруем на фронтенде (проще для старта)
        url = '/components';
        params.name = searchQuery.value;
        console.log(`[API] Поиск "${searchQuery.value}" с последующей фильтрацией по "${currentCategory.value}"`);
        break;

      // 4: Нет ни категории, ни поиска (все компоненты)
      default:
        url = '/components';
        console.log('[API] Запрос всех компонентов');
        break;
    }

    // Выполняем запрос
    const response = await api.get(url, { params });
    
    // Пост-обработка данных (если нужен ручной фильтр на фронтенде для Случая 3)
    let data = response.data;
    if (currentCategory.value && currentCategory.value !== 'all' && searchQuery.value.trim()) {
       // Фильтруем на клиенте, если бэк вернул всё по имени
       const catLower = currentCategory.value.toLowerCase();
       data = data.filter(item => item.category.toLowerCase() === catLower);
    }

    components.value = data;

  } catch (error) {
    console.error('Ошибка при загрузке компонентов:', error);
  } finally {
    loading.value = false;
  }
};

// Обработчик ввода в поиск (с задержкой, чтобы не спамить запросами)
let searchTimeout = null;
const handleSearch = () => {
  if (searchTimeout) clearTimeout(searchTimeout);
  
  searchTimeout = setTimeout(() => {
    fetchComponents();
  }, 500); // Ждем 500мс после последнего ввода
};

// Выбор компонента (заглушка)
const selectComponent = (item) => {
  console.log('Выбран компонент:', item);
  // TODO: Решить, куда вести пользователя (в детали или в сборщик)
  // router.push(`/components-details/${item.id}`);
  router.push('/components-details/${item.id}')
};

// Форматирование заголовка
const formatCategoryTitle = (cat) => {
  if (!cat || cat === 'all') return 'Все компоненты';
  // Простая капитализация первой буквы
  return cat.charAt(0).toUpperCase() + cat.slice(1);
};

// Выход
const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/');
};

// Загрузка при монтировании и изменении категории в URL
onMounted(() => {
  // Получаем категорию из URL параметра :type
  currentCategory.value = route.params.type || 'all';
  fetchComponents();
});

// изменения параметров роута (если переход между категориями без перезагрузки страницы)
watch(() => route.params.type, (newType) => {
  currentCategory.value = newType || 'all';
  searchQuery.value = ''; 
  fetchComponents();
});
</script>


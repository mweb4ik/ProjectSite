<template>
  <div class="details-page">
    <AppHeader :user="user" @logout="logout" />
    
    <main class="content">
      <button @click="router.back()" class="btn-back">← Назад к списку</button>

      <!-- Загрузка -->
      <div v-if="loading" class="loader-container">
        <div class="skeleton-card">
          <div class="skeleton-image"></div>
          <div class="skeleton-text title"></div>
          <div class="skeleton-text line"></div>
        </div>
      </div>

      <!-- Ошибка -->
      <div v-else-if="error" class="error-state">
        <h2>Ошибка загрузки!</h2>
        <p>{{ error }}</p>
        <button @click="loadComponent" class="btn-retry">Попробовать снова</button>
      </div>

      <!-- Карточка компонента (ИСПРАВЛЕНО: убраны лишние закрывающие div) -->
      <div v-else-if="component" class="product-card">
        
        <!-- Заголовок -->
        <div class="card-header">
          <div class="title-group">
            <span class="badge" :class="getCategoryColor(component.category)">
              {{ formatCategory(component.category) }}
            </span>
            <h1 class="product-title">{{ component.name }}</h1>
          </div>
          <div class="price-tag">{{ component.price }} {{ component.currency }}</div>
        </div>

        <!-- Тело карточки -->
        <div class="card-body">
          
          <!-- Левая часть: Изображение -->
          <div class="visual-section">
            <div class="image-wrapper">
              <img 
                :src="getImageUrl(component.imageUrl)" 
                :alt="component.name"
                class="component-image"
                @error="onImageError" 
              />
              <p class="img-caption">{{ component.name }}</p>
            </div>
          </div>

          <!-- Правая часть: Характеристики -->
          <div class="specs-section">
            <h3>Характеристики</h3>
            
            <div class="specs-list">
              <div class="spec-row">
                <span class="label">Описание:</span>
                <span class="value">{{ component.specifications || 'Нет данных' }}</span>
              </div>

              <div v-if="component.socket" class="spec-row highlight">
                <span class="label">🔌 Сокет:</span>
                <span class="value">{{ component.socket }}</span>
                <small v-if="component.category === 'Processor'" class="hint">
                  ⚠️ Требуется совместимая материнская плата
                </small>
              </div>

              <div v-if="component.powerConsumption" class="spec-row">
                <span class="label">⚡ Потребление:</span>
                <span class="value">{{ component.powerConsumption }} Вт</span>
                <small class="hint">Рекомендуемый БП: от {{ calculatePSU(component.powerConsumption) }} Вт</small>
              </div>
              
              <!-- Подсказки по категориям -->
              <div v-if="getCategoryHint(component.category)" class="spec-row info-block">
                <span class="label">ℹ️ Особенности:</span>
                <p class="value text-block">{{ getCategoryHint(component.category) }}</p>
              </div>
            </div>

            <!-- Кнопки действий -->
            <div class="actions">
              <button 
                class="btn-add" 
                @click="addToBuilder(component)"
                :disabled="isInBuilder"
              >
                {{ isInBuilder ? '✅ Уже в сборке' : '🛒 Добавить в сборку' }}
              </button>
              
              <button v-if="isInBuilder" @click="goToBuilder" class="btn-go">
                Перейти в сборку →
              </button>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import AppHeader from '@/components/AppHeader.vue';
import api from '@/api';

const route = useRoute();
const router = useRouter();

const user = ref(JSON.parse(localStorage.getItem('user') || 'null'));
const component = ref(null);
const loading = ref(true);
const error = ref(null);

// Функция получения полного URL картинки
const getImageUrl = (relativePath) => {
  if (!relativePath) return null;
  
  const baseUrl = api.defaults.baseURL; 
  const rootUrl = baseUrl.replace(/\/api$/, '');
  
  let cleanPath = relativePath;
  if (cleanPath.startsWith('wwwroot/')) {
    cleanPath = cleanPath.substring(8);
  }
  if (cleanPath.startsWith('/')) {
    cleanPath = cleanPath.substring(1);
  }
  
  const finalUrl = `${rootUrl}/${cleanPath}`;
  console.log('🖼️ Формируем URL:', finalUrl); 
  return finalUrl;
};

// Если картинка не загрузилась
const onImageError = (e) => {
  // Скрываем битую картинку
  e.target.style.display = 'none';
  // Можно добавить фолбэк иконку, если нужно
};

const loadComponent = async () => {
  loading.value = true;
  error.value = null;
  try {
    const res = await api.get(`/components/${route.params.id}`);
    component.value = res.data;
  } catch (e) {
    console.error(e);
    error.value = e.response?.data?.message || 'Не удалось загрузить компонент';
  } finally {
    loading.value = false;
  }
};

const formatCategory = (cat) => cat ? cat.charAt(0).toUpperCase() + cat.slice(1) : '';

const getCategoryColor = (cat) => {
  const map = {
    Videocard: 'bg-red', Processor: 'bg-blue', Motherboard: 'bg-green',
    Ram: 'bg-yellow', Storage: 'bg-purple', Cooling: 'bg-cyan'
  };
  return map[cat] || 'bg-gray';
};

const calculatePSU = (watts) => watts ? Math.ceil((watts * 1.5) + 200) : '?';

const getCategoryHint = (cat) => {
  const hints = {
    Videocard: "Высокопроизводительное решение для игр и рендеринга. Обратите внимание на габариты корпуса и мощность БП.",
    Processor: "Центральный процессор определяет общую производительность. Критически важен выбор совместимого сокета материнской платы.",
    Motherboard: "Основная плата связывает все компоненты. Проверьте поддержку типа памяти (DDR4/DDR5) и форм-фактор корпуса.",
    Ram: "Объем и частота памяти важны для многозадачности. Убедитесь, что тип памяти (DDR4/DDR5) совпадает с материнской платой.",
    Storage: "NVMe SSD обеспечивают максимальную скорость загрузки. Проверьте наличие свободного слота M.2 на плате.",
    Cooling: "Система охлаждения должна справляться с TDP процессора. Проверьте совместимость сокета и высоту кулера."
  };
  return hints[cat] || null;
};

const builderItems = computed(() => {
  const saved = localStorage.getItem('builder_cart');
  return saved ? JSON.parse(saved) : [];
});

const isInBuilder = computed(() => {
  if (!component.value) return false;
  return builderItems.value.some(item => item.id === component.value.id);
});

const addToBuilder = (item) => {
  const currentCart = builderItems.value;
  const sameCategory = currentCart.find(i => i.category === item.category);
  
  if (sameCategory) {
    if(!confirm(`У вас уже есть ${sameCategory.name} в сборке. Заменить?`)) return;
    const filtered = currentCart.filter(i => i.category !== item.category);
    filtered.push(item);
    localStorage.setItem('builder_cart', JSON.stringify(filtered));
  } else {
    currentCart.push(item);
    localStorage.setItem('builder_cart', JSON.stringify(currentCart));
  }
  alert('Компонент добавлен в сборку!');
};

const goToBuilder = () => router.push('/builder');

const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/');
};

onMounted(loadComponent);
</script>
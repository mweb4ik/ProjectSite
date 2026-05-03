<template>
  <div class="builder-page">
    <AppHeader :user="user" @logout="logout" />

    <main class="content">
      <h1 class="page-title">🛠️ Конфигуратор ПК</h1>

      <div class="builder-container">
        <!-- Левая колонка: Выбор компонентов -->
        <div class="selection-panel">
          <h2>Добавить компоненты</h2>
          
          <div class="category-tabs">
            <button 
              v-for="cat in categories" 
              :key="cat"
              :class="['tab', { active: selectedCategory === cat }]"
              @click="selectCategory(cat)"
            >
              {{ getCategoryName(cat) }}
            </button>
          </div>

          <div class="components-list">
            <div v-if="loadingComponents" class="loader">Загрузка...</div>
            <div v-else-if="components.length === 0" class="empty">Нет компонентов в этой категории</div>
            
            <div v-else class="grid">
              <div v-for="item in components" :key="item.id" class="component-card-small">
                <div class="card-header">
                  <span class="name">{{ item.name }}</span>
                  <span class="price">{{ item.price }} {{ item.currency }}</span>
                </div>
                <p class="specs">{{ item.specifications }}</p>
                <button 
                  class="btn-add" 
                  @click="addComponent(item)"
                  :disabled="isInBuild(item.category)"
                  :class="{ 'in-build': isInBuild(item.category) }"
                >
                  {{ isInBuild(item.category) ? '✓ В сборке' : '+' }}
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Правая колонка: Текущая сборка -->
        <div class="build-panel">
          <h2>Ваша сборка</h2>
          
          <div v-if="buildItems.length === 0" class="empty-build">
            <p>Сборка пуста. Выберите компоненты слева.</p>
          </div>

          <div v-else class="build-list">
            <div v-for="(item, index) in buildItems" :key="item.id" class="build-item">
              <div class="item-info">
                <span class="badge">{{ getCategoryName(item.category) }}</span>
                <span class="name">{{ item.name }}</span>
                <span class="price">{{ item.price }} {{ item.currency }}</span>
              </div>
              <button class="btn-remove" @click="removeComponent(index)">✕</button>
            </div>

            <div class="summary">
              <div class="total">
                <span>Итого:</span>
                <span class="total-price">{{ totalPrice }} {{ currency }}</span>
              </div>
              <div class="power-estimate" v-if="estimatedPower > 0">
                <span>Потребление:</span>
                <span>{{ estimatedPower }} Вт</span>
              </div>
            </div>

            <div class="actions">
              <button class="btn-check" @click="checkCompatibility" :disabled="checking || buildItems.length === 0">
                {{ checking ? 'Проверка...' : '✅ Проверить совместимость' }}
              </button>
              <button class="btn-save" @click="saveBuild" :disabled="saving || buildItems.length === 0">
                {{ saving ? 'Сохранение...' : '💾 Сохранить сборку' }}
              </button>
              <button class="btn-clear" @click="clearBuild">🗑️ Очистить</button>
            </div>

            <!-- Результаты проверки -->
            <div v-if="compatibilityResult" class="result-box" :class="compatibilityResult.isCompatible ? 'success' : 'error'">
              <h3>{{ compatibilityResult.message }}</h3>
              
              <div v-if="compatibilityResult.errors?.length" class="errors">
                <p v-for="(err, i) in compatibilityResult.errors" :key="i" class="error-item">❌ {{ err }}</p>
              </div>
              
              <div v-if="compatibilityResult.warnings?.length" class="warnings">
                <p v-for="(warn, i) in compatibilityResult.warnings" :key="i" class="warn-item">⚠️ {{ warn }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import AppHeader from '@/components/AppHeader.vue';
import api from '@/api';

const router = useRouter();
const user = ref(JSON.parse(localStorage.getItem('user') || 'null'));

// Категории
const categories = ['Processor', 'Motherboard', 'Ram', 'Videocard', 'Storage', 'Cooling'];
const selectedCategory = ref('Processor');

// Данные
const components = ref([]);
const loadingComponents = ref(false);
const buildItems = ref([]);
const checking = ref(false);
const saving = ref(false);
const compatibilityResult = ref(null);
const currency = ref('USD');

// Загрузка компонентов выбранной категории
const fetchComponents = async () => {
  loadingComponents.value = true;
  compatibilityResult.value = null; // Сброс результатов при смене категории
  try {
    const res = await api.get('/components/categories', {
      params: { category: selectedCategory.value }
    });
    components.value = res.data;
    if (components.value.length > 0) {
      currency.value = components.value[0].currency;
    }
  } catch (e) {
    console.error('Ошибка загрузки компонентов:', e);
  } finally {
    loadingComponents.value = false;
  }
};

const selectCategory = (cat) => {
  selectedCategory.value = cat;
  fetchComponents();
};

// Добавление в сборку
const addComponent = (item) => {
  //Удаление уже имующегося компонента
  const existingIndex = buildItems.value.findIndex(i => i.category === item.category);
  if (existingIndex !== -1) {
    buildItems.value.splice(existingIndex, 1);
  }
  buildItems.value.push(item);
  compatibilityResult.value = null; 
};

// Удаление из сборки
const removeComponent = (index) => {
  buildItems.value.splice(index, 1);
  compatibilityResult.value = null;
};

// Очистка
const clearBuild = () => {
  buildItems.value = [];
  compatibilityResult.value = null;
};

// Проверка наличия компонента категории в сборке
const isInBuild = (category) => {
  return buildItems.value.some(i => i.category === category);
};

// Расчеты
const totalPrice = computed(() => {
  return buildItems.value.reduce((sum, item) => sum + (Number(item.price) || 0), 0);
});

const estimatedPower = computed(() => {
  return buildItems.value.reduce((sum, item) => sum + (Number(item.powerConsumption) || 0), 0);
});

// Проверка совместимости
const checkCompatibility = async () => {
  if (buildItems.value.length === 0) return;
  
  checking.value = true;
  compatibilityResult.value = null;

  try {
    const ids = buildItems.value.map(i => i.id);
    const res = await api.post('/components/check-compatibility', { componentIds: ids });
    compatibilityResult.value = res.data;
  } catch (e) {
    console.error(e);
    alert('Ошибка при проверке совместимости');
  } finally {
    checking.value = false;
  }
};

// Сохранение сборки в БД
const saveBuild = async () => {
  if (buildItems.value.length === 0) return;
  
  saving.value = true;
  try {
    const componentsJson = JSON.stringify(buildItems.value);
    
    const isCompatible = compatibilityResult.value ? compatibilityResult.value.isCompatible : false;

    await api.post('/builds', {
      componentsJson: componentsJson,
      totalPrice: totalPrice.value,
      currency: currency.value,
      isCompatible: isCompatible
    });

    alert('Сборка успешно сохранена!');
  } catch (e) {
    console.error('Ошибка сохранения:', e);
    alert('Не удалось сохранить сборку. Проверьте авторизацию.');
  } finally {
    saving.value = false;
  }
};

// Утилиты
const getCategoryName = (cat) => {
  const names = {
    Processor: 'Процессор',
    Motherboard: 'Материнская плата',
    Ram: 'ОЗУ',
    Videocard: 'Видеокарта',
    Storage: 'Накопитель',
    Cooling: 'Охлаждение'
  };
  return names[cat] || cat;
};

const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/');
};

onMounted(() => {
  fetchComponents();
});
</script>


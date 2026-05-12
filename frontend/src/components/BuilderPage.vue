<template>
  <div class="builder-page">
    <AppHeader :user="user" @logout="logout" />

    <main class="content">
      <h1 class="page-title">🛠️ Конфигуратор ПК</h1>

      <div class="builder-container">

        <!-- LEFT -->
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

            <div v-if="loadingComponents" class="loader">
              Загрузка...
            </div>

            <div v-else-if="components.length === 0" class="empty">
              Нет компонентов
            </div>

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
                >
                  {{ isInBuild(item.category) ? '✓ В сборке' : '+' }}
                </button>

              </div>
            </div>
          </div>
        </div>

        <!-- RIGHT -->
        <div class="build-panel">
          <h2>Ваша сборка</h2>

          <div v-if="buildItems.length === 0" class="empty-build">
            <p>Сборка пуста</p>
          </div>

          <div v-else>

            <div class="build-list">
              <div
                v-for="(item, index) in buildItems"
                :key="item.id"
                class="build-item"
              >
                <div>
                  <span class="badge">{{ getCategoryName(item.category) }}</span>
                  <span>{{ item.name }}</span>
                </div>

                <button @click="removeComponent(index)">✕</button>
              </div>
            </div>

            <!-- SUMMARY -->
            <div class="summary">
              <div>Итого: {{ totalPrice }} {{ currency }}</div>
              <div>Потребление: {{ estimatedPower }} W</div>
            </div>

            <!-- ACTIONS -->
            <div class="actions">
              <button @click="checkCompatibility" :disabled="checking">
                Проверить
              </button>

              <button @click="saveBuild" :disabled="saving">
                Сохранить
              </button>

              <button @click="clearBuild">
                Очистить
              </button>
            </div>

            <!-- RESULT -->
            <div v-if="compatibilityResult" class="result-box">
              <h3>{{ compatibilityResult.message }}</h3>

              <div v-if="compatibilityResult.errors?.length">
                <p v-for="e in compatibilityResult.errors" :key="e">❌ {{ e }}</p>
              </div>

              <div v-if="compatibilityResult.warnings?.length">
                <p v-for="w in compatibilityResult.warnings" :key="w">⚠️ {{ w }}</p>
              </div>
            </div>

          </div>
        </div>

      </div>
    </main>
  </div>
</template>

<script setup>
import '@/assets/styles/pages/BuilderPage.css'
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import { useBuilder } from '@/composables/useBuilder'

const router = useRouter()

const user = JSON.parse(localStorage.getItem('user') || 'null')

const {
  buildItems,
  components,
  selectedCategory,

  loading,
  checking,
  saving,

  compatibilityResult,
  currency,

  totalPrice,
  estimatedPower,

  fetchComponents,
  addComponent,
  removeComponent,
  clearBuild,
  isInBuild,
  checkCompatibility,
  saveBuild
} = useBuilder()

const categories = [
  'Processor',
  'Motherboard',
  'Ram',
  'Videocard',
  'Storage',
  'Cooling'
]

const selectCategory = (cat) => {
  selectedCategory.value = cat
  fetchComponents()
}

const getCategoryName = (cat) => ({
  Processor: 'CPU',
  Motherboard: 'MB',
  Ram: 'RAM',
  Videocard: 'GPU',
  Storage: 'SSD',
  Cooling: 'COOL'
}[cat] || cat)

const logout = () => {
  localStorage.clear()
  router.push('/')
}

onMounted(fetchComponents)
</script>
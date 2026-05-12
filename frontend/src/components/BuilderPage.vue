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
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import api from '@/api'

const router = useRouter()

// safe user
const user = ref(
  JSON.parse(localStorage.getItem('user') || 'null')
)

// state
const categories = [
  'Processor',
  'Motherboard',
  'Ram',
  'Videocard',
  'Storage',
  'Cooling'
]

const selectedCategory = ref('Processor')

const components = ref([])
const buildItems = ref([])

const loadingComponents = ref(false)
const checking = ref(false)
const saving = ref(false)

const compatibilityResult = ref(null)

const currency = ref('USD')

/* ================= LOAD ================= */
const fetchComponents = async () => {
  loadingComponents.value = true
  compatibilityResult.value = null

  try {
    const res = await api.get('/components/categories', {
      params: { category: selectedCategory.value }
    })

    components.value = res.data || []

    if (components.value.length > 0) {
      currency.value = components.value[0].currency || 'USD'
    }

  } catch (e) {
    console.error('load error', e)
    components.value = []
  } finally {
    loadingComponents.value = false
  }
}

/* ================= BUILD ================= */
const addComponent = (item) => {
  if (!item?.category) return

  buildItems.value = buildItems.value.filter(
    x => x.category !== item.category
  )

  buildItems.value.push(item)

  compatibilityResult.value = null
}

const removeComponent = (index) => {
  buildItems.value.splice(index, 1)
  compatibilityResult.value = null
}

const clearBuild = () => {
  buildItems.value = []
  compatibilityResult.value = null
}

const isInBuild = (category) => {
  return buildItems.value.some(x => x.category === category)
}

/* ================= COMPUTED ================= */
const totalPrice = computed(() =>
  buildItems.value.reduce((s, i) => s + (i.price || 0), 0)
)

const estimatedPower = computed(() =>
  buildItems.value.reduce((s, i) => s + (i.powerConsumption || 0), 0)
)

/* ================= API ================= */
const checkCompatibility = async () => {
  if (!buildItems.value.length) return

  checking.value = true

  try {
    const ids = buildItems.value.map(x => x.id)

    const res = await api.post('/components/check-compatibility', {
      componentIds: ids
    })

    compatibilityResult.value = res.data

  } catch (e) {
    console.error(e)
  } finally {
    checking.value = false
  }
}

const saveBuild = async () => {
  if (!buildItems.value.length) return

  saving.value = true

  try {
    await api.post('/builds', {
      componentsJson: JSON.stringify(buildItems.value),
      totalPrice: totalPrice.value,
      currency: currency.value,
      isCompatible: compatibilityResult.value?.isCompatible || false
    })

    alert('Сборка сохранена')

  } catch (e) {
    console.error(e)
    alert('Ошибка сохранения')
  } finally {
    saving.value = false
  }
}

/* ================= UI ================= */
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

/* ================= INIT ================= */
onMounted(fetchComponents)
</script>
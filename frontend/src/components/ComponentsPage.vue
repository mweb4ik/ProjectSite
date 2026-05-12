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
        <div
          v-for="item in components"
          :key="item.Id"
          class="component-card"
        >
          <div class="card-image-box">
            <img
              :src="getImageUrl(item.ImageUrl)"
              :alt="item.Name"
              class="card-img"
              @error="onImageError"
            />
          </div>

          <div class="card-content">
            <div class="card-header">
              <span class="badge">{{ item.Category }}</span>
              <span class="price">{{ item.Price }} {{ item.Currency }}</span>
            </div>

            <h3 class="card-title">{{ item.Name }}</h3>

            <div class="card-specs">
              <p><strong>Хар-ки:</strong> {{ item.Specifications }}</p>
              <p v-if="item.Socket"><strong>Сокет:</strong> {{ item.Socket }}</p>
              <p v-if="item.PowerConsumption">
                <strong>Вт:</strong> {{ item.PowerConsumption }}
              </p>
            </div>

            <button class="btn-select" @click="selectComponent(item)">
              Выбрать
            </button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>
<script setup>
import '@/assets/styles/pages/ComponentsPage.css'
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import api from '@/api'
import { mapComponent } from '@/utils/mapper'

const route = useRoute()
const router = useRouter()

const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))

const components = ref([])
const loading = ref(false)

const searchQuery = ref('')
const currentCategory = ref('all')

let timeout = null

const getImageUrl = (path) => {
  if (!path) return ''
  const base = api.defaults.baseURL?.replace('/api', '') || ''
  return `${base}/${path.replace(/^\/+/, '').replace('wwwroot/', '')}`
}

const onImageError = (e) => {
  e.target.style.display = 'none'
}

/* ================= LOAD ================= */
const fetchComponents = async () => {
  loading.value = true

  try {
    const params = {}

    if (currentCategory.value && currentCategory.value !== 'all') {
      params.category = currentCategory.value
    }

    if (searchQuery.value?.trim()) {
      params.name = searchQuery.value.trim()
    }

    const res = await api.get('/components', { params })

    components.value = (res.data || []).map(mapComponent)

  } catch (e) {
    console.error('Components load error:', e)
    components.value = []
  } finally {
    loading.value = false
  }
}

/* ================= SEARCH ================= */
const handleSearch = () => {
  clearTimeout(timeout)
  timeout = setTimeout(() => {
    fetchComponents()
  }, 350)
}

/* ================= NAV ================= */
const selectComponent = (item) => {
  const id = item?.id || item?.Id
  if (!id) return
  router.push(`/components-details/${id}`)
}

const formatCategoryTitle = (cat) => {
  if (!cat || cat === 'all') return 'Все компоненты'
  return cat.charAt(0).toUpperCase() + cat.slice(1)
}

const logout = () => {
  localStorage.clear()
  router.push('/')
}

/* ================= INIT ================= */
onMounted(() => {
  currentCategory.value = route.params.type || 'all'
  fetchComponents()
})

watch(() => route.params.type, (val) => {
  currentCategory.value = val || 'all'
  searchQuery.value = ''
  fetchComponents()
})
</script>
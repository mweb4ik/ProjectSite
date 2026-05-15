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
      <!-- Фильтр по категориям -->
<div class="category-filter">
  <button 
    class="cat-btn" 
    :class="{ active: currentCategory === 'all' }"
    @click="setCategory('all')"
  >
    Все
  </button>
  <button 
    class="cat-btn" 
    :class="{ active: currentCategory === 'Processor' }"
    @click="setCategory('Processor')"
  >
    🧠 CPU
  </button>
  <button 
    class="cat-btn" 
    :class="{ active: currentCategory === 'Motherboard' }"
    @click="setCategory('Motherboard')"
  >
    🔌 MB
  </button>
  <button 
    class="cat-btn" 
    :class="{ active: currentCategory === 'Ram' }"
    @click="setCategory('Ram')"
  >
    💾 RAM
  </button>
  <button 
    class="cat-btn" 
    :class="{ active: currentCategory === 'Videocard' }"
    @click="setCategory('Videocard')"
  >
    🎮 GPU
  </button>
  <button 
    class="cat-btn" 
    :class="{ active: currentCategory === 'Storage' }"
    @click="setCategory('Storage')"
  >
    💿 SSD
  </button>
  <button 
    class="cat-btn" 
    :class="{ active: currentCategory === 'Cooling' }"
    @click="setCategory('Cooling')"
  >
    ❄️ COOL
  </button>
</div>

<!-- Поиск (уже есть) -->
<div class="search-bar">
  <input
    v-model="searchQuery"
    @input="handleSearch"
    type="text"
    placeholder="Поиск (RTX, Intel)..."
    class="search-input"
  />
</div>
<div class="pagination-wrapper" v-if="components.length > 0">
  <button 
    v-if="hasMore" 
    class="btn-pagination"
    @click="fetchComponents(true)"
    :disabled="loadingMore"
  >
    {{ loadingMore ? '⏳ Загрузка...' : `⬇️ Загрузить ещё (${itemsPerPage})` }}
  </button>
  <span v-else class="pagination-info">
    ✅ Все компоненты загружены ({{ totalComponents }})
  </span>
</div>
      <div v-if="loading" class="loader">Загрузка...</div>

      <div v-else-if="components.length === 0" class="empty-state">
        Ничего не найдено
      </div>

      <div v-else class="components-grid">
        <div
          v-for="item in components"
          :key="item.id"
          class="component-card"
        >
          <div class="card-image-box">
            <img
              :src="getImageUrl(item.imageUrl)"
              :alt="item.name"
              class="card-img"
              @error="onImageError"
            />
          </div>

          <div class="card-content">
            <div class="card-header">
              <span class="badge">{{ item.category }}</span>
              <span class="price">
                {{ item.price }} {{ item.currency }}
              </span>
            </div>

            <h3 class="card-title">{{ item.name }}</h3>

            <div class="card-specs">
              <p><strong>Характеристики:</strong> {{ item.specifications }}</p>

              <p v-if="item.socket">
                <strong>Сокет:</strong> {{ item.socket }}
              </p>

              <p v-if="item.powerConsumption">
                <strong>Вт:</strong> {{ item.powerConsumption }}
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
const currentPage = ref(1)
const itemsPerPage = ref(5)
const totalComponents = ref(0)
const hasMore = ref(true)
const loadingMore = ref(false)
let timeout = null

const getImageUrl = (path) => {
  if (!path) return ''
  const base = api.defaults.baseURL?.replace('/api', '') || ''
  return `${base}/${path.replace(/^\/+/, '').replace('wwwroot/', '')}`
}

const onImageError = (e) => {
  e.target.style.display = 'none'
}
const setCategory = (category) => {
  currentCategory.value = category
  searchQuery.value = ''
  currentPage.value = 1  
  hasMore.value = true
  fetchComponents(false) 
}


const fetchComponents = async (append = false) => {
  if (!append) {
    loading.value = true
  } else {
    loadingMore.value = true
  }

  try {
    const params = {
      page: append ? currentPage.value + 1 : 1,
      limit: itemsPerPage.value
    }

    if (currentCategory.value && currentCategory.value !== 'all') {
      params.category = currentCategory.value
    }

    if (searchQuery.value.trim()) {
      params.name = searchQuery.value.trim()
    }

    const res = await api.get('/components', { params })
    
    const { Data, Page, TotalCount, HasMore } = res.data
    
    if (append) {

      components.value = [...components.value, ...Data.map(mapComponent)]
      currentPage.value = Page
    } else {
      components.value = Data.map(mapComponent)
      currentPage.value = 1
    }
    
    totalComponents.value = TotalCount
    hasMore.value = HasMore
    
  } catch (e) {
    console.error(e)
    if (!append) components.value = []
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

const handleSearch = () => {
  clearTimeout(timeout)
  currentPage.value = 1 
  hasMore.value = true
  timeout = setTimeout(() => fetchComponents(false), 300)
}

const selectComponent = (item) => {
  router.push(`/components-details/${item.id}`)
}

const formatCategoryTitle = (cat) => {
  if (!cat || cat === 'all') return 'Все компоненты'
  return cat.charAt(0).toUpperCase() + cat.slice(1)
}

const logout = () => {
  localStorage.clear()
  router.push('/')
}

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
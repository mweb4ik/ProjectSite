import api from '@/api'
import { mapComponent } from '@/utils/mapper'
import { ref, computed } from 'vue'

export function useBuilder() {
  // === STATE ===
  const buildItems = ref([])
  const components = ref([])
  const selectedCategory = ref('all')
  const loading = ref(false)
  const checking = ref(false)
  const saving = ref(false)
  const compatibilityResult = ref(null)
  const currency = ref('$')

  // === COMPUTED ===
  const totalPrice = computed(() => 
    buildItems.value.reduce((sum, item) => sum + (item.price || 0), 0)
  )

  const estimatedPower = computed(() => 
    buildItems.value.reduce((sum, item) => sum + (item.powerConsumption || 0), 0)
  )

const isInBuild = (category) => {
  if (!category) return false
  const cat = String(category).toLowerCase()
  return buildItems.value.some(item => String(item.category).toLowerCase() === cat)
}

  // === METHODS ===
  const isItemInBuild = (itemId) => {
  if (!itemId) return false
  return buildItems.value.some(item => item.id === itemId)
}
  const fetchComponents = async () => {
    loading.value = true
    try {
      const params = {}
      if (selectedCategory.value && selectedCategory.value !== 'all') {
        params.category = selectedCategory.value
      }
      const res = await api.get('/components', { params })
      components.value = (res.data || []).map(mapComponent)
    } catch (e) {
      console.error('Fetch components error:', e)
      components.value = []
    } finally {
      loading.value = false
    }
  }

 const addComponent = (item) => {
  if (isInBuild(item.category)) return false
  if (isItemInBuild(item.id)) return false
  
  buildItems.value.push({ 
    ...item,
    category: String(item.category)
  })
  return true
}

  const removeComponent = (index) => {
    buildItems.value.splice(index, 1)
    compatibilityResult.value = null
  }

  const clearBuild = () => {
    buildItems.value = []
    compatibilityResult.value = null
  }

  const checkCompatibility = async () => {
    if (buildItems.value.length === 0) return
    checking.value = true
    try {
      const componentIds = buildItems.value.map(item => item.id)
      const res = await api.post('/components/check-compatibility', { componentIds })
      compatibilityResult.value = res.data
    } catch (e) {
      console.error('Compatibility check error:', e)
      compatibilityResult.value = { 
        message: 'Ошибка проверки совместимости', 
        errors: ['Не удалось соединиться с сервером'],
        warnings: []
      }
    } finally {
      checking.value = false
    }
  }

  const saveBuild = async () => {
    if (buildItems.value.length === 0) return
    saving.value = true
    try {
      await api.post('/builds', { components: buildItems.value })
      alert('Сборка сохранена!')
    } catch (e) {
      console.error('Save build error:', e)
      alert('Ошибка сохранения сборки')
    } finally {
      saving.value = false
    }
  }

  // === EXPORT ===
  return {
    
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
     isItemInBuild, 
    fetchComponents,
    addComponent,
    removeComponent,
    clearBuild,
    isInBuild,
    checkCompatibility,
    saveBuild
  }
}
import { ref, computed } from 'vue'
import api from '@/api'
import { mapComponent } from '@/utils/mapper'

export function useBuilder() {
  const buildItems = ref([])
  const components = ref([])
  const selectedCategory = ref('Processor')

  const loading = ref(false)
  const checking = ref(false)
  const saving = ref(false)

  const compatibilityResult = ref(null)

  const currency = ref('USD')

  /* ================= LOAD COMPONENTS ================= */
  const fetchComponents = async () => {
    loading.value = true
    compatibilityResult.value = null

    try {
      const res = await api.get('/components/categories', {
        params: { category: selectedCategory.value }
      })

      components.value = (res.data || []).map(mapComponent)

      if (components.value.length) {
        currency.value = components.value[0].currency
      }

    } catch (e) {
      console.error(e)
      components.value = []
    } finally {
      loading.value = false
    }
  }

  /* ================= BUILD ================= */
  const addComponent = (item) => {
    buildItems.value = buildItems.value.filter(
      x => x.category !== item.category
    )
    buildItems.value.push(item)
    compatibilityResult.value = null
  }

  const removeComponent = (i) => {
    buildItems.value.splice(i, 1)
    compatibilityResult.value = null
  }

  const clearBuild = () => {
    buildItems.value = []
    compatibilityResult.value = null
  }

  const isInBuild = (cat) =>
    buildItems.value.some(x => x.category === cat)

  /* ================= COMPUTED ================= */
  const totalPrice = computed(() =>
    buildItems.value.reduce((s, i) => s + (i.price || 0), 0)
  )

  const estimatedPower = computed(() =>
    buildItems.value.reduce((s, i) => s + (i.powerConsumption || 0), 0)
  )

  /* ================= API ================= */
  const checkCompatibility = async () => {
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
    saving.value = true

    try {
      await api.post('/builds', {
        componentsJson: JSON.stringify(buildItems.value),
        totalPrice: totalPrice.value,
        currency: currency.value,
        isCompatible: compatibilityResult.value?.isCompatible || false
      })

    } catch (e) {
      console.error(e)
    } finally {
      saving.value = false
    }
  }

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

    fetchComponents,
    addComponent,
    removeComponent,
    clearBuild,
    isInBuild,
    checkCompatibility,
    saveBuild
  }
}
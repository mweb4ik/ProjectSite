<template>
  <div class="admin-page">
    <div class="page-header">
      <h1>🛡️ Панель администратора</h1>
      <p>Управление пользователями и компонентами системы</p>
    </div>

    <div v-if="loading" class="loading">
      <div class="loader"></div>
      <span>Загрузка данных...</span>
    </div>

    <div v-else>
      <!-- СТАТИСТИКА -->
      <div class="stats-grid">
        <div class="stat-card users">
          <div class="stat-icon">👥</div>
          <div>
            <h3>Пользователей</h3>
            <p>{{ stats.totalUsers }}</p>
          </div>
        </div>

        <div class="stat-card components">
          <div class="stat-icon">🔧</div>
          <div>
            <h3>Компонентов</h3>
            <p>{{ stats.totalComponents }}</p>
          </div>
        </div>

        <div class="stat-card builds">
          <div class="stat-icon">🖥️</div>
          <div>
            <h3>Сборок ПК</h3>
            <p>{{ stats.totalBuilds }}</p>
          </div>
        </div>

        <div class="stat-card quiz">
          <div class="stat-icon">📊</div>
          <div>
            <h3>Результатов квиза</h3>
            <p>{{ stats.totalQuizResults }}</p>
          </div>
        </div>
      </div>

      <!-- USERS -->
      <div class="admin-section">
        <div class="section-header">
          <h2>👥 Пользователи</h2>
        </div>

        <div class="table-wrapper">
          <table class="admin-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Username</th>
                <th>Email</th>
                <th>Роль</th>
                <th>Дата регистрации</th>
                <th>Действия</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="user in users" :key="user.Id">
                <td class="mono">
                  {{ shortId(user.Id) }}
                </td>

                <td>
                  {{ user.Username }}
                </td>

                <td>
                  {{ user.Email }}
                </td>

                <td>
                  <select
                    class="role-select"
                    :value="user.Role"
                    @change="changeUserRole(user.Id, $event.target.value)"
                    :disabled="user.Role === 'admin'"
                  >
                    <option value="standard">Пользователь</option>
                    <option value="admin">Администратор</option>
                    <option value="guest">Гость</option>
                  </select>
                </td>

                <td>
                  {{ formatDate(user.CreatedAt) }}
                </td>

                <td>
                  <button
                    class="icon-btn delete"
                    @click="deleteUser(user.Id)"
                    :disabled="user.Role === 'admin'"
                  >
                    🗑️
                  </button>
                </td>
              </tr>

              <tr v-if="users.length === 0">
                <td colspan="6" class="empty">
                  Пользователи не найдены
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- COMPONENTS -->
      <div class="admin-section">
        <div class="section-header">
          <h2>🔧 Компоненты</h2>

          <button class="btn-add" @click="openCreateModal">
            ➕ Добавить компонент
          </button>
        </div>

        <div class="table-wrapper">
          <table class="admin-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Название</th>
                <th>Категория</th>
                <th>Цена</th>
                <th>Сокет</th>
                <th>Действия</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="component in components" :key="component.Id">
                <td class="mono">
                  {{ component.Id }}
                </td>

                <td>
                  {{ component.Name }}
                </td>

                <td>
                  {{ component.Category }}
                </td>

                <td>
                  {{ component.Price }} {{ component.Currency }}
                </td>

                <td>
                  {{ component.Socket || '-' }}
                </td>

                <td class="actions">
                  <button
                    class="icon-btn edit"
                    @click="editComponent(component)"
                  >
                    ✏️
                  </button>

                  <button
                    class="icon-btn delete"
                    @click="deleteComponent(component.Id)"
                  >
                    🗑️
                  </button>
                </td>
              </tr>

              <tr v-if="components.length === 0">
                <td colspan="6" class="empty">
                  Компоненты не найдены
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- MODAL -->
      <div
        v-if="showAddComponentModal || showEditComponentModal"
        class="modal-overlay"
        @click.self="closeModals"
      >
        <div class="modal">
          <div class="modal-header">
            <h2>
              {{
                showEditComponentModal
                  ? 'Редактирование компонента'
                  : 'Создание компонента'
              }}
            </h2>
          </div>

          <form @submit.prevent="saveComponent">
            <div class="form-grid">
              <div class="form-group">
                <label>ID *</label>
                <input
                  v-model="componentForm.Id"
                  :disabled="showEditComponentModal"
                  required
                />
              </div>

              <div class="form-group">
                <label>Название *</label>
                <input
                  v-model="componentForm.Name"
                  required
                />
              </div>

              <div class="form-group">
                <label>Категория *</label>

                <select v-model="componentForm.Category" required>
                  <option value="">Выберите категорию</option>
                  <option value="Processor">Процессор</option>
                  <option value="Motherboard">Материнская плата</option>
                  <option value="Videocard">Видеокарта</option>
                  <option value="Ram">Оперативная память</option>
                  <option value="Storage">Накопитель</option>
                  <option value="Cooling">Охлаждение</option>
                </select>
              </div>

              <div class="form-group">
                <label>Цена *</label>
                <input
                  type="number"
                  v-model.number="componentForm.Price"
                  required
                />
              </div>

              <div class="form-group">
                <label>Валюта</label>
                <input v-model="componentForm.Currency" />
              </div>

              <div class="form-group">
                <label>Сокет</label>
                <input v-model="componentForm.Socket" />
              </div>

              <div class="form-group full">
                <label>Характеристики</label>
                <textarea
                  v-model="componentForm.Specifications"
                ></textarea>
              </div>

              <div class="form-group">
                <label>Энергопотребление</label>
                <input
                  type="number"
                  v-model.number="componentForm.PowerConsumption"
                />
              </div>

              <div class="form-group">
                <label>URL изображения</label>
                <input v-model="componentForm.ImageUrl" />
              </div>
            </div>

            <div class="modal-actions">
              <button
                type="button"
                class="btn-cancel"
                @click="closeModals"
              >
                Отмена
              </button>

              <button
                type="submit"
                class="btn-save"
              >
                {{
                  showEditComponentModal
                    ? 'Сохранить'
                    : 'Создать'
                }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { api } from '@/api';
import '@/assets/styles/pages/AdminPage.css'
export default {
  name: 'AdminPage',

  data() {
    return {
      loading: true,

      stats: {
        totalUsers: 0,
        totalComponents: 0,
        totalBuilds: 0,
        totalQuizResults: 0
      },

      users: [],
      components: [],

      showAddComponentModal: false,
      showEditComponentModal: false,

      componentForm: {
        Id: '',
        Name: '',
        Category: '',
        Price: 0,
        Currency: '$',
        Specifications: '',
        Socket: '',
        PowerConsumption: null,
        ImageUrl: ''
      }
    };
  },

  async mounted() {
    await this.loadData();
  },

  methods: {
    async loadData() {
      this.loading = true;

      try {
        const [usersRes, componentsRes] = await Promise.all([
          api.get('/admin/users'),
          api.get('/components')
        ]);

        // FIX CASE MISMATCH
        this.users = (usersRes.data || []).map(u => ({
          Id: u.Id || u.id,
          Username: u.Username || u.username,
          Email: u.Email || u.email,
          Role: u.Role || u.role,
          CreatedAt: u.CreatedAt || u.createdAt
        }));

        this.components = (componentsRes.data || []).map(c => ({
          Id: c.Id || c.id,
          Name: c.Name || c.name,
          Category: c.Category || c.category,
          Price: c.Price || c.price,
          Currency: c.Currency || c.currency,
          Specifications: c.Specifications || c.specifications,
          Socket: c.Socket || c.socket,
          PowerConsumption: c.PowerConsumption || c.powerConsumption,
          ImageUrl: c.ImageUrl || c.imageUrl
        }));

        this.stats.totalUsers = this.users.length;
        this.stats.totalComponents = this.components.length;

        try {
          const buildsRes = await api.get('/builds');
          this.stats.totalBuilds = buildsRes.data?.length || 0;
        } catch {
          this.stats.totalBuilds = 0;
        }

        try {
          const quizRes = await api.get('/quiz/results');
          this.stats.totalQuizResults = quizRes.data?.length || 0;
        } catch {
          this.stats.totalQuizResults = 0;
        }

      } catch (error) {
        console.error(error);
        alert('Ошибка загрузки админ-панели');
      } finally {
        this.loading = false;
      }
    },

    shortId(id) {
      if (!id) return '-';
      return id.substring(0, 8) + '...';
    },

    formatDate(dateString) {
      if (!dateString) return '-';

      return new Date(dateString).toLocaleDateString('ru-RU', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    },

    async changeUserRole(userId, role) {
      if (!confirm(`Изменить роль на "${role}"?`)) return;

      try {
        await api.put(`/admin/users/${userId}/role`, {
          role
        });

        await this.loadData();
      } catch (error) {
        console.error(error);
        alert('Ошибка изменения роли');
      }
    },

    async deleteUser(userId) {
      if (!confirm('Удалить пользователя?')) return;

      try {
        await api.delete(`/admin/users/${userId}`);

        await this.loadData();
      } catch (error) {
        console.error(error);
        alert('Ошибка удаления');
      }
    },

    openCreateModal() {
      this.resetForm();
      this.showAddComponentModal = true;
    },

    editComponent(component) {
      this.componentForm = { ...component };
      this.showEditComponentModal = true;
    },

    closeModals() {
      this.showAddComponentModal = false;
      this.showEditComponentModal = false;

      this.resetForm();
    },

    resetForm() {
      this.componentForm = {
        Id: '',
        Name: '',
        Category: '',
        Price: 0,
        Currency: '$',
        Specifications: '',
        Socket: '',
        PowerConsumption: null,
        ImageUrl: ''
      };
    },

    async saveComponent() {
      try {
        if (this.showEditComponentModal) {
          await api.put(
            `/components/${this.componentForm.Id}`,
            this.componentForm
          );
        } else {
          await api.post(
            '/components',
            this.componentForm
          );
        }

        this.closeModals();
        await this.loadData();

      } catch (error) {
        console.error(error);

        alert(
          error.response?.data?.message ||
          'Ошибка сохранения'
        );
      }
    },

    async deleteComponent(componentId) {
      if (!confirm('Удалить компонент?')) return;

      try {
        await api.delete(`/components/${componentId}`);

        await this.loadData();

      } catch (error) {
        console.error(error);
        alert('Ошибка удаления компонента');
      }
    }
  }
};
</script>


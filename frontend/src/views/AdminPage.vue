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

<style scoped>
.admin-page {
  min-height: 100vh;
  background: #0b0f19;
  color: white;
  padding: 40px 20px;
}

.page-header {
  text-align: center;
  margin-bottom: 40px;
}

.page-header h1 {
  font-size: 42px;
  margin-bottom: 10px;
}

.page-header p {
  color: #a0aec0;
}

.loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 15px;
  margin-top: 80px;
  font-size: 20px;
}

.loader {
  width: 28px;
  height: 28px;
  border: 3px solid #444;
  border-top: 3px solid #6c63ff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
  margin-bottom: 35px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 15px;

  padding: 25px;
  border-radius: 18px;

  background: linear-gradient(
    135deg,
    #1f2937,
    #374151
  );

  border: 1px solid rgba(255,255,255,0.08);
}

.stat-card p {
  font-size: 32px;
  font-weight: bold;
  margin-top: 6px;
}

.stat-icon {
  font-size: 38px;
}

.admin-section {
  background: #111827;
  border-radius: 18px;
  padding: 25px;
  margin-bottom: 35px;
  border: 1px solid rgba(255,255,255,0.08);
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 25px;
}

.table-wrapper {
  overflow-x: auto;
}

.admin-table {
  width: 100%;
  border-collapse: collapse;
}

.admin-table th {
  background: #1f2937;
  color: #cbd5e1;
}

.admin-table th,
.admin-table td {
  padding: 16px;
  border-bottom: 1px solid #253041;
  text-align: left;
}

.admin-table tr:hover {
  background: rgba(255,255,255,0.03);
}

.mono {
  font-family: monospace;
}

.role-select,
.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  background: #1f2937;
  color: white;
  border: 1px solid #374151;
  border-radius: 10px;
  padding: 12px;
}

.form-group textarea {
  min-height: 100px;
  resize: vertical;
}

.btn-add,
.btn-save {
  background: linear-gradient(
    135deg,
    #6366f1,
    #8b5cf6
  );

  color: white;
  border: none;
  padding: 12px 18px;
  border-radius: 12px;
  cursor: pointer;
  font-weight: bold;
}

.btn-cancel {
  background: #374151;
  color: white;
  border: none;
  padding: 12px 18px;
  border-radius: 12px;
  cursor: pointer;
}

.icon-btn {
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 20px;
  margin-right: 10px;
}

.icon-btn.delete:hover {
  transform: scale(1.15);
}

.icon-btn.edit:hover {
  transform: scale(1.15);
}

.actions {
  white-space: nowrap;
}

.empty {
  text-align: center;
  padding: 30px;
  color: #94a3b8;
}

.modal-overlay {
  position: fixed;
  inset: 0;

  background: rgba(0,0,0,0.7);

  display: flex;
  justify-content: center;
  align-items: center;

  z-index: 1000;
}

.modal {
  width: 90%;
  max-width: 700px;

  background: #111827;

  border-radius: 20px;
  padding: 30px;

  border: 1px solid rgba(255,255,255,0.08);
}

.modal-header {
  margin-bottom: 25px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.form-group.full {
  grid-column: span 2;
}

.modal-actions {
  margin-top: 25px;
  display: flex;
  justify-content: flex-end;
  gap: 15px;
}

@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
  }

  .form-group.full {
    grid-column: span 1;
  }

  .section-header {
    flex-direction: column;
    gap: 15px;
    align-items: stretch;
  }

  .page-header h1 {
    font-size: 30px;
  }
}
</style>
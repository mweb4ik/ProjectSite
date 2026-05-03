<template>
  <div class="admin-page">
    <h1>🛡️ Панель администратора</h1>
    
    <div v-if="loading" class="loading">Загрузка данных...</div>
    
    <div v-else>
      <!-- Статистика -->
      <div class="stats-grid">
        <div class="stat-card">
          <h3>👥 Пользователей</h3>
          <p class="stat-value">{{ stats.totalUsers }}</p>
        </div>
        <div class="stat-card">
          <h3>🔧 Компонентов</h3>
          <p class="stat-value">{{ stats.totalComponents }}</p>
        </div>
        <div class="stat-card">
          <h3>🏗️ Сборок ПК</h3>
          <p class="stat-value">{{ stats.totalBuilds }}</p>
        </div>
        <div class="stat-card">
          <h3>📊 Результатов квиза</h3>
          <p class="stat-value">{{ stats.totalQuizResults }}</p>
        </div>
      </div>

      <!-- Управление пользователями -->
      <div class="admin-section">
        <h2>👥 Пользователи</h2>
        <div class="table-container">
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
              <tr v-for="user in users" :key="user.id">
                <td>{{ user.id.substring(0, 8) }}...</td>
                <td>{{ user.username }}</td>
                <td>{{ user.email }}</td>
                <td>
                  <select 
                    :value="user.role" 
                    @change="changeUserRole(user.id, $event.target.value)"
                    :disabled="user.role === 'admin'"
                  >
                    <option value="standard">Пользователь</option>
                    <option value="admin">Админ</option>
                    <option value="guest">Гость</option>
                  </select>
                </td>
                <td>{{ formatDate(user.createdAt) }}</td>
                <td>
                  <button 
                    class="btn-delete" 
                    @click="deleteUser(user.id)"
                    :disabled="user.role === 'admin'"
                  >
                    🗑️
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Список компонентов -->
      <div class="admin-section">
        <h2>🔧 Компоненты</h2>
        <button class="btn-add" @click="showAddComponentModal = true">➕ Добавить компонент</button>
        <div class="table-container">
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
              <tr v-for="component in components" :key="component.id">
                <td>{{ component.id }}</td>
                <td>{{ component.name }}</td>
                <td>{{ component.category }}</td>
                <td>{{ component.price }} {{ component.currency }}</td>
                <td>{{ component.socket || '-' }}</td>
                <td>
                  <button class="btn-edit" @click="editComponent(component)">✏️</button>
                  <button class="btn-delete" @click="deleteComponent(component.id)">🗑️</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Модальное окно добавления/редактирования компонента -->
      <div v-if="showAddComponentModal || showEditComponentModal" class="modal-overlay" @click.self="closeModals">
        <div class="modal">
          <h2>{{ showEditComponentModal ? 'Редактировать компонент' : 'Новый компонент' }}</h2>
          
          <form @submit.prevent="saveComponent">
            <div class="form-group">
              <label>ID *</label>
              <input v-model="componentForm.id" :disabled="showEditComponentModal" required />
            </div>
            
            <div class="form-group">
              <label>Название *</label>
              <input v-model="componentForm.name" required />
            </div>
            
            <div class="form-group">
              <label>Категория *</label>
              <select v-model="componentForm.category" required>
                <option value="">Выберите категорию</option>
                <option value="Processor">Процессор</option>
                <option value="Motherboard">Материнская плата</option>
                <option value="Videocard">Видеокарта</option>
                <option value="Ram">Оперативная память</option>
                <option value="Storage">Накопитель</option>
                <option value="Cooling">Охлаждение</option>
                <option value="PowerSupply">Блок питания</option>
              </select>
            </div>
            
            <div class="form-group">
              <label>Цена *</label>
              <input type="number" v-model.number="componentForm.price" required />
            </div>
            
            <div class="form-group">
              <label>Валюта</label>
              <input v-model="componentForm.currency" placeholder="$" />
            </div>
            
            <div class="form-group">
              <label>Характеристики</label>
              <textarea v-model="componentForm.specifications"></textarea>
            </div>
            
            <div class="form-group">
              <label>Сокет</label>
              <input v-model="componentForm.socket" placeholder="LGA1700, AM5 и т.д." />
            </div>
            
            <div class="form-group">
              <label>Энергопотребление (W)</label>
              <input type="number" v-model.number="componentForm.powerConsumption" />
            </div>
            
            <div class="form-group">
              <label>URL изображения</label>
              <input v-model="componentForm.imageUrl" placeholder="wwwroot/images/..." />
            </div>
            
            <div class="modal-actions">
              <button type="button" class="btn-cancel" @click="closeModals">Отмена</button>
              <button type="submit" class="btn-save">{{ showEditComponentModal ? 'Сохранить' : 'Создать' }}</button>
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
        id: '',
        name: '',
        category: '',
        price: 0,
        currency: '$',
        specifications: '',
        socket: '',
        powerConsumption: null,
        imageUrl: ''
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
        
        this.users = usersRes.data || [];
        this.components = componentsRes.data || [];
        
        // Подсчёт статистики
        this.stats.totalUsers = this.users.length;
        this.stats.totalComponents = this.components.length;
        
        // Загрузка сборок и результатов квиза (если есть эндпоинты)
        try {
          const buildsRes = await api.get('/builds');
          this.stats.totalBuilds = (buildsRes.data || []).length;
        } catch (e) {
          this.stats.totalBuilds = 0;
        }
        
        try {
          const quizRes = await api.get('/quiz/results');
          this.stats.totalQuizResults = (quizRes.data || []).length;
        } catch (e) {
          this.stats.totalQuizResults = 0;
        }
        
      } catch (error) {
        console.error('Ошибка загрузки данных:', error);
        alert('Не удалось загрузить данные админ-панели. Убедитесь, что у вас есть права администратора.');
      } finally {
        this.loading = false;
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return '-';
      const date = new Date(dateString);
      return date.toLocaleDateString('ru-RU', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    },
    
    async changeUserRole(userId, newRole) {
      if (!confirm(`Изменить роль пользователя на "${newRole}"?`)) return;
      
      try {
        await api.put(`/auth/users/${userId}/role`, { role: newRole });
        alert('Роль изменена');
        await this.loadData();
      } catch (error) {
        console.error('Ошибка изменения роли:', error);
        alert('Не удалось изменить роль');
      }
    },
    
    async deleteUser(userId) {
      if (!confirm('Вы уверены, что хотите удалить этого пользователя?')) return;
      
      try {
        await api.delete(`/auth/users/${userId}`);
        alert('Пользователь удалён');
        await this.loadData();
      } catch (error) {
        console.error('Ошибка удаления:', error);
        alert('Не удалось удалить пользователя');
      }
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
        id: '',
        name: '',
        category: '',
        price: 0,
        currency: '$',
        specifications: '',
        socket: '',
        powerConsumption: null,
        imageUrl: ''
      };
    },
    
    async saveComponent() {
      try {
        if (this.showEditComponentModal) {
          await api.put(`/components/${this.componentForm.id}`, this.componentForm);
          alert('Компонент обновлён');
        } else {
          await api.post('/components', this.componentForm);
          alert('Компонент создан');
        }
        this.closeModals();
        await this.loadData();
      } catch (error) {
        console.error('Ошибка сохранения:', error);
        alert(error.response?.data?.message || 'Не удалось сохранить компонент');
      }
    },
    
    async deleteComponent(componentId) {
      if (!confirm('Вы уверены, что хотите удалить этот компонент?')) return;
      
      try {
        await api.delete(`/components/${componentId}`);
        alert('Компонент удалён');
        await this.loadData();
      } catch (error) {
        console.error('Ошибка удаления:', error);
        alert('Не удалось удалить компонент');
      }
    }
  }
};
</script>

<style scoped>
.admin-page {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

h1 {
  color: #2c3e50;
  margin-bottom: 2rem;
}

h2 {
  color: #34495e;
  margin-bottom: 1rem;
  border-bottom: 2px solid #3498db;
  padding-bottom: 0.5rem;
}

.loading {
  text-align: center;
  padding: 3rem;
  font-size: 1.2rem;
  color: #7f8c8d;
}

/* Статистика */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1.5rem;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.stat-card h3 {
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
  opacity: 0.9;
}

.stat-value {
  font-size: 2.5rem;
  font-weight: bold;
  margin: 0;
}

/* Секции */
.admin-section {
  background: white;
  padding: 1.5rem;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

/* Таблицы */
.table-container {
  overflow-x: auto;
}

.admin-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}

.admin-table th,
.admin-table td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid #ecf0f1;
}

.admin-table th {
  background-color: #f8f9fa;
  font-weight: 600;
  color: #2c3e50;
}

.admin-table tr:hover {
  background-color: #f8f9fa;
}

.admin-table select {
  padding: 0.4rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  cursor: pointer;
}

.admin-table select:disabled {
  background-color: #f0f0f0;
  cursor: not-allowed;
}

/* Кнопки */
.btn-add {
  background: #27ae60;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  margin-bottom: 1rem;
  transition: background 0.3s;
}

.btn-add:hover {
  background: #219a52;
}

.btn-edit,
.btn-delete {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.2rem;
  padding: 0.3rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.btn-edit:hover {
  background: #f39c12;
}

.btn-delete:hover {
  background: #e74c3c;
}

.btn-delete:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

/* Модальное окно */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: white;
  padding: 2rem;
  border-radius: 10px;
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal h2 {
  margin-top: 0;
  border: none;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.4rem;
  font-weight: 500;
  color: #2c3e50;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}

.form-group textarea {
  min-height: 80px;
  resize: vertical;
}

.form-group input:disabled {
  background-color: #f0f0f0;
  cursor: not-allowed;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

.btn-cancel,
.btn-save {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  transition: background 0.3s;
}

.btn-cancel {
  background: #95a5a6;
  color: white;
}

.btn-cancel:hover {
  background: #7f8c8d;
}

.btn-save {
  background: #3498db;
  color: white;
}

.btn-save:hover {
  background: #2980b9;
}
</style>
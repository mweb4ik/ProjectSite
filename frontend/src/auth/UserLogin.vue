<template>
  <div class="auth-container">
    <!-- Loading Skeleton -->
    <div v-if="isLoading" class="skeleton-container">
      <div class="skeleton-title"></div>
      <div class="skeleton-form">
        <div class="skeleton-input" v-for="i in 4" :key="i"></div>
      </div>
      <div class="skeleton-button"></div>
      <div class="skeleton-link"></div>
    </div>

    <!-- Main Content -->
    <div v-else>
      <div class="particle-container">
        <span class="particle" v-for="i in 50" :key="i" :style="getParticleStyle(i)"></span>
      </div>

      <div class="auth-card">
        <h1 class="auth-title">Register</h1>
        <p class="auth-subtitle">Create your account</p>

        <form @submit.prevent="handleSubmit" class="auth-form">
          <div class="input-group">
            <label for="name" class="input-label">
              Имя
            </label>
            <input
              type="text"
              id="name"
              v-model="form.name"
              class="input-field"
              placeholder="Enter your name"
              required
            >
          </div>

          <div class="input-group">
            <label for="email" class="input-label">
              Email
            </label>
            <input
              type="email"
              id="email"
              v-model="form.email"
              class="input-field"
              placeholder="Enter your email"
              required
            >
          </div>

          <div class="input-group">
            <label for="password" class="input-label">
              <span class="label-icon"></span>
              Пароль
            </label>
            <input
              type="password"
              id="password"
              v-model="form.password"
              class="input-field"
              placeholder="Enter your password"
              required
            >
          </div>

          <div class="input-group">
            <label for="confirmPassword" class="input-label">
              Подтвердить пароль
            </label>
            <input
              type="password"
              id="confirmPassword"
              v-model="form.confirmPassword"
              class="input-field"
              placeholder="Confirm your password"
              required
            >
          </div>

          <div v-if="error" class="error-message">
            {{ error }}
          </div>

          <button type="submit" class="btn-submit" :disabled="isSubmitting">
            <span v-if="!isSubmitting">Зарегистрироваться</span>
            <span v-else class="loading-spinner">⏳</span>
          </button>
        </form>

        <div class="auth-footer">
          <p>Уже есть аккаунт?</p>
          <router-link to="/login" class="auth-link">Войти</router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'UserLogin',

  data() {
    return {
      isLoading: true,
      isSubmitting: false,
      error: '',
      form: {
        name: '',
        email: '',
        password: '',
        confirmPassword: ''
      }
    }
  },

  methods: {
    getParticleStyle(index) {
      const x = Math.random() * 100
      const duration = 5 + Math.random() * 10
      const delay = Math.random() * 5
      const size = 2 + Math.random() * 4
      return {
        left: x + '%',
        animationDuration: duration + 's',
        animationDelay: delay + 's',
        width: size + 'px',
        height: size + 'px'
      }
    },

    validateForm() {
      if (this.form.password.length < 6) {
        this.error = 'Password must be at least 6 characters'
        return false
      }
      if (this.form.password !== this.form.confirmPassword) {
        this.error = 'Passwords do not match'
        return false
      }
      this.error = ''
      return true
    },

    async handleSubmit() {
      if (!this.validateForm()) return

      this.isSubmitting = true
      this.error = ''

      try {
        // TODO: Add API call for registration
        console.log('Register:', {
          name: this.form.name,
          email: this.form.email,
          password: this.form.password
        })

        this.showNotification('Registration successful', 'success')
        this.$router.push('/login')
      } catch (err) {
        this.error = 'Registration failed. Please try again.'
        this.showNotification('Registration failed', 'error')
      } finally {
        this.isSubmitting = false
      }
    },

    showNotification(message, type) {
      console.log('[' + type + ']: ' + message)
    }
  },

  mounted() {
    setTimeout(() => {
      this.isLoading = false
    }, 1200)
  }
}
</script>

<style scoped>
/* ========== Global Styles ========== */
.auth-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  position: relative;
  overflow: hidden;
}

/* ========== Loading Skeleton ========== */
.skeleton-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 30px;
  width: 100%;
  max-width: 450px;
}

.skeleton-title {
  width: 60%;
  height: 45px;
  background: linear-gradient(90deg, #1a1a2e 25%, #2a2a4e 50%, #1a1a2e 75%);
  background-size: 200% 100%;
  border-radius: 12px;
  animation: skeleton-loading 1.5s ease-in-out infinite;
}

.skeleton-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
  width: 100%;
}

.skeleton-input {
  height: 55px;
  background: linear-gradient(90deg, #1a1a2e 25%, #2a2a4e 50%, #1a1a2e 75%);
  background-size: 200% 100%;
  border-radius: 12px;
  animation: skeleton-loading 1.5s ease-in-out infinite;
}

.skeleton-button {
  height: 52px;
  width: 100%;
  background: linear-gradient(90deg, #1a1a2e 25%, #2a2a4e 50%, #1a1a2e 75%);
  background-size: 200% 100%;
  border-radius: 14px;
  animation: skeleton-loading 1.5s ease-in-out infinite;
  animation-delay: 0.2s;
}

.skeleton-link {
  height: 20px;
  width: 50%;
  background: linear-gradient(90deg, #1a1a2e 25%, #2a2a4e 50%, #1a1a2e 75%);
  background-size: 200% 100%;
  border-radius: 8px;
  animation: skeleton-loading 1.5s ease-in-out infinite;
  animation-delay: 0.4s;
}

@keyframes skeleton-loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* ========== Auth Card ========== */
.auth-card {
  background: linear-gradient(135deg, rgba(20, 20, 40, 0.9), rgba(30, 30, 60, 0.9));
  border: 1px solid rgba(0, 255, 157, 0.2);
  border-radius: 20px;
  padding: 40px;
  width: 100%;
  max-width: 450px;
  position: relative;
  z-index: 10;
  box-shadow: 0 0 40px rgba(0, 255, 157, 0.1),
              0 0 80px rgba(0, 163, 255, 0.05);
  animation: cardFadeIn 0.6s ease-out;
}

@keyframes cardFadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* ========== Typography ========== */
.auth-title {
  font-family: 'Orbitron', sans-serif;
  font-size: 36px;
  font-weight: 700;
  letter-spacing: 2px;
  text-transform: uppercase;
  text-align: center;
  margin-bottom: 10px;
  background: linear-gradient(135deg, #00FF9D 0%, #00A3FF 50%, #00FF9D 100%);
  background-size: 200% auto;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  animation: gradientShift 3s ease infinite;
}

.auth-subtitle {
  font-family: 'Exo 2', sans-serif;
  font-size: 16px;
  color: rgba(255, 255, 255, 0.6);
  text-align: center;
  margin-bottom: 30px;
}

/* ========== Form Styles ========== */
.auth-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-label {
  font-family: 'Exo 2', sans-serif;
  font-size: 14px;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.8);
  display: flex;
  align-items: center;
  gap: 8px;
  letter-spacing: 0.5px;
}

.label-icon {
  font-size: 16px;
}

.input-field {
  background: rgba(13, 13, 13, 0.6);
  border: 1px solid rgba(0, 255, 157, 0.2);
  border-radius: 12px;
  padding: 14px 16px;
  font-family: 'Exo 2', sans-serif;
  font-size: 15px;
  color: #FFFFFF;
  transition: all 0.3s ease;
  outline: none;
}

.input-field::placeholder {
  color: rgba(255, 255, 255, 0.3);
}

.input-field:focus {
  border-color: #00FF9D;
  box-shadow: 0 0 20px rgba(0, 255, 157, 0.2);
  background: rgba(13, 13, 13, 0.8);
}

.input-field:hover:not(:focus) {
  border-color: rgba(0, 255, 157, 0.4);
}

/* ========== Error Message ========== */
.error-message {
  background: rgba(255, 71, 87, 0.1);
  border: 1px solid rgba(255, 71, 87, 0.3);
  border-radius: 10px;
  padding: 12px 16px;
  font-family: 'Exo 2', sans-serif;
  font-size: 14px;
  color: #FF4757;
  text-align: center;
  animation: errorShake 0.4s ease;
}

@keyframes errorShake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
}

/* ========== Submit Button ========== */
.btn-submit {
  background: linear-gradient(135deg, #00FF9D 0%, #00A3FF 100%);
  color: #0D0D0D;
  padding: 16px 28px;
  font-family: 'Rajdhani', sans-serif;
  border: none;
  border-radius: 14px;
  font-size: 17px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  letter-spacing: 1px;
  text-transform: uppercase;
  position: relative;
  overflow: hidden;
  margin-top: 10px;
}

.btn-submit::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.3), transparent);
  transition: left 0.5s;
}

.btn-submit:hover::before {
  left: 100%;
}

.btn-submit:hover:not(:disabled) {
  transform: translateY(-3px) scale(1.02);
  box-shadow: 0 10px 40px rgba(0, 255, 157, 0.4),
              0 0 60px rgba(0, 163, 255, 0.2);
}

.btn-submit:active:not(:disabled) {
  transform: translateY(-1px) scale(0.98);
}

.btn-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading-spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* ========== Footer ========== */
.auth-footer {
  text-align: center;
  margin-top: 25px;
  padding-top: 25px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.auth-footer p {
  font-family: 'Exo 2', sans-serif;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.5);
  margin-bottom: 10px;
}

.auth-link {
  font-family: 'Exo 2', sans-serif;
  font-size: 15px;
  font-weight: 600;
  color: #00FF9D;
  text-decoration: none;
  transition: all 0.3s ease;
  position: relative;
}

.auth-link::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  width: 0;
  height: 2px;
  background: #00FF9D;
  transition: width 0.3s ease;
}

.auth-link:hover {
  color: #00A3FF;
}

.auth-link:hover::after {
  width: 100%;
}

/* ========== Particles ========== */
.particle-container {
  width: 100%;
  height: 100%;
  position: fixed;
  top: 0;
  left: 0;
  pointer-events: none;
  overflow: hidden;
  z-index: 0;
}

.particle {
  position: absolute;
  background: rgba(255, 255, 255, 0.8);
  border-radius: 50%;
  animation: fall linear infinite;
  opacity: 0;
}

.particle:nth-child(odd) {
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  box-shadow: 0 0 8px rgba(0, 255, 157, 0.8);
}

.particle:nth-child(even) {
  background: linear-gradient(135deg, #00A3FF, #FFFFFF);
  box-shadow: 0 0 8px rgba(0, 163, 255, 0.8);
}

.particle:nth-child(3n) {
  border-radius: 0%;
}

.particle:nth-child(5n) {
  border-radius: 30%;
}

@keyframes fall {
  0% {
    transform: translateY(-10px) translateX(0) rotate(0deg);
    opacity: 0;
  }
  10% {
    opacity: 1;
  }
  90% {
    opacity: 1;
  }
  100% {
    transform: translateY(100vh) translateX(30px) rotate(360deg);
    opacity: 0;
  }
}

/* ========== Responsive Design ========== */
@media (max-width: 768px) {
  .auth-card {
    padding: 30px 25px;
  }

  .auth-title {
    font-size: 28px;
  }

  .input-field {
    padding: 12px 14px;
    font-size: 14px;
  }

  .btn-submit {
    padding: 14px 24px;
    font-size: 16px;
  }
}

@media (max-width: 460px) {
  .auth-container {
    padding: 20px 15px;
  }

  .auth-card {
    padding: 25px 20px;
    border-radius: 16px;
  }

  .auth-title {
    font-size: 24px;
  }

  .auth-subtitle {
    font-size: 14px;
  }

  .btn-submit {
    padding: 13px 20px;
    font-size: 15px;
  }
}
</style>

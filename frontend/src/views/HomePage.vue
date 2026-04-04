<template>
  <div id="app">
    <header class="header">
    </header>

    <main class="content">
      <div class="sections-grid">
        <button @click="goTo('/lab')" class="section-btn">⚡ Лаборатория разгона</button>
        <button @click="goTo('/quiz')" class="section-btn">🧠 Викторина</button>
        <button @click="goTo('/builder')" class="section-btn">🛠️ Сборка ПК</button>
        <button @click="goTo('/bios')" class="section-btn">💾 BIOS / UEFI</button>
        <button @click="goTo('/profile')" class="section-btn">👤 Личный кабинет</button>
        <button @click="goTo('/admin')" class="section-btn">🛡️ Админ панель</button>
      </div>
      <h1 class="highlight">Познай Внутреннее устройство компьютера</h1>
      <div class="user-info">
        <span class="user-email">{{ user.email }}</span>
        <span class="role-badge" :class="user.role">{{ user.role }}</span>
        <button class="btn btn-outline" @click="logout">Выйти</button>
      </div>
       <!--Скелетон-->
  <div v-if="loading" class="skeleton-wrapper">
    <div class="skeleton-img"></div>
    <div class="skeleton-text"></div>
    <div class="skeleton-text small"></div>

    <div class="skeleton-buttons">
      <div v-for="i in 6" :key="i" class="skeleton-btn"></div>
    </div>
    </div>
  <div v-else>
      <img src="/images/pc.png" alt="Компьютер" class="hero-img" />
      <p class="welcome-text">Добро пожаловать, {{ user.email }}!</p>
      <p class="subtitle">Выберите компонент для изучения</p>
      <div class="buttons-grid" >
        <button class="videocard-btn  green" @click="goTo('videocard')" type="button">
          <span class="emoji-videocard">📟</span>
          Видеокарта
        </button>
        <button class="processor-btn red"  @click="goTo('processor')" type="button">
          <span class="emoji-processor">🔲</span>
          Процессор
        </button>
        <button class="motherboard-btn blue"  @click="goTo('motherboard')" type="button">
          <span class="emoji-motherboard">🀆</span>
          Материнская плата
        </button>
        <button class="cooling-btn yellow"  @click="goTo('cooling')" type="button">
          <span class="emoji-cooling">𖣘</span>
          Охлаждение
        </button>
        <button class="ram-btn yellow" @click="goTo('ram') " type="button">
          <span class="emoji-ram">𝐑𝐚𝐦</span>
          Оперативная память
        </button>
        <button class="storage-btn dark"  @click="goTo('storage')" type="button">
          <span class="emoji-storage">🗄️</span>
          Накопитель
        </button>
      </div>
      </div>
    </main>
  </div>
</template>

<script>
import { useRouter } from 'vue-router'

export default {
  name: 'HomePage',
  setup() {
    const router = useRouter()
    return { router }
  },
  data() {
    return {
      user: { email: '', role: '' },
      loading:true
    }
  },
  mounted() {
    const token = localStorage.getItem('token')
    if (!token) {
      this.router.push('/')
      return
    }
    setTimeout(() => {
      const saved = localStorage.getItem('user')
if (saved) this.user = JSON.parse(saved)
this.loading = false
  }, 1000)
},
  methods: {
    logout() {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      this.router.push('/')
    },

  goTo(page){
    this.router.push('/' + page);
  }
}
}
</script>

<style scoped>
.buttons-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
  margin-top: 20px;
  margin-bottom : 40px;
  margin-left: 10px;
  margin-right: 10px;

}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
  padding: 24px 0;
  border-bottom: 1px solid rgba(0, 163, 255, 0.2);
  margin-bottom: 40px;
}

.header h1 {
  font-family: 'Orbitron', sans-serif;
  font-size: clamp(16px, 2.5vw, 25px);
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-email {
  color: #aaa;
  font-size: 14px;
}

.hero-img {
  max-width: 50%;
  border: 2px solid #00A3FF;
  border-radius: 12px;
  margin: 0 auto 30px;
  display: block;
  box-shadow: 0 0 30px rgba(0, 163, 255, 0.5);
  transition: all 0.3s ease;
}

.hero-img:hover {
  transform: translateY(4px);
  box-shadow: 0 0 30px rgba(0, 255, 157, 0.1);
}
.content {
  text-align: center;
}

.subtitle {
  color: #888;
  font-size: 18px;
  margin-bottom: 20px;
}

.videocard-btn,
.processor-btn,
.motherboard-btn,
.cooling-btn,
.ram-btn,
.storage-btn {
  padding: 15px 30px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  cursor: pointer;
  font-weight: bold;
  transition: background-color 0.3s ease;
}

.videocard-btn:hover,
.processor-btn:hover,
.motherboard-btn:hover,
.cooling-btn:hover,
.ram-btn:hover,
.storage-btn:hover {
  filter: brightness(1.2);
}


.green { background-color: #28a745; color: white; }
.red { background-color: #dc3545; color: white; }
.blue { background-color: #17a2b8; color: white; }
.yellow { background-color: #ffc107; color: black; }
.gray { background-color: #6c757d; color: white; }
.dark { background-color: #424242; color: white; }

/*Анимация скелетона*/
@keyframes shimmer {
  0% {
    background-position: -500px 0;
  }
  100% {
    background-position: 500px 0;
  }
}


.skeleton-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
}

/* общий стиль */
.skeleton-img,
.skeleton-text,
.skeleton-btn {
  background: linear-gradient(
    90deg,
    #1e1e1e 25%,
    #2a2a2a 50%,
    #1e1e1e 75%
  );
  background-size: 1000px 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 10px;
}

.skeleton-img {
  width: 50%;
  height: 200px;
  margin-bottom: 20px;
}

.skeleton-text {
  width: 60%;
  height: 20px;
  margin-bottom: 10px;
}

.skeleton-text.small {
  width: 40%;
}

.skeleton-buttons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
  width: 100%;
  margin-top: 20px;
}

.skeleton-btn {
  height: 50px;
}
.link {
  background: none;
  border: none;
  color: #00A3FF;
  cursor: pointer;
  text-decoration: underline;
}
.left-side {
  display: flex;
  align-items: center;
}

.btn-profile {
  background: linear-gradient(135deg, #00A3FF, #00FF9D);
  color: white;
  border-radius: 8px;
  padding: 8px 14px;
  font-size: 14px;
  transition: 0.3s;
}

.btn-profile:hover {
  filter: brightness(1.2);
  transform: translateY(-1px);
}
.sections-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
  margin-top: 30px;
}

.section-btn {
  padding: 15px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  font-weight: bold;
  background: linear-gradient(135deg, #00FF9D, #00A3FF);
  color: white;
  transition: 0.3s;
  box-shadow: 0 0 15px rgba(0, 255, 157, 0.3);
}

.section-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 0 25px rgba(0, 163, 255, 0.6);
}
</style>

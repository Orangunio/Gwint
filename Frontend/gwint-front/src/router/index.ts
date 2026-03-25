import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/pages/HomeView.vue'
import LoginView from '@/pages/LoginView.vue'
import RegisterView from '@/pages/RegisterView.vue'
import CreateRoom from '@/pages/CreateRoom.vue'
import JoinRoom from '@/pages/JoinRoom.vue'
import Room from '@/pages/Room.vue'
import { usePlayerStore } from '@/stores/player'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
      meta: { guestOnly: true },
    },
    {
      path: '/create-room',
      name: 'create-room',
      component: CreateRoom,
      meta: { requiresAuth: true },
    },
    {
      path: '/join',
      name: 'join-room',
      component: JoinRoom,
      meta: { requiresAuth: true },
    },
    {
      path: '/room/:roomId',
      name: 'Room',
      component: Room,
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach(to => {
  const playerStore = usePlayerStore()

  if (to.meta.guestOnly && playerStore.isLoggedIn) {
    return { name: 'home' }
  }

  if (to.meta.requiresAuth && !playerStore.isLoggedIn) {
    return { name: 'login' }
  }

  return true
})

export default router
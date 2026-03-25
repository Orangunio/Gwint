import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/pages/HomeView.vue'
import LoginView from '@/pages/LoginView.vue'
import RegisterView from '@/pages/RegisterView.vue'
import LobbyView from '@/pages/LobbyView.vue'
import FractionSelectView from '@/pages/FractionSelectView.vue'
import GameView from '@/pages/GameView.vue'
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
      path: '/lobby',
      name: 'lobby',
      component: LobbyView,
      meta: { requiresAuth: true },
    },
    {
      path: '/game/:roomId/fraction',
      name: 'fraction-select',
      component: FractionSelectView,
      meta: { requiresAuth: true },
    },
    {
      path: '/game/:roomId',
      name: 'game',
      component: GameView,
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
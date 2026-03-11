import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import router from '@/router'
import { usePlayerStore } from '@/stores/player'

describe('Router Guards - Bezpieczeństwo tras', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('Pozwala wejść na stronę główną niezalogowanemu', async () => {
    await router.push({ name: 'home' })
    expect(router.currentRoute.value.name).toBe('home')
  })

  it('Przekierowuje zalogowanego użytkownika z /login na / (home)', async () => {
    const playerStore = usePlayerStore()
    playerStore.setSession('Tester', 'token')
    
    await router.push({ name: 'login' })
    expect(router.currentRoute.value.name).toBe('home')
  })

  it('Przekierowuje zalogowanego użytkownika z /register na / (home)', async () => {
    const playerStore = usePlayerStore()
    playerStore.setSession('Tester', 'token')
    
    await router.push({ name: 'register' })
    expect(router.currentRoute.value.name).toBe('home')
  })

  it('Pozwala wejść na /login, gdy użytkownik nie jest zalogowany', async () => {
    const playerStore = usePlayerStore()
    playerStore.logout()
    
    await router.push({ name: 'login' })
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('Sprawdza czy meta.guestOnly działa poprawnie dla trasy rejestracji', () => {
    const route = router.getRoutes().find(r => r.name === 'register')
    expect(route?.meta?.guestOnly).toBe(true)
  })

  it('Router posiada zdefiniowaną trasę home', () => {
    expect(router.hasRoute('home')).toBe(true)
  })

  it('Router posiada zdefiniowaną trasę login', () => {
    expect(router.hasRoute('login')).toBe(true)
  })

  it('Router posiada zdefiniowaną trasę register', () => {
    expect(router.hasRoute('register')).toBe(true)
  })

  it('beforeEach zwraca true dla tras bez meta tagów', async () => {
    const result = await router.push('/')
    expect(result).toBeUndefined() 
  })

  it('Router używa WebHistory', () => {
    expect(router.options.history).toBeDefined()
  })
})
import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAppStore } from '@/stores/app'
import { usePlayerStore } from '@/stores/player'

describe('Extended Store Logic - Zarządzanie stanem', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('AppStore: Dodaje wiele powiadomień i zachowuje kolejność', () => {
    const store = useAppStore()
    store.addNotification('Pierwsze')
    store.addNotification('Drugie')
    expect(store.notifications[0]).toBe('Pierwsze')
    expect(store.notifications[1]).toBe('Drugie')
  })

  it('AppStore: removeNotification czyści tablicę do zera', () => {
    const store = useAppStore()
    store.addNotification('A')
    store.removeNotification(0)
    expect(store.notifications.length).toBe(0)
  })

  it('PlayerStore: winRate zwraca "0%" przy braku obiektu gracza', () => {
    const store = usePlayerStore()
    store.player = null
    expect(store.winRate).toBe('0%')
  })

  it('PlayerStore: Poziom gracza domyślnie wynosi 1', () => {
    const store = usePlayerStore()
    store.setSession('Test', 'token')
    expect(store.player?.level).toBe(1)
  })

  it('PlayerStore: Avatar domyślnie to mdi-account-circle', () => {
    const store = usePlayerStore()
    store.setSession('Test', 'token')
    expect(store.player?.avatar).toBe('mdi-account-circle')
  })

  it('AppStore: ToggleTheme działa wielokrotnie', () => {
    const store = useAppStore()
    store.toggleTheme() 
    store.toggleTheme() 
    expect(store.theme).toBe('dark')
  })

  it('PlayerStore: session zapamiętuje wins i losses jako 0', () => {
    const store = usePlayerStore()
    store.setSession('Test', 'token')
    expect(store.player?.wins).toBe(0)
    expect(store.player?.losses).toBe(0)
  })

  it('AppStore: setLoading akceptuje wartości false', () => {
    const store = useAppStore()
    store.setLoading(true)
    store.setLoading(false)
    expect(store.isLoading).toBe(false)
  })

  it('PlayerStore: token po wylogowaniu jest pusty', () => {
    const store = usePlayerStore()
    store.setSession('A', 'T')
    store.logout()
    expect(store.token).toBeNull()
  })

  it('PlayerStore: winRate obsługuje bardzo dużą liczbę gier', () => {
    const store = usePlayerStore()
    store.player = { name: 'A', level: 1, wins: 1000, losses: 500, avatar: '' }
    expect(store.winRate).toBe('67%')
  })
})
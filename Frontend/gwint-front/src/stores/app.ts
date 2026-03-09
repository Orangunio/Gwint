import { defineStore } from 'pinia'

export const useAppStore = defineStore('app', {
  state: () => ({
    theme: 'dark' as 'dark' | 'light',
    isLoading: false,
    notifications: [] as string[],
  }),

  actions: {
    toggleTheme() {
      this.theme = this.theme === 'dark' ? 'light' : 'dark'
    },
    setLoading(value: boolean) {
      this.isLoading = value
    },
    addNotification(message: string) {
      this.notifications.push(message)
    },
    removeNotification(index: number) {
      this.notifications.splice(index, 1)
    },
  },
})
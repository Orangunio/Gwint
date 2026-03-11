import type { AuthRequest } from '@/types/auth'
import { defineStore } from 'pinia'
import * as authApi from '@/api/auth'

const TOKEN_KEY = 'gwint_token'
const LOGIN_KEY = 'gwint_login'

export interface Player {
  name: string
  level: number
  wins: number
  losses: number
  avatar: string
}

interface AuthState {
  player: Player | null
  token: string | null
  isLoggedIn: boolean
  isLoading: boolean
  error: string | null
}

function getStoredToken (): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

function getStoredLogin (): string | null {
  return localStorage.getItem(LOGIN_KEY)
}

export const usePlayerStore = defineStore('player', {
  state: (): AuthState => ({
    player: getStoredLogin()
      ? {
          name: getStoredLogin() as string,
          level: 1,
          wins: 0,
          losses: 0,
          avatar: 'mdi-account-circle',
        }
      : null,
    token: getStoredToken(),
    isLoggedIn: !!getStoredToken(),
    isLoading: false,
    error: null,
  }),

  getters: {
    winRate: (state): string => {
      if (!state.player) {
        return '0%'
      }
      const total = state.player.wins + state.player.losses
      if (total === 0) {
        return '0%'
      }
      return `${Math.round((state.player.wins / total) * 100)}%`
    },

    displayName: (state): string => state.player?.name ?? 'Gość',
  },

  actions: {
    clearError () {
      this.error = null
    },

    setSession (login: string, token: string) {
      this.token = token
      this.isLoggedIn = true
      this.player = {
        name: login,
        level: 1,
        wins: 0,
        losses: 0,
        avatar: 'mdi-account-circle',
      }

      localStorage.setItem(TOKEN_KEY, token)
      localStorage.setItem(LOGIN_KEY, login)
    },

    async register (payload: AuthRequest) {
      this.isLoading = true
      this.error = null

      try {
        await authApi.register(payload)
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Nie udało się zarejestrować użytkownika.'
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async login (payload: AuthRequest) {
      this.isLoading = true
      this.error = null

      try {
        const response = await authApi.login(payload)
        this.setSession(payload.login, response.token)
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Nie udało się zalogować.'
        throw error
      } finally {
        this.isLoading = false
      }
    },

    logout () {
      this.player = null
      this.token = null
      this.isLoggedIn = false
      this.error = null

      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(LOGIN_KEY)
    },
  },
})

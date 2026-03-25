import type { AuthRequest } from '@/types/auth'
import { defineStore } from 'pinia'
import * as authApi from '@/api/auth'
import { http } from '@/api/http'

const TOKEN_KEY = 'gwint_token'
const LOGIN_KEY = 'gwint_login'
const PLAYER_ID_KEY = 'gwint_player_id'

export interface Player {
  id: number
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

function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

function getStoredLogin(): string | null {
  return localStorage.getItem(LOGIN_KEY)
}

function getStoredPlayerId(): number | null {
  const id = localStorage.getItem(PLAYER_ID_KEY)
  return id ? parseInt(id, 10) : null
}

export const usePlayerStore = defineStore('player', {
  state: (): AuthState => ({
    player: getStoredLogin()
      ? {
          id: getStoredPlayerId() ?? 0,
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
      if (!state.player) return '0%'
      const total = state.player.wins + state.player.losses
      if (total === 0) return '0%'
      return `${Math.round((state.player.wins / total) * 100)}%`
    },
    displayName: (state): string => state.player?.name ?? 'Gość',
    playerId: (state): number => state.player?.id ?? 0,
  },

  actions: {
    clearError() {
      this.error = null
    },

    setSession(login: string, token: string, id?: number) {
      this.token = token
      this.isLoggedIn = true
      this.player = {
        id: id ?? 0,
        name: login,
        level: 1,
        wins: 0,
        losses: 0,
        avatar: 'mdi-account-circle',
      }
      localStorage.setItem(TOKEN_KEY, token)
      localStorage.setItem(LOGIN_KEY, login)
      if (id) localStorage.setItem(PLAYER_ID_KEY, String(id))
    },

    async fetchMe() {
      if (!this.token) return
      try {
        const data = await http<{ id: number; login: string }>('/player/me', {
          token: this.token,
        })
        if (this.player) {
          this.player.id = data.id
          localStorage.setItem(PLAYER_ID_KEY, String(data.id))
        }
      } catch {
      }
    },

    async register(payload: AuthRequest) {
      this.isLoading = true
      this.error = null
      try {
        await authApi.register(payload)
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Nie udało się zarejestrować.'
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async login(payload: AuthRequest) {
      this.isLoading = true
      this.error = null
      try {
        const response = await authApi.login(payload)
        this.setSession(payload.login, response.token)
        await this.fetchMe()
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Nie udało się zalogować.'
        throw error
      } finally {
        this.isLoading = false
      }
    },

    logout() {
      this.player = null
      this.token = null
      this.isLoggedIn = false
      this.error = null
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(LOGIN_KEY)
      localStorage.removeItem(PLAYER_ID_KEY)
    },
  },
})
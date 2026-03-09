import { defineStore } from 'pinia'

export interface Player {
    name: string
    level: number
    wins: number
    losses: number
    avatar: string
}

export const usePlayerStore = defineStore('player', {
    state: () => ({
        player: null as Player | null,
        isLoggedIn: false,
    }),

    getters: {
        winRate: (state): string => {
            if (!state.player) return '0%'
            const total = state.player.wins + state.player.losses
            if (total === 0) return '0%'
            return `${Math.round((state.player.wins / total) * 100)}%`
        },
        displayName: (state): string => state.player?.name ?? 'Gość',
    },

    actions: {
        login(name: string) {
            this.player = {
                name,
                level: 1,
                wins: 0,
                losses: 0,
                avatar: 'mdi-account-circle',
            }
            this.isLoggedIn = true
        },
        logout() {
            this.player = null
            this.isLoggedIn = false
        },
    },
})
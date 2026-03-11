import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import PlayerStats from '../PlayerStats.vue'

type MockPlayer = {
    name: string
    level: number
    wins: number
    losses: number
    avatar: string
}

type MockPlayerStore = {
    isLoggedIn: boolean
    player: MockPlayer | null
    winRate: string
}

const mockPlayerStore: MockPlayerStore = {
    isLoggedIn: false,
    player: null,
    winRate: '0%',
}

vi.mock('@/stores/player', () => ({
    usePlayerStore: () => mockPlayerStore,
}))

describe('PlayerStats', () => {
    beforeEach(() => {
        mockPlayerStore.isLoggedIn = false
        mockPlayerStore.player = null
        mockPlayerStore.winRate = '0%'
    })

    it('nie renderuje się, gdy użytkownik nie jest zalogowany', () => {
        const wrapper = mount(PlayerStats)

        expect(wrapper.find('.stats-card').exists()).toBe(false)
        expect(wrapper.text()).toBe('')
    })

    it('renderuje nazwę gracza, poziom i win rate dla zalogowanego użytkownika', () => {
        mockPlayerStore.isLoggedIn = true
        mockPlayerStore.player = {
            name: 'MarekTowarek',
            level: 7,
            wins: 8,
            losses: 2,
            avatar: 'mdi-account-circle',
        }
        mockPlayerStore.winRate = '80%'

        const wrapper = mount(PlayerStats)

        expect(wrapper.find('.stats-card').exists()).toBe(true)
        expect(wrapper.text()).toContain('MarekTowarek')
        expect(wrapper.text()).toContain('Poziom 7')
        expect(wrapper.text()).toContain('80%')
        expect(wrapper.text()).toContain('Win Rate')
    })

    it('renderuje statystyki wygranych, przegranych i razem', () => {
        mockPlayerStore.isLoggedIn = true
        mockPlayerStore.player = {
            name: 'MarekTowarek',
            level: 3,
            wins: 12,
            losses: 5,
            avatar: 'mdi-account-circle',
        }
        mockPlayerStore.winRate = '71%'

        const wrapper = mount(PlayerStats)

        expect(wrapper.text()).toContain('Wygrane')
        expect(wrapper.text()).toContain('12')

        expect(wrapper.text()).toContain('Przegrane')
        expect(wrapper.text()).toContain('5')

        expect(wrapper.text()).toContain('Razem')
        expect(wrapper.text()).toContain('17')
    })

    it('renderuje dokładnie 3 bloki statystyk', () => {
        mockPlayerStore.isLoggedIn = true
        mockPlayerStore.player = {
            name: 'MarekTowarek',
            level: 3,
            wins: 4,
            losses: 6,
            avatar: 'mdi-account-circle',
        }
        mockPlayerStore.winRate = '40%'

        const wrapper = mount(PlayerStats)
        const statBlocks = wrapper.findAll('.stat-block')

        expect(statBlocks).toHaveLength(3)
    })

    it('poprawnie liczy wartość "Razem"', () => {
        mockPlayerStore.isLoggedIn = true
        mockPlayerStore.player = {
            name: 'Tester',
            level: 2,
            wins: 9,
            losses: 11,
            avatar: 'mdi-account-circle',
        }
        mockPlayerStore.winRate = '45%'

        const wrapper = mount(PlayerStats)
        const statBlocks = wrapper.findAll('.stat-block')

        expect(statBlocks[2].text()).toContain('Razem')
        expect(statBlocks[2].text()).toContain('20')
    })

    it('renderuje zera w statystykach, gdy gracz ma 0 wygranych i 0 przegranych', () => {
        mockPlayerStore.isLoggedIn = true
        mockPlayerStore.player = {
            name: 'NowyGracz',
            level: 1,
            wins: 0,
            losses: 0,
            avatar: 'mdi-account-circle',
        }
        mockPlayerStore.winRate = '0%'

        const wrapper = mount(PlayerStats)

        expect(wrapper.text()).toContain('NowyGracz')
        expect(wrapper.text()).toContain('Poziom 1')
        expect(wrapper.text()).toContain('0%')
        expect(wrapper.text()).toContain('Wygrane')
        expect(wrapper.text()).toContain('Przegrane')
        expect(wrapper.text()).toContain('Razem')
    })
})
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { usePlayerStore } from '../player'
import * as authApi from '@/api/auth.ts'

vi.mock('@/api/auth', () => ({
    register: vi.fn(),
    login: vi.fn(),
}))

describe('player store', () => {
    beforeEach(() => {
        setActivePinia(createPinia())
        localStorage.clear()
        vi.clearAllMocks()
    })

    it('inicjalizuje się pustym stanem, gdy localStorage jest pusty', () => {
        const store = usePlayerStore()

        expect(store.player).toBe(null)
        expect(store.token).toBe(null)
        expect(store.isLoggedIn).toBe(false)
        expect(store.isLoading).toBe(false)
        expect(store.error).toBe(null)
    })

    it('inicjalizuje się z localStorage, gdy zapisano login i token', () => {
        localStorage.setItem('gwint_login', 'MarekTowarek')
        localStorage.setItem('gwint_token', 'abc-token')

        const store = usePlayerStore()

        expect(store.token).toBe('abc-token')
        expect(store.isLoggedIn).toBe(true)
        expect(store.player).toEqual({
            name: 'MarekTowarek',
            level: 1,
            wins: 0,
            losses: 0,
            avatar: 'mdi-account-circle',
        })
    })

    it('getter winRate zwraca 0%, gdy brak gracza', () => {
        const store = usePlayerStore()

        expect(store.winRate).toBe('0%')
    })

    it('getter winRate zwraca 0%, gdy liczba gier wynosi 0', () => {
        const store = usePlayerStore()

        store.player = {
            name: 'MarekTowarek',
            level: 1,
            wins: 0,
            losses: 0,
            avatar: 'mdi-account-circle',
        }

        expect(store.winRate).toBe('0%')
    })

    it('getter winRate poprawnie oblicza procent wygranych', () => {
        const store = usePlayerStore()

        store.player = {
            name: 'MarekTowarek',
            level: 1,
            wins: 8,
            losses: 2,
            avatar: 'mdi-account-circle',
        }

        expect(store.winRate).toBe('80%')
    })

    it('getter displayName zwraca "Gość", gdy brak gracza', () => {
        const store = usePlayerStore()

        expect(store.displayName).toBe('Gość')
    })

    it('getter displayName zwraca nazwę gracza', () => {
        const store = usePlayerStore()

        store.player = {
            name: 'MarekTowarek',
            level: 1,
            wins: 0,
            losses: 0,
            avatar: 'mdi-account-circle',
        }

        expect(store.displayName).toBe('MarekTowarek')
    })

    it('clearError czyści błąd', () => {
        const store = usePlayerStore()
        store.error = 'Jakiś błąd'

        store.clearError()

        expect(store.error).toBe(null)
    })

    it('setSession ustawia sesję i zapisuje dane do localStorage', () => {
        const store = usePlayerStore()

        store.setSession('MarekTowarek', 'token-123')

        expect(store.token).toBe('token-123')
        expect(store.isLoggedIn).toBe(true)
        expect(store.player).toEqual({
            name: 'MarekTowarek',
            level: 1,
            wins: 0,
            losses: 0,
            avatar: 'mdi-account-circle',
        })

        expect(localStorage.getItem('gwint_token')).toBe('token-123')
        expect(localStorage.getItem('gwint_login')).toBe('MarekTowarek')
    })

    it('logout czyści stan i localStorage', () => {
        const store = usePlayerStore()

        store.setSession('MarekTowarek', 'token-123')
        store.error = 'Błąd'

        store.logout()

        expect(store.player).toBe(null)
        expect(store.token).toBe(null)
        expect(store.isLoggedIn).toBe(false)
        expect(store.error).toBe(null)
        expect(localStorage.getItem('gwint_token')).toBe(null)
        expect(localStorage.getItem('gwint_login')).toBe(null)
    })

    it('register wywołuje API i kończy bez błędu przy sukcesie', async () => {
        vi.mocked(authApi.register).mockResolvedValue(undefined)

        const store = usePlayerStore()
        const payload = {
            login: 'MarekTowarek',
            password: 'tajnehaslo',
        }

        await store.register(payload)

        expect(authApi.register).toHaveBeenCalledWith(payload)
        expect(store.isLoading).toBe(false)
        expect(store.error).toBe(null)
    })

    it('register ustawia error i rzuca wyjątek przy błędzie API', async () => {
        vi.mocked(authApi.register).mockRejectedValue(new Error('Login zajęty'))

        const store = usePlayerStore()
        const payload = {
            login: 'MarekTowarek',
            password: 'tajnehaslo',
        }

        await expect(store.register(payload)).rejects.toThrow('Login zajęty')

        expect(store.error).toBe('Login zajęty')
        expect(store.isLoading).toBe(false)
    })

    it('login wywołuje API i ustawia sesję przy sukcesie', async () => {
        vi.mocked(authApi.login).mockResolvedValue({
            token: 'jwt-token',
        })

        const store = usePlayerStore()
        const payload = {
            login: 'MarekTowarek',
            password: 'tajnehaslo',
        }

        await store.login(payload)

        expect(authApi.login).toHaveBeenCalledWith(payload)
        expect(store.token).toBe('jwt-token')
        expect(store.isLoggedIn).toBe(true)
        expect(store.player?.name).toBe('MarekTowarek')
        expect(store.error).toBe(null)
        expect(store.isLoading).toBe(false)
        expect(localStorage.getItem('gwint_token')).toBe('jwt-token')
        expect(localStorage.getItem('gwint_login')).toBe('MarekTowarek')
    })

    it('login ustawia error i nie loguje użytkownika przy błędzie API', async () => {
        vi.mocked(authApi.login).mockRejectedValue(new Error('Nieprawidłowe hasło'))

        const store = usePlayerStore()
        const payload = {
            login: 'MarekTowarek',
            password: 'zlehaslo',
        }

        await expect(store.login(payload)).rejects.toThrow('Nieprawidłowe hasło')

        expect(store.error).toBe('Nieprawidłowe hasło')
        expect(store.isLoggedIn).toBe(false)
        expect(store.token).toBe(null)
        expect(store.player).toBe(null)
        expect(store.isLoading).toBe(false)
    })
})
import { test, expect } from '@playwright/test'
import { mockLoginSuccess, mockLoginError, mockFetchMe } from './helpers/auth'

test.describe('Login', () => {
    test('użytkownik może się zalogować', async ({ page }) => {
        await mockLoginSuccess(page)

        await page.goto('/login')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('geralt')
        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('tajnehaslo')

        await form.getByRole('button', { name: 'Zaloguj się', exact: true }).click()

        await expect(page).toHaveURL('/')

        await expect(page.getByText('geralt')).toBeVisible()
        await expect(page.locator('header').getByText('Poziom 1')).toBeVisible()
        await expect(page.getByText('Win Rate')).toBeVisible()
        await expect(page.getByText('Dostępne')).toBeVisible()
    })

    test('pokazuje błąd logowania, gdy API zwraca 401', async ({ page }) => {
        await mockLoginError(page, 'Nieprawidłowe dane logowania.')

        await page.goto('/login')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('geralt')
        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('zlehaslo')

        await form.getByRole('button', { name: 'Zaloguj się', exact: true }).click()

        await expect(page).toHaveURL(/\/login/)
        await expect(page.getByText('Nieprawidłowe dane logowania.')).toBeVisible()
    })

    test('zalogowany użytkownik nie powinien wejść na /login', async ({ page }) => {
        await page.goto('/')

        await page.evaluate(() => {
            localStorage.setItem('gwint_token', 'seed-token')
            localStorage.setItem('gwint_login', 'geralt')
        })

        await page.goto('/login')

        await expect(page).toHaveURL('/')
        await expect(page.getByText('geralt')).toBeVisible()
    })

    test('po logowaniu pobiera player id i zapisuje je w localStorage', async ({ page }) => {
        await mockLoginSuccess(page)
        await mockFetchMe(page, 777, 'geralt')

        await page.goto('/login')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('geralt')
        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('tajnehaslo')
        await form.getByRole('button', { name: 'Zaloguj się', exact: true }).click()

        await expect(page).toHaveURL('/')

        const storage = await page.evaluate(() => ({
            token: localStorage.getItem('gwint_token'),
            login: localStorage.getItem('gwint_login'),
            playerId: localStorage.getItem('gwint_player_id'),
        }))

        expect(storage.token).toBe('fake-jwt-token')
        expect(storage.login).toBe('geralt')
        expect(storage.playerId).toBe('777')
    })
})
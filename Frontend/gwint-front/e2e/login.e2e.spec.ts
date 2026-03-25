import { test, expect } from '@playwright/test'
import { mockLoginSuccess, mockLoginError } from './helpers/auth'

test.describe('Login', () => {
    test('użytkownik może się zalogować', async ({ page }) => {
        await mockLoginSuccess(page)

        await page.goto('/login')

        await expect(page.getByRole('heading', { name: /Zaloguj się do GWINTA/i })).toBeVisible()

        await page.getByLabel('Login').fill('geralt')
        await page.getByLabel('Hasło').fill('tajnehaslo')

        await page.getByRole('button', { name: 'Zaloguj się' }).click()

        await expect(page).toHaveURL('/')

        await expect(page.getByText('Poziom 1')).toBeVisible()
        await expect(page.getByText('geralt')).toBeVisible()
        await expect(page.getByText('Win Rate')).toBeVisible()
        await expect(page.getByText('Dostępne')).toBeVisible()
    })

    test('pokazuje błąd logowania, gdy API zwraca 401', async ({ page }) => {
        await mockLoginError(page, 'Nieprawidłowe dane logowania.')

        await page.goto('/login')

        await page.getByLabel('Login').fill('geralt')
        await page.getByLabel('Hasło').fill('zlehaslo')

        await page.getByRole('button', { name: 'Zaloguj się' }).click()

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
})
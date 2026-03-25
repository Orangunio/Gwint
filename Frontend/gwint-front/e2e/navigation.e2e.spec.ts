import { test, expect } from '@playwright/test'
import { seedLoggedUser } from './helpers/session'

test.describe('Navigation', () => {
    test('z login można przejść do register', async ({ page }) => {
        await page.goto('/login')

        await page.getByRole('link', { name: 'Utwórz konto', exact: true }).click()

        await expect(page).toHaveURL(/\/register/)
        await expect(page.getByRole('heading', { name: /Dołącz do GWINTA/i })).toBeVisible()
    })

    test('z register można przejść do login', async ({ page }) => {
        await page.goto('/register')

        await page.getByRole('link', { name: 'Mam już konto', exact: true }).click()

        await expect(page).toHaveURL(/\/login/)
        await expect(page.getByRole('heading', { name: /Zaloguj się do GWINTA/i })).toBeVisible()
    })

    test('z login można wrócić do strony głównej', async ({ page }) => {
        await page.goto('/login')

        await page.getByRole('link', { name: 'Wróć do strony głównej', exact: true }).click()

        await expect(page).toHaveURL('/')
        await expect(page.getByRole('heading', { name: 'GWINT', exact: true })).toBeVisible()
    })

    test('z register można wrócić do strony głównej', async ({ page }) => {
        await page.goto('/register')

        await page.getByRole('link', { name: 'Wróć do strony głównej', exact: true }).click()

        await expect(page).toHaveURL('/')
        await expect(page.getByRole('heading', { name: 'GWINT', exact: true })).toBeVisible()
    })

    test('kliknięcie brandu GWINT w headerze wraca na home', async ({ page }) => {
        await page.goto('/login')

        await page.locator('.header-brand').click()

        await expect(page).toHaveURL('/')
        await expect(page.getByRole('heading', { name: 'GWINT', exact: true })).toBeVisible()
    })

    test('zalogowany użytkownik nie powinien wejść na /register', async ({ page }) => {
        await seedLoggedUser(page, 'lambert')

        await page.goto('/register')

        await expect(page).toHaveURL('/')
        await expect(page.getByText('lambert')).toBeVisible()
    })
})
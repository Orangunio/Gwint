import { test, expect } from '@playwright/test'
import { seedLoggedUser } from './helpers/session'

test.describe('Session', () => {
    test('sesja utrzymuje się po reloadzie strony', async ({ page }) => {
        await seedLoggedUser(page, 'geralt')

        await expect(page.getByText('geralt')).toBeVisible()

        await page.reload()

        await expect(page.getByText('geralt')).toBeVisible()
        await expect(page.locator('header').getByText('Poziom 1')).toBeVisible()
    })

    test('po reloadzie zalogowany użytkownik nadal widzi PlayerStats', async ({ page }) => {
        await seedLoggedUser(page, 'yennefer')

        await page.reload()

        await expect(page.getByText('yennefer')).toBeVisible()
        await expect(page.getByText('Win Rate')).toBeVisible()
        await expect(page.getByText('Wygrane', { exact: true })).toBeVisible()
    })

    test('logout czyści localStorage', async ({ page }) => {
        await seedLoggedUser(page, 'vesemir')

        const accountButton = page.locator('button').filter({
            has: page.locator('.mdi-account-circle'),
        }).first()

        await accountButton.click()
        await page.getByText('Wyloguj', { exact: true }).click()

        const storage = await page.evaluate(() => ({
            token: localStorage.getItem('gwint_token'),
            login: localStorage.getItem('gwint_login'),
        }))

        expect(storage.token).toBeNull()
        expect(storage.login).toBeNull()
    })

    test('po odtworzeniu sesji z localStorage zachowuje playerId', async ({ page }) => {
        await page.goto('/')

        await page.evaluate(() => {
            localStorage.setItem('gwint_token', 'seed-token')
            localStorage.setItem('gwint_login', 'geralt')
            localStorage.setItem('gwint_player_id', '321')
        })

        await page.reload()

        const storage = await page.evaluate(() => ({
            token: localStorage.getItem('gwint_token'),
            login: localStorage.getItem('gwint_login'),
            playerId: localStorage.getItem('gwint_player_id'),
        }))

        expect(storage.token).toBe('seed-token')
        expect(storage.login).toBe('geralt')
        expect(storage.playerId).toBe('321')

        await expect(page.getByText('geralt')).toBeVisible()
    })
})
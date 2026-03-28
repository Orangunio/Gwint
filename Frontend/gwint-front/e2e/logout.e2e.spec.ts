import { test, expect } from '@playwright/test'

test.describe('Logout', () => {
    test('użytkownik może się wylogować z menu konta', async ({ page }) => {
        await page.goto('/')

        await page.evaluate(() => {
            localStorage.setItem('gwint_token', 'seed-token')
            localStorage.setItem('gwint_login', 'vesemir')
        })

        await page.reload()

        await expect(page.getByText('vesemir')).toBeVisible()
        await expect(page.locator('header').getByText('Poziom 1')).toBeVisible()

        const accountButton = page.locator('button').filter({
            has: page.locator('.mdi-account-circle'),
        }).first()

        await accountButton.click()
        await page.getByText('Wyloguj').click()

        await expect(page).toHaveURL('/')
        await expect(page.getByRole('button', { name: 'Zaloguj się' })).toBeVisible()
        await expect(page.getByText('Zostałeś wylogowany.')).toBeVisible()

        const token = await page.evaluate(() => localStorage.getItem('gwint_token'))
        expect(token).toBeNull()

        const storage = await page.evaluate(() => ({
            token: localStorage.getItem('gwint_token'),
            login: localStorage.getItem('gwint_login'),
            playerId: localStorage.getItem('gwint_player_id'),
        }))

        expect(storage.token).toBeNull()
        expect(storage.login).toBeNull()
        expect(storage.playerId).toBeNull()
    })
})
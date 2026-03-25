import { test, expect } from '@playwright/test'

test.describe('Protected route guards', () => {
    test('gość nie może wejść na /lobby', async ({ page }) => {
        await page.goto('/lobby')

        await expect(page).toHaveURL(/\/login/)
        await expect(page.getByRole('heading', { name: /Zaloguj się do GWINTA/i })).toBeVisible()
    })

    test('gość nie może wejść na /game/:roomId/fraction', async ({ page }) => {
        await page.goto('/game/ABC123/fraction')

        await expect(page).toHaveURL(/\/login/)
        await expect(page.getByRole('heading', { name: /Zaloguj się do GWINTA/i })).toBeVisible()
    })

    test('gość nie może wejść na /game/:roomId', async ({ page }) => {
        await page.goto('/game/ABC123')

        await expect(page).toHaveURL(/\/login/)
        await expect(page.getByRole('heading', { name: /Zaloguj się do GWINTA/i })).toBeVisible()
    })
})
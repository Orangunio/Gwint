import { test, expect } from '@playwright/test'

test.describe('Home routing changes', () => {
    test('gość po kliknięciu "Zagraj Teraz" trafia finalnie na login przez guard', async ({ page }) => {
        await page.goto('/')

        await page.getByRole('button', { name: 'Zagraj Teraz', exact: true }).click()

        await expect(page).toHaveURL(/\/login/)
        await expect(page.getByRole('heading', { name: /Zaloguj się do GWINTA/i })).toBeVisible()
    })
})
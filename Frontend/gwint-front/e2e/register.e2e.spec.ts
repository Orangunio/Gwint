import { test, expect } from '@playwright/test'
import { mockRegisterSuccess, mockRegisterError } from './helpers/auth'

test.describe('Register', () => {
    test('pokazuje błąd gdy hasła nie są takie same', async ({ page }) => {
        await page.goto('/register')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('ciri')
        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('haslo123')
        await form.getByRole('textbox', { name: 'Powtórz hasło', exact: true }).fill('innehaslo')

        await form.getByRole('button', { name: 'Utwórz konto', exact: true }).click()

        await expect(page.getByText('Hasła nie są takie same.')).toBeVisible()
        await expect(page).toHaveURL(/\/register/)
    })

    test('pokazuje błąd gdy hasło jest za krótkie', async ({ page }) => {
        await page.goto('/register')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('ciri')
        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('123')
        await form.getByRole('textbox', { name: 'Powtórz hasło', exact: true }).fill('123')

        await form.getByRole('button', { name: 'Utwórz konto', exact: true }).click()

        await expect(page.getByText('Hasło powinno mieć minimum 6 znaków.')).toBeVisible()
    })

    test('użytkownik może założyć konto i zostaje przekierowany do logowania', async ({ page }) => {
        await mockRegisterSuccess(page)

        await page.goto('/register')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('yennefer')
        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('sekret123')
        await form.getByRole('textbox', { name: 'Powtórz hasło', exact: true }).fill('sekret123')

        await form.getByRole('button', { name: 'Utwórz konto', exact: true }).click()

        await expect(page.getByText('Konto zostało utworzone. Za chwilę przejdziesz do logowania.')).toBeVisible()
        await expect(page).toHaveURL(/\/login/, { timeout: 3000 })
    })

    test('pokazuje błąd z API przy rejestracji', async ({ page }) => {
        await mockRegisterError(page, 'Taki użytkownik już istnieje.')

        await page.goto('/register')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('yennefer')
        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('sekret123')
        await form.getByRole('textbox', { name: 'Powtórz hasło', exact: true }).fill('sekret123')

        await form.getByRole('button', { name: 'Utwórz konto', exact: true }).click()

        await expect(page.getByText('Taki użytkownik już istnieje.')).toBeVisible()
        await expect(page).toHaveURL(/\/register/)
    })

    test('pokazuje błąd gdy login jest pusty', async ({ page }) => {
        await page.goto('/register')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Hasło', exact: true }).fill('sekret123')
        await form.getByRole('textbox', { name: 'Powtórz hasło', exact: true }).fill('sekret123')

        await form.getByRole('button', { name: 'Utwórz konto', exact: true }).click()

        await expect(page.getByText('Login jest wymagany.')).toBeVisible()
    })

    test('pokazuje błąd gdy hasło jest puste', async ({ page }) => {
        await page.goto('/register')

        const form = page.locator('form')

        await form.getByRole('textbox', { name: 'Login', exact: true }).fill('ciri')
        await form.getByRole('button', { name: 'Utwórz konto', exact: true }).click()

        await expect(page.getByText('Hasło jest wymagane.')).toBeVisible()
    })
})
import type { Page } from '@playwright/test'

export async function seedLoggedUser(page: Page, login = 'geralt', token = 'seed-token') {
    await page.goto('/')

    await page.evaluate(
        ({ login, token }) => {
            localStorage.setItem('gwint_token', token)
            localStorage.setItem('gwint_login', login)
        },
        { login, token }
    )

    await page.reload()
}
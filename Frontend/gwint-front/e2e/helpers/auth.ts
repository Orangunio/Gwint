import type { Page } from '@playwright/test'

export async function mockLoginSuccess(page: Page, token = 'fake-jwt-token') {
    await page.route('**/api/player/login', async route => {
        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
                token,
            }),
        })
    })
}

export async function mockLoginError(page: Page, message = 'Nieprawidłowy login lub hasło.') {
    await page.route('**/api/player/login', async route => {
        await route.fulfill({
            status: 401,
            contentType: 'application/json',
            body: JSON.stringify({
                message,
            }),
        })
    })
}

export async function mockRegisterSuccess(page: Page) {
    await page.route('**/api/player/register', async route => {
        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
                success: true,
            }),
        })
    })
}

export async function mockRegisterError(page: Page, message = 'Taki użytkownik już istnieje.') {
    await page.route('**/api/player/register', async route => {
        await route.fulfill({
            status: 400,
            contentType: 'application/json',
            body: JSON.stringify({
                message,
            }),
        })
    })
}
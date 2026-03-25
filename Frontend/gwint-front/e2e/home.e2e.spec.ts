import { test, expect } from '@playwright/test'
import { seedLoggedUser } from './helpers/session'

test.describe('Home', () => {
    test('gość widzi hero i główne CTA', async ({ page }) => {
        await page.goto('/')

        await expect(page.getByRole('heading', { name: 'GWINT', exact: true })).toBeVisible()
        await expect(page.getByText(/Strategiczna gra karciana/i)).toBeVisible()
        await expect(page.getByRole('button', { name: 'Zagraj Teraz', exact: true })).toBeVisible()
        await expect(page.getByRole('button', { name: 'Jak Grać?', exact: true })).toBeVisible()
    })

    test('kliknięcie "Jak Grać?" pokazuje sekcję menu', async ({ page }) => {
        await page.goto('/')

        await page.getByRole('button', { name: 'Jak Grać?', exact: true }).click()

        await expect(page.getByRole('heading', { name: 'Co chcesz zrobić?', exact: true })).toBeVisible()
        await expect(page.getByText('Gra Online', { exact: true })).toBeVisible()
        await expect(page.getByText('Gra z Botem', { exact: true })).toBeVisible()
        await expect(page.getByText('Kolekcja Kart', { exact: true })).toBeVisible()
        await expect(page.getByText('Ranking', { exact: true })).toBeVisible()
    })

    test('gość po kliknięciu "Zagraj Teraz" trafia na login', async ({ page }) => {
        await page.goto('/')

        await page.getByRole('button', { name: 'Zagraj Teraz', exact: true }).click()

        await expect(page).toHaveURL(/\/login/)
        await expect(page.getByRole('heading', { name: /Zaloguj się do GWINTA/i })).toBeVisible()
    })

    test('zalogowany użytkownik po kliknięciu "Zagraj Teraz" trafia do lobby', async ({ page }) => {
        await seedLoggedUser(page, 'geralt')

        await page.getByRole('button', { name: 'Zagraj Teraz', exact: true }).click()

        await expect(page).toHaveURL(/\/lobby/)
        await expect(page.getByText('Stwórz pokój', { exact: true })).toBeVisible()
        await expect(page.getByText('Dołącz do pokoju', { exact: true })).toBeVisible()
    })

    test('gość widzi badge "Wymaga logowania" przy Gra Online', async ({ page }) => {
        await page.goto('/')

        await expect(page.getByText('Gra Online', { exact: true })).toBeVisible()
        await expect(page.getByText('Wymaga logowania', { exact: true })).toBeVisible()
    })

    test('zalogowany użytkownik widzi badge "Dostępne" przy Gra Online', async ({ page }) => {
        await seedLoggedUser(page, 'geralt')

        await expect(page.getByText('Gra Online', { exact: true })).toBeVisible()
        await expect(page.getByText('Dostępne', { exact: true })).toBeVisible()
    })

    test('gość nie widzi PlayerStats', async ({ page }) => {
        await page.goto('/')

        await expect(page.getByText('Win Rate')).toHaveCount(0)
        await expect(page.getByText('Wygrane', { exact: true })).toHaveCount(0)
        await expect(page.getByText('Przegrane', { exact: true })).toHaveCount(0)
    })

    test('zalogowany użytkownik widzi PlayerStats', async ({ page }) => {
        await seedLoggedUser(page, 'vesemir')

        await expect(page.getByText('vesemir')).toBeVisible()
        await expect(page.getByText('Win Rate')).toBeVisible()
        await expect(page.getByText('Wygrane', { exact: true })).toBeVisible()
        await expect(page.getByText('Przegrane', { exact: true })).toBeVisible()
        await expect(page.getByText('Razem', { exact: true })).toBeVisible()
        await expect(page.getByText('0%')).toBeVisible()
    })
})
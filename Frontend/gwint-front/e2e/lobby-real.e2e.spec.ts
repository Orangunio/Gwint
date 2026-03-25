import { test, expect } from '@playwright/test'
import { createAuthedPage } from './helpers/real-auth'

test.describe('Lobby real backend', () => {
    test.setTimeout(60_000)

    test('zalogowany użytkownik po kliknięciu "Zagraj Teraz" trafia do lobby', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'hero')

        await page.goto('/')
        await page.getByRole('button', { name: 'Zagraj Teraz', exact: true }).click()

        await expect(page).toHaveURL(/\/lobby/)
        await expect(page.getByText('Stwórz pokój', { exact: true })).toBeVisible()
        await expect(page.getByText('Dołącz do pokoju', { exact: true })).toBeVisible()

        await context.close()
    })

    test('zalogowany użytkownik widzi w lobby opcje stworzenia i dołączenia do pokoju', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'lobbymenu')

        await page.goto('/lobby')

        await expect(page.getByTestId('create-room-card')).toBeVisible()
        await expect(page.getByTestId('join-room-card')).toBeVisible()

        await context.close()
    })

    test('stworzenie pokoju przełącza lobby w fazę waiting', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'createwait')

        await page.goto('/lobby')
        await page.getByTestId('create-room-card').click()

        await expect(page.getByTestId('room-code')).toBeVisible()
        await expect(page.getByTestId('leave-room-button')).toBeVisible()

        await context.close()
    })

    test('po stworzeniu pokoju wyświetla się 6-znakowy kod pokoju', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'roomcode')

        await page.goto('/lobby')
        await page.getByTestId('create-room-card').click()

        const roomCode = await page.getByTestId('room-code').textContent()
        expect(roomCode).toMatch(/^[A-Z0-9]{6}$/)

        await context.close()
    })

    test('po stworzeniu pokoju w waiting widać login hosta', async ({ browser, request }) => {
        const { context, page, session } = await createAuthedPage(browser, request, 'hostlogin')

        await page.goto('/lobby')
        await page.getByTestId('create-room-card').click()

        await expect(page.getByText(session.login)).toBeVisible()

        await context.close()
    })

    test('opuszczenie pokoju wraca do ekranu menu lobby', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'leave')

        await page.goto('/lobby')
        await page.getByTestId('create-room-card').click()
        await page.getByTestId('leave-room-button').click()

        await expect(page.getByTestId('create-room-card')).toBeVisible()
        await expect(page.getByTestId('join-room-card')).toBeVisible()

        await context.close()
    })

    test('kliknięcie "Dołącz do pokoju" otwiera formularz join', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'joinopen')

        await page.goto('/lobby')
        await page.getByTestId('join-room-card').click()

        await expect(page.getByTestId('join-room-form')).toBeVisible()
        await expect(page.getByLabel('Kod pokoju (6 znaków)')).toBeVisible()

        await context.close()
    })

    test('kliknięcie "Anuluj" zamyka formularz join', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'joincancel')

        await page.goto('/lobby')
        await page.getByTestId('join-room-card').click()
        await expect(page.getByTestId('join-room-form')).toBeVisible()

        await page.getByTestId('join-cancel-button').click()
        await expect(page.getByTestId('join-room-form')).toHaveCount(0)

        await context.close()
    })

    test('przycisk Dołącz jest zablokowany gdy kod ma mniej niż 6 znaków', async ({ browser, request }) => {
        const { context, page } = await createAuthedPage(browser, request, 'joinshort')

        await page.goto('/lobby')
        await page.getByTestId('join-room-card').click()

        await page.getByLabel('Kod pokoju (6 znaków)').fill('abc')
        await expect(page.getByTestId('join-submit-button')).toBeDisabled()

        await context.close()
    })

    test('drugi gracz może dołączyć wpisując kod małymi literami', async ({ browser, request }) => {
        const host = await createAuthedPage(browser, request, 'hostlower')
        const guest = await createAuthedPage(browser, request, 'guestlower')

        await host.page.goto('/lobby')
        await guest.page.goto('/lobby')

        await host.page.getByTestId('create-room-card').click()
        const roomCode = (await host.page.getByTestId('room-code').textContent())!

        await guest.page.getByTestId('join-room-card').click()
        await guest.page.getByLabel('Kod pokoju (6 znaków)').fill(roomCode.toLowerCase())
        await guest.page.getByTestId('join-submit-button').click()

        await expect(guest.page.getByTestId('room-code')).toBeVisible()

        await host.context.close()
        await guest.context.close()
    })

    test('host widzi że przeciwnik dołączył i może rozpocząć grę', async ({ browser, request }) => {
        const host = await createAuthedPage(browser, request, 'hostready')
        const guest = await createAuthedPage(browser, request, 'guestready')

        await host.page.goto('/lobby')
        await guest.page.goto('/lobby')

        await host.page.getByTestId('create-room-card').click()
        const roomCode = (await host.page.getByTestId('room-code').textContent())!

        await guest.page.getByTestId('join-room-card').click()
        await guest.page.getByLabel('Kod pokoju (6 znaków)').fill(roomCode)
        await guest.page.getByTestId('join-submit-button').click()

        await expect(host.page.getByText('Przeciwnik dołączył!')).toBeVisible({ timeout: 10_000 })
        await expect(host.page.getByTestId('start-room-game-button')).toBeVisible({ timeout: 10_000 })

        await host.context.close()
        await guest.context.close()
    })

    test('host może rozpocząć grę i obaj gracze trafiają na wybór frakcji', async ({ browser, request }) => {
        const host = await createAuthedPage(browser, request, 'hoststart')
        const guest = await createAuthedPage(browser, request, 'gueststart')

        await host.page.goto('/lobby')
        await guest.page.goto('/lobby')

        await host.page.getByTestId('create-room-card').click()
        const roomCode = (await host.page.getByTestId('room-code').textContent())!

        await guest.page.getByTestId('join-room-card').click()
        await guest.page.getByLabel('Kod pokoju (6 znaków)').fill(roomCode)
        await guest.page.getByTestId('join-submit-button').click()

        await expect(host.page.getByTestId('start-room-game-button')).toBeVisible({ timeout: 10_000 })
        await host.page.getByTestId('start-room-game-button').click()

        await expect(host.page).toHaveURL(new RegExp(`/game/${roomCode}/fraction`), { timeout: 15_000 })
        await expect(guest.page).toHaveURL(new RegExp(`/game/${roomCode}/fraction`), { timeout: 15_000 })

        await host.context.close()
        await guest.context.close()
    })
})
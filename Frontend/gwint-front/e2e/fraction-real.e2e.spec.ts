import { test, expect } from '@playwright/test'
import { createAuthedPage } from './helpers/real-auth'

async function setupPlayersOnFractionPage(browser: any, request: any) {
    const host = await createAuthedPage(browser, request, 'fractionhost')
    const guest = await createAuthedPage(browser, request, 'fractionguest')

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

    return { host, guest, roomCode }
}

test.describe('Fraction select real backend', () => {
    test.setTimeout(60_000)

    test('na stronie wyboru frakcji host widzi roomId', async ({ browser, request }) => {
        const { host, guest, roomCode } = await setupPlayersOnFractionPage(browser, request)

        await expect(host.page.getByTestId('fraction-room-id')).toHaveText(roomCode)

        await host.context.close()
        await guest.context.close()
    })

    test('na stronie wyboru frakcji guest widzi roomId', async ({ browser, request }) => {
        const { host, guest, roomCode } = await setupPlayersOnFractionPage(browser, request)

        await expect(guest.page.getByTestId('fraction-room-id')).toHaveText(roomCode)

        await host.context.close()
        await guest.context.close()
    })

    test('przycisk potwierdzenia jest zablokowany przed wyborem frakcji', async ({ browser, request }) => {
        const { host, guest } = await setupPlayersOnFractionPage(browser, request)

        await expect(host.page.getByTestId('confirm-fraction-button')).toBeDisabled()

        await host.context.close()
        await guest.context.close()
    })

    test('wybór frakcji zaznacza kartę frakcji', async ({ browser, request }) => {
        const { host, guest } = await setupPlayersOnFractionPage(browser, request)

        await host.page.getByTestId('fraction-card-1').click()
        await expect(host.page.getByTestId('fraction-card-1').getByText('Wybrana')).toBeVisible()

        await host.context.close()
        await guest.context.close()
    })

    test('wybór frakcji odblokowuje przycisk potwierdzenia', async ({ browser, request }) => {
        const { host, guest } = await setupPlayersOnFractionPage(browser, request)

        await host.page.getByTestId('fraction-card-2').click()
        await expect(host.page.getByTestId('confirm-fraction-button')).toBeEnabled()

        await host.context.close()
        await guest.context.close()
    })

    test('po potwierdzeniu wyboru host widzi oczekiwanie na przeciwnika', async ({ browser, request }) => {
        const { host, guest } = await setupPlayersOnFractionPage(browser, request)

        await host.page.getByTestId('fraction-card-3').click()
        await host.page.getByTestId('confirm-fraction-button').click()

        await expect(host.page.getByTestId('fraction-waiting-status')).toBeVisible()

        await host.context.close()
        await guest.context.close()
    })
})
import { test, expect } from '@playwright/test'
import type { Page, Browser } from '@playwright/test'

const BASE_URL = 'http://localhost:5173'

interface TestUser {
  login: string
  password: string
}

const TEST_USER_1: TestUser = { login: 'TestPlayer1', password: 'Test1234!' }
const TEST_USER_2: TestUser = { login: 'TestPlayer2', password: 'Test1234!' }
const TEST_USER_3: TestUser = { login: 'TestPlayer3', password: 'Test1234!' }

async function loginAs(page: Page, user: TestUser): Promise<void> {
  await page.goto(`${BASE_URL}/login`);

  await page.getByLabel('Login').fill(user.login);
  await page.locator('input[type="password"]').fill(user.password);

  // kliknij logowanie
  await page.locator('form button[type="submit"]').click();

  // czekaj na token JWT w localStorage
  await page.waitForFunction(() => !!localStorage.getItem('token'));

  // teraz powinien być redirect na stronę główną
  await page.waitForURL(`${BASE_URL}/`);
}

function getRoomId(url: string): string {
  const id = url.split('/room/')[1]
  if (!id) throw new Error(`Nie można pobrać roomId z URL: ${url}`)
  return id
}

async function createRoom(page: Page): Promise<string> {
  await page.goto(`${BASE_URL}/create-room`)

  if (page.url().includes('/login')) {
    throw new Error('Nie jesteś zalogowany w createRoom()')
  }

  await page.getByRole('button', { name: 'Utwórz pokój' }).click()

  await page.waitForURL(/\/room\//)
  await page.locator('text=Graczy w pokoju').waitFor()

  return getRoomId(page.url())
}

async function joinRoom(page: Page, roomId: string): Promise<void> {
  await page.goto(`${BASE_URL}/join`)
  await page.getByPlaceholder('Kod pokoju').fill(roomId)
  await page.getByRole('button', { name: 'Dołącz' }).click()

  await page.waitForURL(/\/room\//)
  await page.locator('text=Graczy w pokoju').waitFor()
}

test('niezalogowany użytkownik widzi redirect na login', async ({ page }) => {
  await page.goto(`${BASE_URL}/create-room`)
  await expect(page).toHaveURL(`${BASE_URL}/login`)
})

test('zalogowany użytkownik tworzy pokój', async ({ page }) => {
  await loginAs(page, TEST_USER_1)
  const roomId = await createRoom(page)

  expect(roomId).toBeTruthy()
  await expect(page.locator('h2')).toContainText('Pokój:')
})

test('gospodarz widzi siebie w pokoju po utworzeniu', async ({ page }) => {
  await loginAs(page, TEST_USER_1)
  await createRoom(page)

  await expect(page.locator('table')).toContainText(TEST_USER_1.login)
  await expect(page.locator('text=Graczy w pokoju')).toContainText('1 / 2')
})

test('przycisk start jest zablokowany gdy tylko 1 gracz', async ({ page }) => {
  await loginAs(page, TEST_USER_1)
  await createRoom(page)

  const startButton = page.getByRole('button', { name: /Czekaj na drugiego gracza/i })
  await expect(startButton).toBeDisabled()
})

test('dołączenie do nieistniejącego pokoju pokazuje błąd', async ({ page }) => {
  await loginAs(page, TEST_USER_1)

  await page.goto(`${BASE_URL}/join`)
  await page.getByPlaceholder('Kod pokoju').fill('XXXXXX')
  await page.getByRole('button', { name: 'Dołącz' }).click()

  await page.waitForURL(/\/room\//)
  await expect(page.locator('text=nie istnieje')).toBeVisible()
})

test('dwóch graczy widzi się nawzajem w pokoju', async ({ browser }) => {
  const context1 = await browser.newContext()
  const context2 = await browser.newContext()
  const page1 = await context1.newPage()
  const page2 = await context2.newPage()

  await loginAs(page1, TEST_USER_1)
  const roomId = await createRoom(page1)

  await loginAs(page2, TEST_USER_2)
  await joinRoom(page2, roomId)

  await expect(page1.locator('text=Graczy w pokoju')).toContainText('2 / 2')
  await expect(page2.locator('text=Graczy w pokoju')).toContainText('2 / 2')

  await expect(page1.locator('table')).toContainText(TEST_USER_2.login)
  await expect(page2.locator('table')).toContainText(TEST_USER_1.login)

  await context1.close()
  await context2.close()
})

test('trzeci gracz widzi błąd pełnego pokoju', async ({ browser }) => {
  const context1 = await browser.newContext()
  const context2 = await browser.newContext()
  const context3 = await browser.newContext()
  const page1 = await context1.newPage()
  const page2 = await context2.newPage()
  const page3 = await context3.newPage()

  await loginAs(page1, TEST_USER_1)
  const roomId = await createRoom(page1)

  await loginAs(page2, TEST_USER_2)
  await joinRoom(page2, roomId)

  await loginAs(page3, TEST_USER_3)
  await page3.goto(`${BASE_URL}/join`)
  await page3.getByPlaceholder('Kod pokoju').fill(roomId)
  await page3.getByRole('button', { name: 'Dołącz' }).click()

  await page3.waitForURL(/\/room\//)
  await expect(page3.locator('text=jest pełny')).toBeVisible()

  await context1.close()
  await context2.close()
  await context3.close()
})

test('gracz może wyjść z pokoju i wraca na stronę główną', async ({ page }) => {
  await loginAs(page, TEST_USER_1)
  await createRoom(page)

  await page.getByRole('button', { name: 'Wyjdź z pokoju' }).click()
  await expect(page).toHaveURL(`${BASE_URL}/`)
})

test('gospodarz widzi że gracz wyszedł z pokoju', async ({ browser }) => {
  const context1 = await browser.newContext()
  const context2 = await browser.newContext()
  const page1 = await context1.newPage()
  const page2 = await context2.newPage()

  await loginAs(page1, TEST_USER_1)
  const roomId = await createRoom(page1)

  await loginAs(page2, TEST_USER_2)
  await joinRoom(page2, roomId)

  await expect(page1.locator('text=Graczy w pokoju')).toContainText('2 / 2')

  await page2.getByRole('button', { name: 'Wyjdź z pokoju' }).click()

  await expect(page1.locator('text=Graczy w pokoju')).toContainText('1 / 2')
  await expect(page1.locator('table')).not.toContainText(TEST_USER_2.login)

  await context1.close()
  await context2.close()
})

test('gospodarz startuje grę i obaj gracze widzą informację o starcie', async ({ browser }) => {
  const context1 = await browser.newContext()
  const context2 = await browser.newContext()
  const page1 = await context1.newPage()
  const page2 = await context2.newPage()

  await loginAs(page1, TEST_USER_1)
  const roomId = await createRoom(page1)

  await loginAs(page2, TEST_USER_2)
  await joinRoom(page2, roomId)

  await page1.getByRole('button', { name: 'Start gry!' }).click()

  await expect(page1.locator('text=Gra się zaczęła')).toBeVisible()
  await expect(page2.locator('text=Gra się zaczęła')).toBeVisible()

  await context1.close()
  await context2.close()
})
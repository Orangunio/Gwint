import { expect, type APIRequestContext, type Browser, type BrowserContext, type Page } from '@playwright/test'

const API_BASE = 'http://localhost:5006/api'

export type RealCreds = {
    login: string
    password: string
}

export type RealSession = {
    login: string
    password: string
    token: string
    playerId: string
}

function randomSuffix() {
    return `${Date.now()}_${Math.floor(Math.random() * 100000)}`
}

export function makeUniqueCreds(prefix: string): RealCreds {
    return {
        login: `${prefix}_${randomSuffix()}`,
        password: 'Secret123!',
    }
}

export async function registerAndLogin(
    request: APIRequestContext,
    creds: RealCreds,
): Promise<RealSession> {
    const registerResponse = await request.post(`${API_BASE}/player/register`, {
        data: {
            login: creds.login,
            password: creds.password,
        },
    })

    expect(registerResponse.ok()).toBeTruthy()

    const loginResponse = await request.post(`${API_BASE}/player/login`, {
        data: {
            login: creds.login,
            password: creds.password,
        },
    })

    expect(loginResponse.ok()).toBeTruthy()

    const loginJson = (await loginResponse.json()) as { token: string }

    const meResponse = await request.get(`${API_BASE}/player/me`, {
        headers: {
            Authorization: `Bearer ${loginJson.token}`,
        },
    })

    expect(meResponse.ok()).toBeTruthy()

    const meJson = (await meResponse.json()) as { id: number; login: string }

    return {
        login: creds.login,
        password: creds.password,
        token: loginJson.token,
        playerId: String(meJson.id),
    }
}

export async function createAuthedPage(
    browser: Browser,
    request: APIRequestContext,
    prefix: string,
): Promise<{
    context: BrowserContext
    page: Page
    session: RealSession
}> {
    const creds = makeUniqueCreds(prefix)
    const session = await registerAndLogin(request, creds)

    const context = await browser.newContext()
    await context.addInitScript(
        ({ token, login, playerId }) => {
            localStorage.setItem('gwint_token', token)
            localStorage.setItem('gwint_login', login)
            localStorage.setItem('gwint_player_id', playerId)
        },
        {
            token: session.token,
            login: session.login,
            playerId: session.playerId,
        },
    )

    const page = await context.newPage()

    return { context, page, session }
}
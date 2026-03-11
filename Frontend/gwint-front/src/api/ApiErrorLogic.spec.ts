import { describe, it, expect, vi, beforeEach } from 'vitest'
import { http, ApiError } from './http'

describe('ApiError & Network Edge Cases', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
    
    vi.spyOn(console, 'error').mockImplementation((msg) => {
      if (msg.includes('Could not parse CSS stylesheet')) return
    })
  })

  it('ApiError posiada poprawną nazwę klasy', () => {
    const error = new ApiError('Test Message', 400)
    expect(error.name).toBe('ApiError')
  })

  it('ApiError poprawnie przypisuje status HTTP', () => {
    const error = new ApiError('Forbidden', 403)
    expect(error.status).toBe(403)
  })

  it('http rzuca błąd ApiError przy statusie 500', async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: false,
      status: 500,
      headers: new Headers(),
      text: async () => 'Internal Server Error'
    } as any)

    await expect(http('/error')).rejects.toThrow(ApiError)
  })

  it('http rzuca błąd z komunikatem z serwera (string)', async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: false,
      status: 400,
      headers: new Headers(),
      text: async () => 'Zły format danych'
    } as any)

    try {
      await http('/test')
    } catch (e: any) {
      expect(e.message).toBe('Zły format danych')
    }
  })

  it('http parsuje błąd z obiektu JSON {message: "..."}', async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: false,
      status: 400,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: async () => ({ message: 'Login zajęty' })
    } as any)

    await expect(http('/auth')).rejects.toThrow('Login zajęty')
  })

  it('http zwraca null, gdy odpowiedź jest pusta i nie jest JSONem', async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      headers: new Headers({ 'content-type': 'text/plain' }),
      text: async () => ''
    } as any)

    const result = await http('/empty')
    expect(result).toBeNull()
  })

  it('http dołącza token do nagłówka Authorization', async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      headers: new Headers(),
      text: async () => 'ok'
    } as any)

    await http('/secure', { token: 'super-secret' })
    const callHeaders = vi.mocked(fetch).mock.calls[0][1]?.headers as any
    expect(callHeaders['Authorization']).toBe('Bearer super-secret')
  })

  it('http wysyła metodę POST, gdy tak określono w opcjach', async () => {
    vi.mocked(fetch).mockResolvedValue({ ok: true, headers: new Headers(), text: async () => 'ok' } as any)
    await http('/data', { method: 'POST' })
    expect(vi.mocked(fetch).mock.calls[0][1]?.method).toBe('POST')
  })

  it('http poprawnie serializuje body do JSONa', async () => {
    vi.mocked(fetch).mockResolvedValue({ ok: true, headers: new Headers(), text: async () => 'ok' } as any)
    const payload = { a: 1 }
    await http('/post', { body: payload })
    expect(vi.mocked(fetch).mock.calls[0][1]?.body).toBe(JSON.stringify(payload))
  })

  it('ApiError rzucony ręcznie zachowuje wiadomość', () => {
    const err = new ApiError('Manual Error', 500)
    expect(err.message).toBe('Manual Error')
  })
})
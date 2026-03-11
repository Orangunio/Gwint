import { API_BASE_URL } from './config'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

interface RequestOptions {
  method?: HttpMethod
  body?: unknown
  token?: string | null
}

export class ApiError extends Error {
  public status: number

  constructor (message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function http<T> (endpoint: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, token } = options

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  const contentType = response.headers.get('content-type') || ''
  const isJson = contentType.includes('application/json')

  let data: unknown = null

  if (isJson) {
    data = await response.json()
  } else {
    const text = await response.text()
    data = text || null
  }

  if (!response.ok) {
    const message
      = typeof data === 'string'
        ? data
        : (data as { message?: string } | null)?.message || 'Wystąpił błąd podczas komunikacji z API.'

    throw new ApiError(message, response.status)
  }

  return data as T
}

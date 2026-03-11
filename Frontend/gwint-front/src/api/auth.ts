import type { AuthRequest, LoginResponse } from '@/types/auth'
import { http } from './http'

export function register (payload: AuthRequest) {
  return http('/player/register', {
    method: 'POST',
    body: payload,
  })
}

export function login (payload: AuthRequest) {
  return http<LoginResponse>('/player/login', {
    method: 'POST',
    body: payload,
  })
}

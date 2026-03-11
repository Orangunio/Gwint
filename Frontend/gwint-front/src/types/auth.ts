export interface AuthRequest {
  login: string
  password: string
}

export interface LoginResponse {
  token: string
}

export interface ApiErrorResponse {
  message: string
}

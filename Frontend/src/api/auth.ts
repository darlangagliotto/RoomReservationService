/*
  Contrato do AuthService. Ver docs/services/auth-service.md.
*/

import type { ApiClient } from '@/api/client'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  /** Emitido pelo backend; a expiracao efetiva vem do claim `exp` do token. */
  expiresAt: string
}

/**
 * Credencial invalida, usuario bloqueado, usuario inexistente e UserService
 * fora do ar produzem o mesmo BusinessError ("E-mail ou senha inválidos.") —
 * o backend nao os distingue, e a UI nao deve inventar essa distincao.
 */
export function login(client: ApiClient, credentials: LoginRequest): Promise<LoginResponse> {
  return client.request<LoginResponse>('/api/auth/login', {
    method: 'POST',
    body: credentials,
  })
}

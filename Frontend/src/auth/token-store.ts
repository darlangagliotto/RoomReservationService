/*
  Guarda do token. Unico modulo do app que toca sessionStorage.

  Memoria e a fonte de verdade; sessionStorage existe so para o token sobreviver
  ao F5 da aba. Nunca localStorage: o token vale 60 minutos, nao ha refresh nem
  revogacao, entao a sessao nao deve sobreviver ao fechamento da aba
  (ver docs/specs/001-login-e-sessao.md#5-regras-de-negocio).
*/

import { isExpired } from '@/lib/jwt'

export const TOKEN_STORAGE_KEY = 'rrs.auth.token'

let inMemoryToken: string | null = null

/** sessionStorage pode lancar (modo privado, storage desabilitado). Nunca quebra o app. */
function safeStorage(): Storage | null {
  try {
    return globalThis.sessionStorage ?? null
  } catch {
    return null
  }
}

export function getToken(): string | null {
  return inMemoryToken
}

export function setToken(token: string): void {
  inMemoryToken = token
  try {
    safeStorage()?.setItem(TOKEN_STORAGE_KEY, token)
  } catch {
    // Sessao segue valida so em memoria; perde-se no F5.
  }
}

export function clearToken(): void {
  inMemoryToken = null
  try {
    safeStorage()?.removeItem(TOKEN_STORAGE_KEY)
  } catch {
    // Nada a fazer: a memoria ja foi limpa.
  }
}

/**
 * Restaura a sessao ao iniciar o app.
 *
 * Token expirado e descartado aqui mesmo, sem ida ao servidor: a chamada
 * retornaria 401 e causaria um flash de tela autenticada.
 */
export function restoreToken(now: Date = new Date()): string | null {
  let stored: string | null = null

  try {
    stored = safeStorage()?.getItem(TOKEN_STORAGE_KEY) ?? null
  } catch {
    stored = null
  }

  if (stored === null) {
    return null
  }

  if (isExpired(stored, now)) {
    clearToken()
    return null
  }

  inMemoryToken = stored
  return stored
}

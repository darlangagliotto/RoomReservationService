import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  TOKEN_STORAGE_KEY,
  clearToken,
  getToken,
  restoreToken,
  setToken,
} from '@/auth/token-store'
import { makeToken, makeTokenExpiringIn } from '@/test/make-token'

beforeEach(() => {
  clearToken()
  sessionStorage.clear()
})

describe('setToken / getToken', () => {
  it('guarda em memoria e espelha em sessionStorage', () => {
    const token = makeTokenExpiringIn(60)

    setToken(token)

    expect(getToken()).toBe(token)
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBe(token)
  })

  it('nunca escreve em localStorage', () => {
    // localStorage e injetado como duble: no Node 26 o global nativo exige
    // --localstorage-file e fica indefinido, sombreando o do jsdom.
    const fakeLocalStorage = { setItem: vi.fn(), getItem: vi.fn(), removeItem: vi.fn() }
    vi.stubGlobal('localStorage', fakeLocalStorage)

    setToken(makeTokenExpiringIn(60))

    expect(fakeLocalStorage.setItem).not.toHaveBeenCalled()
  })
})

describe('clearToken', () => {
  it('limpa memoria e storage', () => {
    setToken(makeTokenExpiringIn(60))

    clearToken()

    expect(getToken()).toBeNull()
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })
})

describe('restoreToken', () => {
  it('devolve null quando nao ha nada guardado', () => {
    expect(restoreToken()).toBeNull()
    expect(getToken()).toBeNull()
  })

  it('restaura a sessao quando o token ainda vale', () => {
    const token = makeTokenExpiringIn(30)
    sessionStorage.setItem(TOKEN_STORAGE_KEY, token)

    expect(restoreToken()).toBe(token)
    expect(getToken()).toBe(token)
  })

  it('descarta token expirado e limpa o storage, sem restaurar', () => {
    const token = makeTokenExpiringIn(-1)
    sessionStorage.setItem(TOKEN_STORAGE_KEY, token)

    expect(restoreToken()).toBeNull()
    expect(getToken()).toBeNull()
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })

  it('descarta token malformado', () => {
    sessionStorage.setItem(TOKEN_STORAGE_KEY, 'nao-e-um-token')

    expect(restoreToken()).toBeNull()
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })

  it('descarta token sem exp', () => {
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeToken({ email: 'joao@email.com' }))

    expect(restoreToken()).toBeNull()
  })
})

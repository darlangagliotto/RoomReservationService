import { describe, expect, it } from 'vitest'
import { decodeJwtPayload, getEmail, getExpiresAt, isExpired } from '@/lib/jwt'
import { makeToken } from '@/test/make-token'

describe('decodeJwtPayload', () => {
  it('le as claims do payload', () => {
    const token = makeToken({ sub: 'user-1', email: 'joao@email.com', exp: 1_800_000_000 })

    expect(decodeJwtPayload(token)).toEqual({
      sub: 'user-1',
      email: 'joao@email.com',
      exp: 1_800_000_000,
    })
  })

  it('decodifica base64url com caracteres fora do ASCII', () => {
    const token = makeToken({ email: 'joão+conta@email.com' })

    expect(getEmail(token)).toBe('joão+conta@email.com')
  })

  it.each([
    ['string vazia', ''],
    ['sem os tres segmentos', 'abc.def'],
    ['payload que nao e base64 valido', 'abc.!!!.def'],
    ['payload que nao e JSON', `abc.${btoa('nao sou json')}.def`],
  ])('devolve null e nao lanca quando o token e %s', (_caso, token) => {
    expect(decodeJwtPayload(token)).toBeNull()
  })
})

describe('getEmail', () => {
  it('devolve null quando o token nao tem a claim', () => {
    expect(getEmail(makeToken({ sub: 'user-1' }))).toBeNull()
  })
})

describe('isExpired', () => {
  const agora = new Date('2026-09-02T12:00:00Z')

  it('e falso enquanto exp esta no futuro', () => {
    const token = makeToken({ exp: Math.floor(agora.getTime() / 1000) + 60 })

    expect(isExpired(token, agora)).toBe(false)
    expect(getExpiresAt(token)).toEqual(new Date('2026-09-02T12:01:00Z'))
  })

  it('e verdadeiro quando exp ja passou', () => {
    const token = makeToken({ exp: Math.floor(agora.getTime() / 1000) - 1 })

    expect(isExpired(token, agora)).toBe(true)
  })

  it('trata o instante exato da expiracao como expirado', () => {
    const token = makeToken({ exp: Math.floor(agora.getTime() / 1000) })

    expect(isExpired(token, agora)).toBe(true)
  })

  it('trata token sem exp como expirado', () => {
    expect(isExpired(makeToken({ email: 'joao@email.com' }), agora)).toBe(true)
  })

  it('trata token malformado como expirado', () => {
    expect(isExpired('nao-e-um-token', agora)).toBe(true)
  })
})

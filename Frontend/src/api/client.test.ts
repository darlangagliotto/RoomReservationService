import { describe, expect, it, vi } from 'vitest'
import { createApiClient } from '@/api/client'
import {
  BusinessError,
  NetworkError,
  ServerError,
  UnauthorizedError,
  ValidationError,
} from '@/api/errors'

function jsonResponse(body: unknown, status: number): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

function setup(
  responder: () => Response | Promise<Response>,
  token: string | null = 'token-valido',
) {
  const onUnauthorized = vi.fn()
  const fetchImpl = vi.fn(async () => await responder())
  const client = createApiClient({
    getToken: () => token,
    onUnauthorized,
    fetchImpl: fetchImpl as unknown as typeof fetch,
  })

  return { client, fetchImpl, onUnauthorized }
}

function headersOf(fetchImpl: ReturnType<typeof vi.fn>): Headers {
  const init = fetchImpl.mock.calls[0]?.[1] as RequestInit | undefined
  return new Headers(init?.headers)
}

describe('request — sucesso', () => {
  it('devolve o corpo desserializado', async () => {
    const { client } = setup(() => jsonResponse({ id: 'r-1', name: 'Sala Azul' }, 200))

    await expect(client.request('/api/rooms/r-1')).resolves.toEqual({
      id: 'r-1',
      name: 'Sala Azul',
    })
  })

  it('devolve undefined em 204, sem tentar ler o corpo', async () => {
    const { client } = setup(() => new Response(null, { status: 204 }))

    await expect(client.request('/api/reservations/r-1', { method: 'DELETE' })).resolves.toBeUndefined()
  })

  it('envia o token no header Authorization', async () => {
    const { client, fetchImpl } = setup(() => jsonResponse({}, 200))

    await client.request('/api/rooms')

    expect(headersOf(fetchImpl).get('Authorization')).toBe('Bearer token-valido')
  })

  it('omite Authorization quando nao ha sessao', async () => {
    const { client, fetchImpl } = setup(() => jsonResponse({ token: 'x' }, 200), null)

    await client.request('/api/auth/login', { method: 'POST', body: { email: 'a@b.c' } })

    expect(headersOf(fetchImpl).get('Authorization')).toBeNull()
    expect(headersOf(fetchImpl).get('Content-Type')).toBe('application/json')
  })
})

describe('request — mapa de erros', () => {
  it('400 "Business error" vira BusinessError com o detail do backend', async () => {
    const { client } = setup(() =>
      jsonResponse({ title: 'Business error', detail: 'E-mail ou senha inválidos.' }, 400),
    )

    await expect(client.request('/api/auth/login')).rejects.toThrowError(
      new BusinessError('E-mail ou senha inválidos.'),
    )
  })

  it('400 com errors vira ValidationError por campo', async () => {
    const { client } = setup(() =>
      jsonResponse(
        { title: 'One or more validation errors occurred.', errors: { Email: ['Invalid email.'] } },
        400,
      ),
    )

    const erro = await client.request('/api/users').catch((e: unknown) => e)

    expect(erro).toBeInstanceOf(ValidationError)
    expect((erro as ValidationError).first('Email')).toBe('Invalid email.')
    expect((erro as ValidationError).first('Nome')).toBeUndefined()
  })

  it('401 encerra a sessao e vira UnauthorizedError', async () => {
    const { client, onUnauthorized } = setup(() => new Response(null, { status: 401 }))

    await expect(client.request('/api/rooms')).rejects.toBeInstanceOf(UnauthorizedError)
    expect(onUnauthorized).toHaveBeenCalledOnce()
  })

  it('500 vira ServerError sem vazar o detail do servidor', async () => {
    const { client } = setup(() =>
      jsonResponse({ title: 'Internal server error', detail: 'NpgsqlException: boom' }, 500),
    )

    const erro = (await client.request('/api/rooms').catch((e: unknown) => e)) as ServerError

    expect(erro).toBeInstanceOf(ServerError)
    expect(erro.message).not.toContain('Npgsql')
  })

  it('falha de rede vira NetworkError', async () => {
    const { client } = setup(() => Promise.reject(new TypeError('Failed to fetch')))

    await expect(client.request('/api/rooms')).rejects.toBeInstanceOf(NetworkError)
  })

  it('400 sem corpo JSON ainda vira BusinessError com mensagem utilizavel', async () => {
    const { client } = setup(() => new Response('nao json', { status: 400 }))

    const erro = (await client.request('/api/rooms').catch((e: unknown) => e)) as BusinessError

    expect(erro).toBeInstanceOf(BusinessError)
    expect(erro.message.length).toBeGreaterThan(0)
  })
})

describe('requestList — 400 de lista vazia', () => {
  it.each(['Nenhuma sala encontrada.', 'Nenhuma reserva encontrada.', 'nenhuma sala encontrada'])(
    'trata "%s" como colecao vazia',
    async (detail) => {
      const { client } = setup(() => jsonResponse({ title: 'Business error', detail }, 400))

      await expect(client.requestList('/api/rooms')).resolves.toEqual([])
    },
  )

  it('mantem como erro um 400 de negocio que nao e ausencia de resultado', async () => {
    const { client } = setup(() =>
      jsonResponse({ title: 'Business error', detail: 'Esta sala já está cadastrada.' }, 400),
    )

    await expect(client.requestList('/api/rooms')).rejects.toBeInstanceOf(BusinessError)
  })

  it('nao mascara 401 nem 500', async () => {
    const { client } = setup(() => new Response(null, { status: 401 }))

    await expect(client.requestList('/api/rooms')).rejects.toBeInstanceOf(UnauthorizedError)
  })
})

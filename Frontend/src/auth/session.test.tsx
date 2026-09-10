import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { apiClient } from '@/api'
import { AuthProvider } from '@/auth/AuthProvider'
import { TOKEN_STORAGE_KEY, clearToken } from '@/auth/token-store'
import { routes } from '@/routes'
import { makeTokenExpiringIn } from '@/test/make-token'

function jsonResponse(body: unknown, status: number): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

/** Roteia por URL — a Home (spec 009) busca salas e disponibilidade ao montar. */
function stubFetch(handler: (url: string, init?: RequestInit) => Response) {
  const fetchMock = vi.fn((input: RequestInfo | URL, init?: RequestInit) =>
    Promise.resolve(handler(String(input), init)),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

/** Resposta vazia para as duas chamadas que a Home sempre faz, ou null se a URL for de outra coisa. */
function homeDataResponse(url: string): Response | null {
  if (url.includes('/api/rooms')) return jsonResponse([], 200)
  if (url.includes('/api/reservations/availability')) return jsonResponse([], 200)
  return null
}

function renderApp(initialEntries: string[] = ['/']) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createMemoryRouter(routes, { initialEntries })
  render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </QueryClientProvider>,
  )
  return router
}

const HOME_HEADING = 'Salas'

beforeEach(() => {
  clearToken()
  sessionStorage.clear()
})

describe('rota protegida', () => {
  it('envia ao login quem nao tem sessao', async () => {
    renderApp(['/'])

    expect(await screen.findByLabelText('Senha')).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: HOME_HEADING })).not.toBeInTheDocument()
  })

  it('restaura a sessao guardada e entra direto na Home', async () => {
    stubFetch((url) => homeDataResponse(url) ?? jsonResponse({}, 200))
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(30, 'joao@email.com'))

    renderApp(['/'])

    expect(await screen.findByRole('heading', { name: HOME_HEADING })).toBeInTheDocument()
    expect(screen.getByText('joao@email.com')).toBeInTheDocument()
  })

  it('descarta token expirado sem chamar a API', async () => {
    const fetchMock = stubFetch(() => jsonResponse({}, 200))
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(-1))

    renderApp(['/'])

    expect(await screen.findByLabelText('Senha')).toBeInTheDocument()
    expect(fetchMock).not.toHaveBeenCalled()
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })
})

describe('login', () => {
  it('autentica e volta para a rota pretendida', async () => {
    stubFetch((url) => {
      if (url.includes('/api/auth/login')) {
        return jsonResponse(
          { token: makeTokenExpiringIn(60, 'joao@email.com'), expiresAt: '2026-09-02T18:00:00Z' },
          200,
        )
      }
      return homeDataResponse(url) ?? jsonResponse({}, 200)
    })
    const router = renderApp(['/'])

    await userEvent.type(await screen.findByLabelText('E-mail'), 'joao@email.com')
    await userEvent.type(screen.getByLabelText('Senha'), 'segredo123')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByRole('heading', { name: HOME_HEADING })).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/')
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).not.toBeNull()
  })

  it('mostra o erro do backend e limpa a senha quando a credencial e invalida', async () => {
    stubFetch(() =>
      jsonResponse({ title: 'Business error', detail: 'E-mail ou senha inválidos.' }, 400),
    )
    renderApp(['/'])

    await userEvent.type(await screen.findByLabelText('E-mail'), 'joao@email.com')
    const senha = screen.getByLabelText('Senha')
    await userEvent.type(senha, 'errada')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('E-mail ou senha inválidos.')
    expect(senha).toHaveValue('')
    expect(screen.queryByRole('heading', { name: HOME_HEADING })).not.toBeInTheDocument()
  })

  it('nao envia requisicao quando o e-mail tem formato invalido', async () => {
    const fetchMock = stubFetch(() => jsonResponse({}, 200))
    renderApp(['/'])

    await userEvent.type(await screen.findByLabelText('E-mail'), 'nao-e-email')
    await userEvent.type(screen.getByLabelText('Senha'), 'segredo123')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Informe um e-mail válido.')
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('envia a requisicao mesmo com senha curta — o comprimento e regra do servidor', async () => {
    const fetchMock = stubFetch(() =>
      jsonResponse({ token: makeTokenExpiringIn(60), expiresAt: '2026-09-02T18:00:00Z' }, 200),
    )
    renderApp(['/'])

    await userEvent.type(await screen.findByLabelText('E-mail'), 'joao@email.com')
    await userEvent.type(screen.getByLabelText('Senha'), 'abc')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledOnce()
    })
  })
})

describe('sessao expirada durante o uso', () => {
  it('um 401 encerra a sessao e devolve ao login', async () => {
    stubFetch((url) => homeDataResponse(url) ?? jsonResponse({}, 200))
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(30))
    renderApp(['/'])
    expect(await screen.findByRole('heading', { name: HOME_HEADING })).toBeInTheDocument()

    stubFetch(() => new Response(null, { status: 401 }))
    await act(async () => {
      await apiClient.request('/api/rooms').catch(() => undefined)
    })

    expect(await screen.findByLabelText('Senha')).toBeInTheDocument()
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })
})

describe('logout', () => {
  it('limpa a sessao e devolve ao login', async () => {
    stubFetch((url) => homeDataResponse(url) ?? jsonResponse({}, 200))
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(30))
    renderApp(['/'])

    await userEvent.click(await screen.findByRole('button', { name: 'Sair' }))

    expect(await screen.findByLabelText('Senha')).toBeInTheDocument()
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })
})

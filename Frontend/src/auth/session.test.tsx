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

function stubFetch(responder: () => Response) {
  const fetchMock = vi.fn(() => Promise.resolve(responder()))
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function renderApp(initialEntries: string[] = ['/']) {
  const router = createMemoryRouter(routes, { initialEntries })
  render(
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>,
  )
  return router
}

const HOME_TEXT = 'Salas e reservas aparecerão aqui.'

beforeEach(() => {
  clearToken()
  sessionStorage.clear()
})

describe('rota protegida', () => {
  it('envia ao login quem nao tem sessao', async () => {
    renderApp(['/'])

    expect(await screen.findByLabelText('Senha')).toBeInTheDocument()
    expect(screen.queryByText(HOME_TEXT)).not.toBeInTheDocument()
  })

  it('restaura a sessao guardada e entra direto na Home', async () => {
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(30, 'joao@email.com'))

    renderApp(['/'])

    expect(await screen.findByText(HOME_TEXT)).toBeInTheDocument()
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
    stubFetch(() =>
      jsonResponse(
        { token: makeTokenExpiringIn(60, 'joao@email.com'), expiresAt: '2026-09-02T18:00:00Z' },
        200,
      ),
    )
    const router = renderApp(['/'])

    await userEvent.type(await screen.findByLabelText('E-mail'), 'joao@email.com')
    await userEvent.type(screen.getByLabelText('Senha'), 'segredo123')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByText(HOME_TEXT)).toBeInTheDocument()
    expect(router.state.location.pathname).toBe('/')
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).not.toBeNull()
  })

  it('mostra o erro do backend e limpa a senha quando a credencial e invalida', async () => {
    stubFetch(() =>
      jsonResponse({ title: 'Business error', detail: 'Invalid email or password!' }, 400),
    )
    renderApp(['/'])

    await userEvent.type(await screen.findByLabelText('E-mail'), 'joao@email.com')
    const senha = screen.getByLabelText('Senha')
    await userEvent.type(senha, 'errada')
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid email or password!')
    expect(senha).toHaveValue('')
    expect(screen.queryByText(HOME_TEXT)).not.toBeInTheDocument()
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
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(30))
    renderApp(['/'])
    expect(await screen.findByText(HOME_TEXT)).toBeInTheDocument()

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
    sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(30))
    renderApp(['/'])

    await userEvent.click(await screen.findByRole('button', { name: 'Sair' }))

    expect(await screen.findByLabelText('Senha')).toBeInTheDocument()
    expect(sessionStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull()
  })
})

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '@/auth/AuthProvider'
import { TOKEN_STORAGE_KEY } from '@/auth/token-store'
import { HomePage } from '@/features/home/HomePage'
import { makeTokenExpiringIn } from '@/test/make-token'

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

const room = {
  id: 'r-1',
  name: 'Sala Azul',
  number: 101,
  planSlot: null,
  equipments: [
    {
      id: 'e-1',
      type: 'Tv',
      placement: 'Parede',
      brand: 'Samsung',
      serialNumber: 'SN-002',
      purchaseDate: '2023-05-02T00:00:00Z',
      roomId: 'r-1',
    },
  ],
}

const availableRoom = {
  roomId: 'r-1',
  roomName: 'Sala Azul',
  roomNumber: 101,
  status: 'Disponivel' as const,
  busyUntil: null,
  nextReservationAt: null,
}

function stubApi(handler: (url: string, init?: RequestInit) => Response) {
  const fetchMock = vi.fn((input: RequestInfo | URL, init?: RequestInit) =>
    Promise.resolve(handler(String(input), init)),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <HomePage />
      </AuthProvider>
    </QueryClientProvider>,
  )
}

beforeEach(() => {
  sessionStorage.clear()
  sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(60))
})

describe('grade de salas', () => {
  it('mostra estado vazio quando não há sala cadastrada', async () => {
    stubApi(() => jsonResponse([]))
    renderPage()

    expect(await screen.findByText('Nenhuma sala cadastrada')).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('mostra estado de erro com opção de repetir quando a disponibilidade falha', async () => {
    stubApi((url) =>
      url.includes('/availability')
        ? jsonResponse({ title: 'Internal server error' }, 500)
        : jsonResponse([room]),
    )
    renderPage()

    expect(await screen.findByRole('alert')).toHaveTextContent('Não foi possível carregar as salas.')
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })

  it('mostra um card por sala com nome, número e status', async () => {
    stubApi((url) =>
      url.includes('/availability') ? jsonResponse([availableRoom]) : jsonResponse([room]),
    )
    renderPage()

    const card = await screen.findByRole('button', { name: /Sala Azul/ })
    expect(within(card).getByText('nº 101')).toBeInTheDocument()
    expect(within(card).getByText('Livre')).toBeInTheDocument()
  })

  it('mostra "Em uso" quando a sala está ocupada agora', async () => {
    const busy = { ...availableRoom, status: 'EmUso' as const, busyUntil: '2099-01-01T16:00:00Z' }
    stubApi((url) => (url.includes('/availability') ? jsonResponse([busy]) : jsonResponse([room])))
    renderPage()

    const card = await screen.findByRole('button', { name: /Sala Azul/ })
    expect(within(card).getByText('Em uso')).toBeInTheDocument()
  })
})

describe('painel de detalhe', () => {
  it('abre ao clicar no card e mostra os equipamentos', async () => {
    stubApi((url) =>
      url.includes('/availability') ? jsonResponse([availableRoom]) : jsonResponse([room]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: /Sala Azul/ }))

    expect(await screen.findByText(/SN-002/)).toBeInTheDocument()
  })

  it('mostra o horário de contexto quando em uso', async () => {
    const busy = { ...availableRoom, status: 'EmUso' as const, busyUntil: '2099-01-01T16:00:00Z' }
    stubApi((url) => (url.includes('/availability') ? jsonResponse([busy]) : jsonResponse([room])))
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: /Sala Azul/ }))

    expect(await screen.findByText(/Ocupada até/)).toBeInTheDocument()
  })

  it('abre o formulário de nova reserva com a sala já selecionada', async () => {
    stubApi((url) =>
      url.includes('/availability') ? jsonResponse([availableRoom]) : jsonResponse([room]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: /Sala Azul/ }))
    await userEvent.click(await screen.findByRole('button', { name: 'Reservar esta sala' }))

    const dialog = await screen.findByRole('dialog')
    expect(within(dialog).getByLabelText('Sala')).toHaveValue('r-1')
  })

  it('fecha com Esc', async () => {
    stubApi((url) =>
      url.includes('/availability') ? jsonResponse([availableRoom]) : jsonResponse([room]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: /Sala Azul/ }))
    await waitFor(() => {
      expect(screen.getByText(/SN-002/)).toBeInTheDocument()
    })

    await userEvent.keyboard('{Escape}')

    await waitFor(() => {
      expect(screen.queryByText(/SN-002/)).not.toBeInTheDocument()
    })
  })
})

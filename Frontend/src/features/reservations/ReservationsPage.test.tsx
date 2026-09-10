import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '@/auth/AuthProvider'
import { TOKEN_STORAGE_KEY } from '@/auth/token-store'
import { ReservationsPage } from '@/features/reservations/ReservationsPage'
import { makeTokenExpiringIn } from '@/test/make-token'

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

const room = { id: 'r-1', name: 'Sala Azul', number: 101, planSlot: 3, equipments: [] }

const reservation = {
  id: 'res-1',
  userId: 'user-1',
  userName: 'João',
  roomId: 'r-1',
  roomName: 'Sala Azul',
  roomNumber: 101,
  startDate: '2099-01-01T14:00:00Z',
  endDate: '2099-01-01T15:00:00Z',
}

/** Roteia por URL para não depender da ordem das chamadas do React Query. */
function stubApi(handler: (url: string, init?: RequestInit) => Response) {
  const fetchMock = vi.fn((input: RequestInfo | URL, init?: RequestInit) =>
    Promise.resolve(handler(String(input), init)),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function setViewport(wide: boolean) {
  vi.stubGlobal('matchMedia', (query: string) => ({
    matches: wide,
    media: query,
    addEventListener: () => {},
    removeEventListener: () => {},
  }))
}

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ReservationsPage />
      </AuthProvider>
    </QueryClientProvider>,
  )
}

beforeEach(() => {
  sessionStorage.clear()
  sessionStorage.setItem(TOKEN_STORAGE_KEY, makeTokenExpiringIn(60))
  setViewport(true)
})

describe('listagem', () => {
  it('trata o 400 de "Nenhuma reserva encontrada." como estado vazio, não como erro', async () => {
    stubApi((url) =>
      url.includes('/api/reservations')
        ? jsonResponse({ title: 'Business error', detail: 'Nenhuma reserva encontrada.' }, 400)
        : jsonResponse([room]),
    )

    renderPage()

    expect(await screen.findByText('Nenhuma reserva ainda')).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('mostra sala, número, início e fim', async () => {
    stubApi((url) =>
      url.includes('/api/reservations') ? jsonResponse([reservation]) : jsonResponse([room]),
    )

    renderPage()

    const row = await screen.findByRole('row', { name: /Sala Azul/ })
    expect(within(row).getByText('101')).toBeInTheDocument()
  })

  it('mostra estado de erro com opção de repetir quando a API falha', async () => {
    stubApi((url) =>
      url.includes('/api/reservations')
        ? jsonResponse({ title: 'Internal server error' }, 500)
        : jsonResponse([room]),
    )

    renderPage()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Não foi possível carregar as reservas.',
    )
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })

  it('não oferece cancelar quando a reserva já começou', async () => {
    const started = { ...reservation, startDate: '2020-01-01T10:00:00Z', endDate: '2020-01-01T11:00:00Z' }
    stubApi((url) => (url.includes('/api/reservations') ? jsonResponse([started]) : jsonResponse([room])))

    renderPage()

    await screen.findByText('Sala Azul')
    expect(screen.queryByRole('button', { name: /^Cancelar/ })).not.toBeInTheDocument()
  })
})

describe('nova reserva', () => {
  it('sugere sala e intervalo, e cria a reserva sem recarregar a página', async () => {
    let created = false
    stubApi((url, init) => {
      if (init?.method === 'POST') {
        created = true
        return jsonResponse({ reservation }, 201)
      }
      if (url.includes('/api/reservations')) return jsonResponse(created ? [reservation] : [])
      return jsonResponse([room])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova reserva' }))
    const dialog = await screen.findByRole('dialog')

    expect(within(dialog).getByLabelText('Início')).toHaveValue()
    expect(within(dialog).getByLabelText('Fim')).toHaveValue()

    await userEvent.selectOptions(within(dialog).getByLabelText('Sala'), 'r-1')
    await userEvent.click(within(dialog).getByRole('button', { name: 'Reservar' }))

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    })
    expect(await screen.findByText('Sala Azul')).toBeInTheDocument()
  })

  it('não envia requisição quando o início está no passado', async () => {
    const fetchMock = stubApi((url) =>
      url.includes('/api/reservations') ? jsonResponse([]) : jsonResponse([room]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova reserva' }))
    const dialog = await screen.findByRole('dialog')
    await userEvent.selectOptions(within(dialog).getByLabelText('Sala'), 'r-1')
    fireEvent.change(within(dialog).getByLabelText('Início'), { target: { value: '2020-01-01T10:00' } })
    const callsBefore = fetchMock.mock.calls.length

    await userEvent.click(within(dialog).getByRole('button', { name: 'Reservar' }))

    expect(await within(dialog).findByRole('alert')).toHaveTextContent(
      'O horário de início precisa estar no futuro.',
    )
    expect(fetchMock.mock.calls.length).toBe(callsBefore)
  })

  it('não envia requisição quando o fim é antes do início', async () => {
    const fetchMock = stubApi((url) =>
      url.includes('/api/reservations') ? jsonResponse([]) : jsonResponse([room]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova reserva' }))
    const dialog = await screen.findByRole('dialog')
    await userEvent.selectOptions(within(dialog).getByLabelText('Sala'), 'r-1')
    fireEvent.change(within(dialog).getByLabelText('Início'), { target: { value: '2099-01-01T15:00' } })
    fireEvent.change(within(dialog).getByLabelText('Fim'), { target: { value: '2099-01-01T14:00' } })
    const callsBefore = fetchMock.mock.calls.length

    await userEvent.click(within(dialog).getByRole('button', { name: 'Reservar' }))

    expect(await within(dialog).findByRole('alert')).toHaveTextContent(
      'O término precisa ser depois do início.',
    )
    expect(fetchMock.mock.calls.length).toBe(callsBefore)
  })

  it('exibe a mensagem do backend quando a sala já está reservada, preservando o que preenchi', async () => {
    stubApi((url, init) => {
      if (init?.method === 'POST') {
        return jsonResponse(
          { title: 'Business error', detail: 'A sala já está reservada nesse período.' },
          400,
        )
      }
      if (url.includes('/api/reservations')) return jsonResponse([])
      return jsonResponse([room])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova reserva' }))
    const dialog = await screen.findByRole('dialog')
    await userEvent.selectOptions(within(dialog).getByLabelText('Sala'), 'r-1')
    await userEvent.click(within(dialog).getByRole('button', { name: 'Reservar' }))

    expect(await within(dialog).findByRole('alert')).toHaveTextContent(
      'A sala já está reservada nesse período.',
    )
    expect(within(dialog).getByLabelText('Sala')).toHaveValue('r-1')
  })

  it('fecha com Esc', async () => {
    stubApi((url) => (url.includes('/api/reservations') ? jsonResponse([]) : jsonResponse([room])))
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova reserva' }))
    await screen.findByRole('dialog')

    await userEvent.keyboard('{Escape}')

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    })
  })
})

describe('cancelamento', () => {
  it('pede confirmação e some da lista ao confirmar', async () => {
    let cancelled = false
    stubApi((url, init) => {
      if (init?.method === 'DELETE') {
        cancelled = true
        return jsonResponse({ id: reservation.id })
      }
      if (url.includes('/api/reservations')) return jsonResponse(cancelled ? [] : [reservation])
      return jsonResponse([room])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: /^Cancelar/ }))
    const dialog = await screen.findByRole('dialog')
    expect(within(dialog).getByText(/Esta ação não pode ser desfeita/)).toBeInTheDocument()

    await userEvent.click(within(dialog).getByRole('button', { name: 'Cancelar reserva' }))

    await waitFor(() => {
      expect(screen.queryByText('Sala Azul')).not.toBeInTheDocument()
    })
  })

  it('mostra a mensagem do servidor quando a recusa acontece', async () => {
    stubApi((url, init) => {
      if (init?.method === 'DELETE') {
        return jsonResponse(
          { title: 'Business error', detail: 'Não é possível cancelar uma reserva já iniciada.' },
          400,
        )
      }
      if (url.includes('/api/reservations')) return jsonResponse([reservation])
      return jsonResponse([room])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: /^Cancelar/ }))
    const dialog = await screen.findByRole('dialog')
    await userEvent.click(within(dialog).getByRole('button', { name: 'Cancelar reserva' }))

    expect(await within(dialog).findByRole('alert')).toHaveTextContent(
      'Não é possível cancelar uma reserva já iniciada.',
    )
  })
})

describe('apresentação por largura', () => {
  it('em tela estreita mostra cartões, sem tabela', async () => {
    setViewport(false)
    stubApi((url) =>
      url.includes('/api/reservations') ? jsonResponse([reservation]) : jsonResponse([room]),
    )

    renderPage()

    const card = await screen.findByRole('listitem')
    expect(within(card).getByText('Sala Azul')).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('em tela larga mostra tabela, sem cartões', async () => {
    setViewport(true)
    stubApi((url) =>
      url.includes('/api/reservations') ? jsonResponse([reservation]) : jsonResponse([room]),
    )

    renderPage()

    expect(await screen.findByRole('table')).toBeInTheDocument()
    expect(screen.queryByRole('listitem')).not.toBeInTheDocument()
  })
})

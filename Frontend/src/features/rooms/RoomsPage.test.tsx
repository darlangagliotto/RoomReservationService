import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { RoomsPage } from '@/features/rooms/RoomsPage'

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
  planSlot: 3,
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

/** Roteia por URL para não depender da ordem das chamadas do React Query. */
function stubApi(handler: (url: string, init?: RequestInit) => Response) {
  const fetchMock = vi.fn((input: RequestInfo | URL, init?: RequestInit) =>
    Promise.resolve(handler(String(input), init)),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function setViewport(wide: boolean) {
  // jsdom devolve matches:false para tudo; sem controlar isso, a suite so
  // exercitaria a apresentacao estreita.
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
      <RoomsPage />
    </QueryClientProvider>,
  )
}

beforeEach(() => {
  sessionStorage.clear()
  setViewport(true)
})

describe('listagem', () => {
  it('trata o 400 de "Nenhuma sala encontrada." como estado vazio, não como erro', async () => {
    stubApi((url) =>
      url.includes('/api/rooms')
        ? jsonResponse({ title: 'Business error', detail: 'Nenhuma sala encontrada.' }, 400)
        : jsonResponse([]),
    )

    renderPage()

    expect(await screen.findByText('Nenhuma sala cadastrada')).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('mostra nome, número, posição na planta e equipamentos na tabela', async () => {
    stubApi((url) => (url.includes('/api/rooms') ? jsonResponse([room]) : jsonResponse([])))

    renderPage()

    const row = await screen.findByRole('row', { name: /Sala Azul/ })
    expect(within(row).getByText('101')).toBeInTheDocument()
    expect(within(row).getByText('3')).toBeInTheDocument()
    expect(within(row).getByText(/Tv \(parede\)/)).toBeInTheDocument()
  })

  it('mostra estado de erro com opção de repetir quando a API falha', async () => {
    stubApi((url) =>
      url.includes('/api/rooms')
        ? jsonResponse({ title: 'Internal server error' }, 500)
        : jsonResponse([]),
    )

    renderPage()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Não foi possível carregar as salas.',
    )
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })
})

describe('cadastro', () => {
  it('não envia requisição quando o nome é curto demais', async () => {
    const fetchMock = stubApi((url) =>
      url.includes('/api/rooms') ? jsonResponse([room]) : jsonResponse([]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova sala' }))
    await userEvent.type(screen.getByLabelText('Nome'), 'ab')
    await userEvent.type(screen.getByLabelText('Número'), '5')
    const callsBefore = fetchMock.mock.calls.length

    await userEvent.click(screen.getByRole('button', { name: 'Cadastrar sala' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'O nome precisa de ao menos 3 caracteres.',
    )
    expect(fetchMock.mock.calls.length).toBe(callsBefore)
  })

  it('exibe a mensagem do backend quando o slot da planta já está ocupado', async () => {
    stubApi((url, init) => {
      if (init?.method === 'POST') {
        return jsonResponse({ title: 'Business error', detail: 'Esta posição da planta já está ocupada.' }, 400)
      }
      return url.includes('/api/rooms') ? jsonResponse([room]) : jsonResponse([])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova sala' }))
    await userEvent.type(screen.getByLabelText('Nome'), 'Sala Verde')
    await userEvent.type(screen.getByLabelText('Número'), '102')
    await userEvent.type(screen.getByLabelText('Posição na planta'), '3')
    await userEvent.click(screen.getByRole('button', { name: 'Cadastrar sala' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Esta posição da planta já está ocupada.')
  })

  it('volta para a lista depois de cadastrar', async () => {
    stubApi((url, init) => {
      if (init?.method === 'POST') return jsonResponse({ room }, 201)
      return url.includes('/api/rooms') ? jsonResponse([room]) : jsonResponse([])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova sala' }))
    await userEvent.type(screen.getByLabelText('Nome'), 'Sala Verde')
    await userEvent.type(screen.getByLabelText('Número'), '102')
    await userEvent.click(screen.getByRole('button', { name: 'Cadastrar sala' }))

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Nova sala' })).toBeInTheDocument()
    })
  })

  it('só oferece equipamentos livres', async () => {
    const unassignedEquipment = { ...room.equipments[0], id: 'e-9', serialNumber: 'SN-LIVRE', roomId: null }
    stubApi((url) =>
      url.includes('/api/equipments') ? jsonResponse([unassignedEquipment]) : jsonResponse([room]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Nova sala' }))

    expect(await screen.findByText(/SN-LIVRE/)).toBeInTheDocument()
  })
})

describe('apresentação por largura', () => {
  it('em tela estreita mostra cartões, sem tabela — a mesma informação', async () => {
    setViewport(false)
    stubApi((url) => (url.includes('/api/rooms') ? jsonResponse([room]) : jsonResponse([])))

    renderPage()

    const card = await screen.findByRole('listitem')
    // `selector` desempata com o rótulo sr-only do botão Editar, que repete o
    // nome de propósito para dar nome acessível distinto a cada botão.
    expect(within(card).getByText('Sala Azul', { selector: 'p' })).toBeInTheDocument()
    expect(within(card).getByText(/nº 101/)).toBeInTheDocument()
    // Só uma das apresentações existe no DOM: renderizar as duas duplicaria o
    // conteúdo para leitor de tela.
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('em tela larga mostra tabela, sem cartões', async () => {
    setViewport(true)
    stubApi((url) => (url.includes('/api/rooms') ? jsonResponse([room]) : jsonResponse([])))

    renderPage()

    expect(await screen.findByRole('table')).toBeInTheDocument()
    expect(screen.queryByText(/nº 101/)).not.toBeInTheDocument()
  })
})

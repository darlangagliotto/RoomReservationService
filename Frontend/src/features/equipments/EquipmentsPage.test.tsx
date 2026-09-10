import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { EquipmentsPage } from '@/features/equipments/EquipmentsPage'

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

const equipment = {
  id: 'e-1',
  type: 'Projetor',
  placement: 'Teto',
  brand: 'Epson',
  serialNumber: 'SN-123',
  purchaseDate: '2024-01-10T00:00:00Z',
  roomId: 'r-1',
}

const room = { id: 'r-1', name: 'Sala Azul', number: 101, planSlot: 3, equipments: [] }

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
      <EquipmentsPage />
    </QueryClientProvider>,
  )
}

beforeEach(() => {
  sessionStorage.clear()
  setViewport(true)
})

describe('listagem', () => {
  it('mostra estado vazio, não erro, quando não há equipamento cadastrado', async () => {
    stubApi((url) => (url.includes('/api/equipments') ? jsonResponse([]) : jsonResponse([room])))

    renderPage()

    expect(await screen.findByText('Nenhum equipamento cadastrado')).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('mostra tipo, âncora, marca, número de série, data de compra e a sala', async () => {
    stubApi((url) =>
      url.includes('/api/equipments') ? jsonResponse([equipment]) : jsonResponse([room]),
    )

    renderPage()

    const row = await screen.findByRole('row', { name: /Projetor/ })
    expect(within(row).getByText('Teto')).toBeInTheDocument()
    expect(within(row).getByText('Epson')).toBeInTheDocument()
    expect(within(row).getByText('SN-123')).toBeInTheDocument()
    expect(within(row).getByText('10/01/2024')).toBeInTheDocument()
    expect(within(row).getByText('Sala Azul')).toBeInTheDocument()
  })

  it('mostra "Livre" quando o equipamento não está alocado', async () => {
    const free = { ...equipment, roomId: null }
    stubApi((url) => (url.includes('/api/equipments') ? jsonResponse([free]) : jsonResponse([room])))

    renderPage()

    const row = await screen.findByRole('row', { name: /Projetor/ })
    expect(within(row).getByText('Livre')).toBeInTheDocument()
  })

  it('filtra por tipo', async () => {
    const fetchMock = stubApi((url) =>
      url.includes('/api/equipments') ? jsonResponse([equipment]) : jsonResponse([room]),
    )

    renderPage()
    await screen.findByText('Epson')

    await userEvent.selectOptions(screen.getByLabelText('Tipo'), 'Projetor')

    await waitFor(() => {
      expect(fetchMock.mock.calls.some(([input]) => String(input).includes('type=Projetor'))).toBe(
        true,
      )
    })
  })

  it('filtra por "apenas livres"', async () => {
    const fetchMock = stubApi((url) =>
      url.includes('/api/equipments') ? jsonResponse([equipment]) : jsonResponse([room]),
    )

    renderPage()
    await screen.findByText('Epson')

    await userEvent.click(screen.getByLabelText('Apenas livres'))

    await waitFor(() => {
      expect(
        fetchMock.mock.calls.some(([input]) => String(input).includes('unassigned=true')),
      ).toBe(true)
    })
  })

  it('mostra estado de erro com opção de repetir quando a API falha', async () => {
    stubApi((url) =>
      url.includes('/api/equipments')
        ? jsonResponse({ title: 'Internal server error' }, 500)
        : jsonResponse([room]),
    )

    renderPage()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Não foi possível carregar os equipamentos.',
    )
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })
})

describe('cadastro', () => {
  it('mostra exatamente os 11 valores do vocabulário no seletor de tipo', async () => {
    stubApi((url) => (url.includes('/api/equipments') ? jsonResponse([]) : jsonResponse([room])))
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Novo equipamento' }))

    const options = screen.getAllByRole('option', { name: /.+/ })
    // 11 tipos + a opcao "Selecione…"
    expect(options).toHaveLength(12)
  })

  it('não envia requisição quando a data de compra é futura', async () => {
    const fetchMock = stubApi((url) =>
      url.includes('/api/equipments') ? jsonResponse([]) : jsonResponse([room]),
    )
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Novo equipamento' }))
    await userEvent.selectOptions(screen.getByLabelText('Tipo'), 'Projetor')
    await userEvent.type(screen.getByLabelText('Marca'), 'Epson')
    await userEvent.type(screen.getByLabelText('Número de série'), 'SN-999')

    const futureYear = new Date().getFullYear() + 1
    fireEvent.change(screen.getByLabelText('Data de compra'), {
      target: { value: `${futureYear}-01-10` },
    })
    const callsBefore = fetchMock.mock.calls.length

    await userEvent.click(screen.getByRole('button', { name: 'Cadastrar equipamento' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'A data de compra não pode ser no futuro.',
    )
    expect(fetchMock.mock.calls.length).toBe(callsBefore)
  })

  it('exibe a mensagem do backend quando o número de série já está cadastrado', async () => {
    stubApi((url, init) => {
      if (init?.method === 'POST') {
        return jsonResponse(
          { title: 'Business error', detail: 'Este equipamento já está cadastrado.' },
          400,
        )
      }
      return url.includes('/api/equipments') ? jsonResponse([]) : jsonResponse([room])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Novo equipamento' }))
    await userEvent.selectOptions(screen.getByLabelText('Tipo'), 'Projetor')
    await userEvent.type(screen.getByLabelText('Marca'), 'Epson')
    await userEvent.type(screen.getByLabelText('Número de série'), 'SN-123')
    fireEvent.change(screen.getByLabelText('Data de compra'), { target: { value: '2024-01-10' } })
    await userEvent.click(screen.getByRole('button', { name: 'Cadastrar equipamento' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Este equipamento já está cadastrado.',
    )
    // O que foi digitado continua no formulario.
    expect(screen.getByLabelText('Marca')).toHaveValue('Epson')
  })

  it('volta para a lista e mostra o equipamento cadastrado sem recarregar', async () => {
    stubApi((url, init) => {
      if (init?.method === 'POST') return jsonResponse({ equipment }, 201)
      return url.includes('/api/equipments') ? jsonResponse([]) : jsonResponse([room])
    })
    renderPage()

    await userEvent.click(await screen.findByRole('button', { name: 'Novo equipamento' }))
    await userEvent.selectOptions(screen.getByLabelText('Tipo'), 'Projetor')
    await userEvent.type(screen.getByLabelText('Marca'), 'Epson')
    await userEvent.type(screen.getByLabelText('Número de série'), 'SN-123')
    fireEvent.change(screen.getByLabelText('Data de compra'), { target: { value: '2024-01-10' } })
    await userEvent.click(screen.getByRole('button', { name: 'Cadastrar equipamento' }))

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Novo equipamento' })).toBeInTheDocument()
    })
  })
})

describe('apresentação por largura', () => {
  it('em tela estreita mostra cartões, sem tabela', async () => {
    setViewport(false)
    stubApi((url) =>
      url.includes('/api/equipments') ? jsonResponse([equipment]) : jsonResponse([room]),
    )

    renderPage()

    const card = await screen.findByRole('listitem')
    expect(within(card).getByText('Projetor')).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('em tela larga mostra tabela, sem cartões', async () => {
    setViewport(true)
    stubApi((url) =>
      url.includes('/api/equipments') ? jsonResponse([equipment]) : jsonResponse([room]),
    )

    renderPage()

    expect(await screen.findByRole('table')).toBeInTheDocument()
  })
})

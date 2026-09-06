/*
  Unico lugar do app com `fetch`.

  Traduz as respostas do backend para os tipos de errors.ts. Nenhum componente,
  hook ou feature inspeciona status HTTP — todos consomem os erros tipados.

  O cliente e criado por fabrica (nao um singleton com import de token-store)
  para poder ser exercitado em teste com fetch e token falsos.
*/

import {
  BusinessError,
  NetworkError,
  ServerError,
  UnauthorizedError,
  ValidationError,
} from '@/api/errors'

/**
 * O backend responde 400 quando a busca nao encontra nada, em vez de 200 com
 * lista vazia — consequencia do ADR-011. Ex.: "Nenhuma sala encontrada.",
 * "Nenhuma reserva encontrada.". Isso e ausencia de dado, nao erro.
 *
 * Casar por prosa e fragil: se a mensagem mudar, a tela volta a mostrar erro
 * onde deveria mostrar estado vazio. O caminho definitivo e o backend
 * responder 200 com [] — feito na spec 003 e 004 para os endpoints novos.
 * Este padrao cobre os endpoints antigos ate serem alinhados.
 */
const EMPTY_RESULT_PATTERN = /^nenhum[ao]?\s+.+\s+encontrad[ao]s?[.!]?$/i

interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PATCH' | 'PUT' | 'DELETE'
  body?: unknown
  signal?: AbortSignal
}

export interface ApiClientOptions {
  getToken: () => string | null
  onUnauthorized: () => void
  fetchImpl?: typeof fetch
}

export interface ApiClient {
  request: <T>(path: string, options?: RequestOptions) => Promise<T>
  requestList: <T>(path: string, options?: RequestOptions) => Promise<T[]>
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  try {
    const parsed: unknown = await response.json()
    return typeof parsed === 'object' && parsed !== null ? (parsed as ProblemDetails) : null
  } catch {
    return null
  }
}

function hasFieldErrors(problem: ProblemDetails | null): problem is ProblemDetails & {
  errors: Record<string, string[]>
} {
  return !!problem?.errors && Object.keys(problem.errors).length > 0
}

export function createApiClient({
  getToken,
  onUnauthorized,
  fetchImpl,
}: ApiClientOptions): ApiClient {
  // Resolvido a cada chamada, e nao capturado na criacao: mantem o cliente
  // padrao substituivel em teste e evita chamar fetch desvinculado do global.
  const doFetch: typeof fetch = fetchImpl ?? ((input, init) => globalThis.fetch(input, init))

  async function send(path: string, options: RequestOptions): Promise<Response> {
    const token = getToken()
    const headers = new Headers({ Accept: 'application/json' })

    if (token !== null) {
      headers.set('Authorization', `Bearer ${token}`)
    }
    if (options.body !== undefined) {
      headers.set('Content-Type', 'application/json')
    }

    try {
      return await doFetch(path, {
        method: options.method ?? 'GET',
        headers,
        body: options.body === undefined ? undefined : JSON.stringify(options.body),
        signal: options.signal,
      })
    } catch {
      // Erro de rede nao vaza como TypeError generico do fetch.
      throw new NetworkError()
    }
  }

  /** Converte uma resposta nao-ok no erro tipado correspondente. */
  async function toError(response: Response): Promise<never> {
    if (response.status === 401) {
      onUnauthorized()
      throw new UnauthorizedError()
    }

    const problem = await readProblem(response)

    if (response.status === 400) {
      if (hasFieldErrors(problem)) {
        throw new ValidationError(problem.errors, problem.detail)
      }
      throw new BusinessError(problem?.detail ?? 'Nao foi possivel completar a operacao.')
    }

    throw new ServerError(response.status)
  }

  async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const response = await send(path, options)

    if (!response.ok) {
      return toError(response)
    }

    if (response.status === 204) {
      return undefined as T
    }

    return (await response.json()) as T
  }

  /**
   * Igual a `request`, mas trata "nenhum resultado" como lista vazia em vez de
   * erro de negocio. Use sempre que o endpoint devolver colecao — assim a tela
   * renderiza estado vazio, nao estado de erro.
   */
  async function requestList<T>(path: string, options: RequestOptions = {}): Promise<T[]> {
    try {
      return await request<T[]>(path, options)
    } catch (error) {
      if (error instanceof BusinessError && EMPTY_RESULT_PATTERN.test(error.message.trim())) {
        return []
      }
      throw error
    }
  }

  return { request, requestList }
}

/*
  Tipos de erro da API.

  O backend responde em tres formatos distintos (ver
  docs/architecture/cross-cutting.md#tratamento-de-erros). Traduzir status HTTP
  cru para estes tipos acontece uma unica vez, em client.ts — nenhuma tela deve
  inspecionar `response.status`.
*/

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = new.target.name
    this.status = status
  }
}

/** Regra de negocio violada: 400 com ProblemDetails "Business error". */
export class BusinessError extends ApiError {
  constructor(detail: string) {
    super(detail, 400)
  }
}

/** Validacao de entrada: 400 com ValidationProblemDetails (campo -> mensagens). */
export class ValidationError extends ApiError {
  readonly fieldErrors: Readonly<Record<string, string[]>>

  constructor(fieldErrors: Record<string, string[]>, detail?: string) {
    super(detail ?? 'Verifique os campos destacados.', 400)
    this.fieldErrors = fieldErrors
  }

  /** Primeira mensagem de um campo, no formato que o react-hook-form espera. */
  first(field: string): string | undefined {
    return this.fieldErrors[field]?.[0]
  }
}

/** Token ausente, invalido ou expirado. Encerra a sessao. */
export class UnauthorizedError extends ApiError {
  constructor() {
    super('Sua sessao expirou. Entre novamente.', 401)
  }
}

/** Falha inesperada no servidor. O detail cru nunca vai para a tela. */
export class ServerError extends ApiError {
  constructor(status: number) {
    super('Nao foi possivel completar a operacao. Tente novamente.', status)
  }
}

/** O request nem chegou ao servidor (offline, DNS, CORS, servico fora do ar). */
export class NetworkError extends ApiError {
  constructor() {
    super('Nao foi possivel falar com o servidor. Verifique sua conexao.', 0)
  }
}

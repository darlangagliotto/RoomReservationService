/*
  Leitura do payload do JWT.

  APENAS PARA EXIBICAO. Nenhuma decisao de autorizacao se apoia no que e lido
  aqui — o cliente nao verifica assinatura e o conteudo e trivialmente forjavel.
  Quem autoriza e o backend, a cada requisicao.

  O token emitido pelo AuthService carrega sub, email, jti, iss, aud, nbf e exp.
  Nao ha nome nem papel (ver docs/services/auth-service.md).
*/

export interface JwtPayload {
  sub?: string
  email?: string
  exp?: number
}

function decodeBase64Url(segment: string): string | null {
  const base64 = segment.replaceAll('-', '+').replaceAll('_', '/')
  const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=')

  try {
    const binary = atob(padded)
    const bytes = Uint8Array.from(binary, (char) => char.codePointAt(0) ?? 0)
    return new TextDecoder().decode(bytes)
  } catch {
    return null
  }
}

/** Devolve o payload, ou null se o token for malformado. Nunca lanca. */
export function decodeJwtPayload(token: string): JwtPayload | null {
  const parts = token.split('.')
  const payloadSegment = parts[1]

  if (parts.length !== 3 || !payloadSegment) {
    return null
  }

  const json = decodeBase64Url(payloadSegment)
  if (json === null) {
    return null
  }

  try {
    const parsed: unknown = JSON.parse(json)
    if (typeof parsed !== 'object' || parsed === null) {
      return null
    }
    return parsed as JwtPayload
  } catch {
    return null
  }
}

export function getEmail(token: string): string | null {
  return decodeJwtPayload(token)?.email ?? null
}

/** Instante de expiracao, ou null se o token nao declarar `exp`. */
export function getExpiresAt(token: string): Date | null {
  const exp = decodeJwtPayload(token)?.exp
  return typeof exp === 'number' ? new Date(exp * 1000) : null
}

/**
 * Token expirado? Malformado e token sem `exp` contam como expirados —
 * na duvida, nao restaura a sessao.
 */
export function isExpired(token: string, now: Date = new Date()): boolean {
  const expiresAt = getExpiresAt(token)
  return expiresAt === null || expiresAt.getTime() <= now.getTime()
}

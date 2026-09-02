/*
  Fabrica de JWT para testes. Assinatura e irrelevante: o cliente nunca verifica
  (ver src/lib/jwt.ts) — so o payload importa.
*/

/*
  Um JWT real codifica o JSON em UTF-8 antes do base64. Passar a string direto
  para btoa gera bytes Latin-1 e quebra qualquer caractere acentuado — que e
  exatamente o que o decoder de producao (UTF-8) espera receber correto.
*/
function base64Url(value: string): string {
  const bytes = new TextEncoder().encode(value)
  const binary = Array.from(bytes, (byte) => String.fromCharCode(byte)).join('')
  return btoa(binary).replaceAll('+', '-').replaceAll('/', '_').replaceAll('=', '')
}

export interface TokenPayload {
  sub?: string
  email?: string
  /** Segundos desde a epoca. Omitido = token sem `exp`. */
  exp?: number
}

export function makeToken(payload: TokenPayload): string {
  const header = base64Url(JSON.stringify({ alg: 'HS256', typ: 'JWT' }))
  const body = base64Url(JSON.stringify(payload))
  return `${header}.${body}.assinatura-irrelevante`
}

export function makeTokenExpiringIn(minutes: number, email = 'joao@email.com'): string {
  const exp = Math.floor(Date.now() / 1000) + minutes * 60
  return makeToken({ sub: 'user-1', email, exp })
}

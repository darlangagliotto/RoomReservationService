/*
  Conversao local <-> UTC num unico modulo (spec 008, §5 "Tempo"). A pessoa
  escolhe horario local; o contrato da API e UTC — a alternativa de espalhar
  essa conversao pelos componentes e descobrir o fuso trocado tres telas
  depois.
*/

function pad(n: number): string {
  return String(n).padStart(2, '0')
}

/** Formato que `<input type="datetime-local">` espera, no fuso local do navegador. */
function toDatetimeLocalValue(date: Date): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

/**
 * Valor de `<input type="datetime-local">` (sem fuso — o browser trata como
 * hora local) para o instante UTC que o contrato da API espera.
 */
export function toApiUtc(localDateTime: string): string {
  return new Date(localDateTime).toISOString()
}

/** Instante UTC ISO da API para o valor que `<input type="datetime-local">` espera. */
export function fromApiUtc(isoUtc: string): string {
  return toDatetimeLocalValue(new Date(isoUtc))
}

/** Exibição amigável de um instante UTC, sempre convertida para hora local. */
export function formatLocalDateTime(isoUtc: string): string {
  return new Date(isoUtc).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
}

/**
 * Próxima meia hora cheia, com 1h de duração — palpite deliberado para o caso
 * comum custar dois cliques em vez de quatro, e editável (spec 008, §5).
 */
export function defaultReservationWindow(now: Date = new Date()): { start: string; end: string } {
  const start = new Date(now)
  start.setSeconds(0, 0)
  start.setMinutes(start.getMinutes() < 30 ? 30 : 60)

  const end = new Date(start)
  end.setHours(end.getHours() + 1)

  return { start: toDatetimeLocalValue(start), end: toDatetimeLocalValue(end) }
}

/**
 * Janela de referência da Home (spec 009, §5): "agora" até "agora + N
 * minutos", já em UTC ISO — pronta para `GET /api/reservations/availability`.
 * Sem round-trip por `<input>`, já que ninguém digita esse intervalo.
 */
export function nowWindow(minutes = 30, now: Date = new Date()): { start: string; end: string } {
  const end = new Date(now.getTime() + minutes * 60_000)
  return { start: now.toISOString(), end: end.toISOString() }
}

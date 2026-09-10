/**
 * Datas puras (`YYYY-MM-DD`, sem hora nem fuso) — o formato de
 * `<input type="date">` e o que a spec 006 manda enviar ao backend.
 */

const TODAY_ISO_LENGTH = 10

function todayIso(): string {
  return new Date().toISOString().slice(0, TODAY_ISO_LENGTH)
}

/** Compara como string ISO: funciona porque `YYYY-MM-DD` ordena lexicograficamente. */
export function isFutureDate(value: string): boolean {
  return value > todayIso()
}

export function formatDate(value: string): string {
  return new Date(value).toLocaleDateString('pt-BR', { timeZone: 'UTC' })
}

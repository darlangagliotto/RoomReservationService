import { useSyncExternalStore } from 'react'

/**
 * Escolhe a apresentacao em JS em vez de esconder por CSS.
 *
 * Renderizar tabela E cartoes deixando o CSS ocultar um deles duplica o
 * conteudo no DOM: leitor de tela anuncia duas vezes e o peso da pagina dobra.
 * Aqui so uma das duas existe.
 */
export function useMediaQuery(query: string): boolean {
  return useSyncExternalStore(
    (onChange) => {
      const list = globalThis.matchMedia?.(query)
      list?.addEventListener('change', onChange)
      return () => list?.removeEventListener('change', onChange)
    },
    () => globalThis.matchMedia?.(query).matches ?? false,
    // Sem DOM (SSR, teste de no) assume a apresentacao larga.
    () => true,
  )
}

/** Ponto de virada entre cartoes e tabela, alinhado ao `md` do Tailwind. */
export function useIsWideScreen(): boolean {
  return useMediaQuery('(min-width: 768px)')
}

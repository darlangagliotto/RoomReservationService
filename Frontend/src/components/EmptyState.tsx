import type { ReactNode } from 'react'

interface EmptyStateProps {
  title: string
  description: string
  /** Acao seguinte, quando houver uma. Estado vazio sem saida e beco sem saida. */
  action?: ReactNode
}

/**
 * Ausencia de dado nao e erro. Lembre que o backend responde 400 para busca sem
 * resultado — `requestList` ja converte isso em colecao vazia, e a tela mostra
 * este componente, nao uma mensagem de falha.
 */
export function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <div className="rounded-lg border border-dashed border-border px-6 py-14 text-center">
      <h2 className="font-display text-lg font-semibold tracking-tight">{title}</h2>
      <p className="mx-auto mt-2 max-w-md text-sm text-muted">{description}</p>
      {action && <div className="mt-6 flex justify-center">{action}</div>}
    </div>
  )
}

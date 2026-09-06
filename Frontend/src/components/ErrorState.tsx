import { Button } from '@/components/ui/button'

interface ErrorStateProps {
  title?: string
  description: string
  onRetry?: () => void
}

/**
 * Estado de falha. Nunca mostra ProblemDetails cru nem stack trace — o
 * client.ts ja traduziu o erro para linguagem de produto.
 */
export function ErrorState({ title = 'Algo deu errado', description, onRetry }: ErrorStateProps) {
  return (
    <div
      role="alert"
      className="rounded-lg border border-danger/40 bg-surface px-6 py-10 text-center"
    >
      <h2 className="font-display text-lg font-semibold tracking-tight">{title}</h2>
      <p className="mx-auto mt-2 max-w-md text-sm text-muted">{description}</p>
      {onRetry && (
        <div className="mt-6 flex justify-center">
          <Button variant="ghost" size="sm" onClick={onRetry}>
            Tentar novamente
          </Button>
        </div>
      )}
    </div>
  )
}

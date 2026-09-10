import type { ComponentProps } from 'react'
import { cn } from '@/lib/utils'

/**
 * `<select>` nativo estilizado, nao um Radix. Teclado e leitor de tela ja
 * funcionam de graca; nao ha justificativa para a dependencia extra aqui.
 */
export function Select({ className, children, ...props }: ComponentProps<'select'>) {
  return (
    <select
      className={cn(
        'h-11 w-full rounded-md border border-border bg-surface px-3 text-base text-foreground',
        'aria-[invalid=true]:border-danger',
        className,
      )}
      {...props}
    >
      {children}
    </select>
  )
}

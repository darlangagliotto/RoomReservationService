import type { ComponentProps } from 'react'
import { cn } from '@/lib/utils'

export function Input({ className, ...props }: ComponentProps<'input'>) {
  return (
    <input
      className={cn(
        'h-11 w-full rounded-md border border-border bg-surface px-3 text-base text-foreground',
        'placeholder:text-muted',
        'aria-[invalid=true]:border-danger',
        className,
      )}
      {...props}
    />
  )
}

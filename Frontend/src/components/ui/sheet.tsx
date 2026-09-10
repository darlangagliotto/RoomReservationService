import * as RadixDialog from '@radix-ui/react-dialog'
import type { ComponentProps } from 'react'
import { cn } from '@/lib/utils'

export const Sheet = RadixDialog.Root
export const SheetClose = RadixDialog.Close

/**
 * Mesma base do Dialog (foco preso, fecha com Esc), mas ancorado num canto em
 * vez de centralizado — é o painel de detalhe da sala (spec 009), que
 * responde "o que tem aqui" sem tirar a grade de cards de vista.
 */
export function SheetContent({ className, children, ...props }: ComponentProps<typeof RadixDialog.Content>) {
  return (
    <RadixDialog.Portal>
      <RadixDialog.Overlay className="fixed inset-0 z-40 bg-black/50" />
      <RadixDialog.Content
        className={cn(
          'fixed inset-y-0 right-0 z-50 flex w-[calc(100%-2rem)] max-w-sm flex-col',
          'border-l border-border bg-surface p-6 shadow-lg',
          'overflow-y-auto',
          className,
        )}
        {...props}
      >
        {children}
      </RadixDialog.Content>
    </RadixDialog.Portal>
  )
}

export function SheetTitle({ className, ...props }: ComponentProps<typeof RadixDialog.Title>) {
  return (
    <RadixDialog.Title
      className={cn('font-display text-lg font-bold tracking-tight', className)}
      {...props}
    />
  )
}

export function SheetDescription({ className, ...props }: ComponentProps<typeof RadixDialog.Description>) {
  return <RadixDialog.Description className={cn('text-sm text-muted', className)} {...props} />
}

import { cn } from '@/lib/utils'

/** Skeleton com a forma do conteudo final — nao spinner solto. */
export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('animate-pulse rounded-md bg-border', className)} />
}

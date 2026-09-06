import type { ReactNode } from 'react'

export function PageHeader({ title, action }: { title: string; action?: ReactNode }) {
  return (
    <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
      <h1 className="font-display text-xl font-bold tracking-tight sm:text-2xl">{title}</h1>
      {action}
    </div>
  )
}

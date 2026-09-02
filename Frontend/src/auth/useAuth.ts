import { use } from 'react'
import { AuthContext, type AuthContextValue } from '@/auth/auth-context'

export function useAuth(): AuthContextValue {
  const context = use(AuthContext)

  if (context === null) {
    throw new Error('useAuth precisa estar dentro de <AuthProvider>.')
  }

  return context
}

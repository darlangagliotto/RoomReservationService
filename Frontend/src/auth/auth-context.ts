import { createContext } from 'react'
import type { LoginRequest } from '@/api/auth'

export interface Session {
  token: string
  /** Lido do claim `email`. O token nao carrega nome nem papel. */
  email: string | null
}

export interface AuthContextValue {
  session: Session | null
  signIn: (credentials: LoginRequest) => Promise<void>
  signOut: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

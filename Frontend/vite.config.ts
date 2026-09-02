import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// Portas do backend: ver Infra/docker-compose.yml
const GATEWAY = 'http://localhost:5000'
const AUTH_SERVICE = 'http://localhost:5001'
const USER_SERVICE = 'http://localhost:5002'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy: {
      // TODO(B2): remover os dois desvios abaixo e deixar tudo no Gateway.
      // O Gateway aplica FallbackPolicy=RequireAuthenticatedUser tambem nas
      // rotas do YARP, entao /api/auth/login e POST /api/users exigem um token
      // que quem ainda nao autenticou nao tem.
      // Ver docs/sdd/backlog.md#b2
      '/api/auth': { target: AUTH_SERVICE, changeOrigin: true },
      '/api/users': { target: USER_SERVICE, changeOrigin: true },

      '/api': { target: GATEWAY, changeOrigin: true },
    },
  },
})

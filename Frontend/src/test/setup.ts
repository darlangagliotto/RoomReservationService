import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'

/*
  O cleanup automatico do Testing Library so se registra quando `globals: true`.
  Aqui os helpers do vitest sao importados explicitamente, entao a desmontagem
  entre testes precisa ser registrada a mao — sem isso, arvores de renders
  anteriores continuam no DOM e as queries encontram a tela errada.
*/
afterEach(cleanup)

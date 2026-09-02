# 001 — Login e sessão autenticada

| Campo | Valor |
|---|---|
| Status | Aprovada |
| Serviços afetados | Frontend (novo), AuthService (consumo, sem alteração) |
| Depende de | — |
| Autor / data | Darlan · 2026-09-02 |

## 1. Problema

Não existe frontend. Hoje a única forma de exercitar a API é pelo Swagger de cada
serviço, colando o JWT manualmente a cada chamada. Sem uma sessão autenticada no
navegador, nenhuma tela de negócio (salas, reservas) pode ser construída — toda rota da
API, exceto login e cadastro, exige `Authorization: Bearer`.

Esta spec entrega a **primeira fatia vertical** do frontend: o caminho completo de
autenticação. Ela existe tanto para liberar as telas seguintes quanto para validar, na
prática, as decisões de arquitetura registradas na skill `frontend-app` antes que
qualquer outra tela dependa delas.

## 2. Resultado esperado

Uma pessoa abre o app, informa e-mail e senha, entra, e permanece autenticada enquanto
navega e ao recarregar a aba. Se tentar acessar uma rota protegida sem sessão, é levada
ao login e devolvida ao destino original depois de autenticar. Quando o token expira,
a sessão é encerrada de forma previsível, sem tela quebrada. O logout é explícito.

## 3. Escopo

**Incluído**

- Projeto React + TypeScript em `Frontend/`, com a stack fixada na skill `frontend-app`
- Proxy de desenvolvimento no Vite (contorna CORS e o bloqueio B2 do Gateway)
- `api/client.ts` com o mapa completo de erros do backend
- `auth/token-store.ts` (memória + `sessionStorage`), `AuthProvider`, `RequireAuth`
- Tela de login com validação, estados de carregamento e erro
- `AppShell` responsivo com o nome do produto, identificação do usuário e ação de logout
- **Home** (`/`), rota autenticada de destino após o login, com conteúdo placeholder
- Testes dos comportamentos de sessão

**Fora de escopo**

- Cadastro de usuário (`POST /api/users`) — spec própria
- Qualquer tela de salas, equipamentos ou reservas
- Dockerfile do frontend e entrada no `docker-compose`
- CORS no Gateway e correção de B2 — necessários só para ambiente publicado
- Refresh token, "lembrar-me", recuperação de senha, tema escuro

## 4. Contrato

Nenhum endpoint novo. Consome o contrato existente do
[AuthService](../services/auth-service.md).

### `POST /api/auth/login` — anônimo

Request:
```json
{ "email": "joao@email.com", "password": "123456" }
```

`200 OK`:
```json
{ "token": "<jwt>", "expiresAt": "2026-09-02T18:30:00Z" }
```

Erros:

| Condição | Resposta | `detail` |
|---|---|---|
| Credencial inválida, usuário bloqueado ou inexistente | `400` `ProblemDetails` | `"Invalid email or password!"` |
| UserService indisponível | `400` `ProblemDetails` | `"Invalid email or password!"` |
| Falha inesperada | `500` `ProblemDetails` | genérico |

O backend **não distingue** credencial errada de indisponibilidade do UserService — o
erro remoto é logado e convertido em credencial inválida. A UI herda essa limitação e
não deve inventar uma mensagem de indisponibilidade que não pode comprovar.

### Roteamento

O Gateway hoje bloqueia `/api/auth/*` pela `FallbackPolicy`
([B2](../sdd/backlog.md#b2)). Em desenvolvimento, o proxy do Vite envia `/api/auth`
direto para `http://localhost:5001` e o restante de `/api` para o Gateway na `5000`,
com um `TODO(B2)` no `vite.config.ts` marcando a remoção do desvio.

O app sempre chama caminhos relativos (`/api/auth/login`); nenhum host aparece no código
da aplicação.

## 5. Regras de negócio

### Validação do formulário

| Campo | Regra no cliente |
|---|---|
| `email` | obrigatório; formato de e-mail |
| `password` | obrigatório |

**Exceção deliberada à convenção**: a skill `frontend-app` manda espelhar as invariantes
do domínio, e `RegisterUserRequestValidator` exige senha ≥ 6 caracteres. No **login**
essa regra não se aplica — validar comprimento aqui revela a política de senha a quem
não está autenticado e recusa localmente credenciais que o servidor talvez aceite. O
servidor é a autoridade sobre a senha; o cliente só verifica preenchimento.

### Sessão

- O token é a fonte de verdade da sessão; vale 60 minutos e **não há renovação**.
- `sessionStorage` guarda apenas o token, sob a chave `rrs.auth.token`. Nada mais é
  persistido. Não usar `localStorage`.
- Ao iniciar o app, o token guardado é lido e sua expiração (`exp`) verificada
  localmente: se já expirou, é descartado e a sessão não é restaurada — evita uma
  chamada inútil que retornaria `401`.
- Qualquer `401` de qualquer chamada encerra a sessão e leva ao login, preservando a
  rota de origem para retorno após autenticar.
- Logout limpa memória e `sessionStorage` e leva ao login. Não há revogação no servidor;
  a UI não deve afirmar que a sessão foi encerrada em outros dispositivos.

### Nomenclatura e destino

O produto chama-se **Room Reservation**. O nome aparece no `AppShell`, no `<title>` do
documento e na tela de login. `RoomReservationService` permanece como nome do
repositório e dos assemblies — não é usado na interface.

Após autenticar, o destino é a **Home** (`/`). Nesta spec a Home é um placeholder: um
estado vazio que nomeia o que virá ("Salas e reservas aparecerão aqui"), sem métricas,
gráficos ou cards inventados. O conteúdo real da Home será especificado quando as telas
de salas e reservas existirem e houver dado verdadeiro para mostrar.

### Identificação do usuário

O JWT carrega apenas `sub`, `email` e `jti` — **não há nome nem papel**, e
`GET /api/users/id/{id}` não existe ([B1](../sdd/backlog.md#b1)). O shell exibe o
**e-mail**, lido do claim `email` decodificado do próprio token.

A decodificação é **para exibição apenas**. Nenhuma decisão de autorização se apoia no
conteúdo lido no cliente — quem autoriza é o backend, a cada requisição. Não construir
UI condicional a perfil: a autorização do sistema é binária.

### Tratamento de resposta

O `client.ts` normaliza as quatro faixas descritas na skill `frontend-app` (negócio,
validação, `401`, `500`) e converte o `400` de "lista vazia" em coleção vazia. Nesta
spec só as faixas de negócio e `401` são exercitadas, mas o mapa completo é implementado
agora, porque as telas seguintes dependem dele.

## 6. Impacto em dados

Nenhuma alteração de banco, entidade ou migration.

Único dado persistido no cliente: `sessionStorage["rrs.auth.token"]`, apagado no logout,
na expiração e ao fechar a aba.

## 7. Impacto entre serviços

O frontend passa a chamar o AuthService, que já chama o UserService. Nenhum contrato de
backend muda; nenhuma ordem de implantação é imposta.

Indisponibilidade: com o AuthService fora do ar, o login falha com erro genérico; com o
UserService fora do ar, falha como credencial inválida (§4). Ambos os casos deixam a
pessoa na tela de login com mensagem — nunca em tela branca.

Para ambiente publicado (fora de escopo aqui), duas pendências de backend precisam estar
resolvidas: CORS no Gateway e o desbloqueio de `/api/auth/*` (B2).

## 8. Critérios de aceite

**Autenticação**

- [ ] Dado credencial válida, quando submeto o login, então sou levado à Home (`/`) e o meu e-mail aparece no shell.
- [ ] Dado credencial inválida, quando submeto, então vejo `"Invalid email or password!"` junto ao formulário, o campo de senha é limpo e permaneço no login.
- [ ] Dado o AuthService indisponível, quando submeto, então vejo mensagem de erro e opção de tentar novamente, sem tela quebrada.
- [ ] Dado e-mail em formato inválido, quando submeto, então vejo o erro no campo e nenhuma requisição é enviada.
- [ ] Dado que digito uma senha de 3 caracteres, quando submeto, então a requisição **é** enviada (o cliente não valida comprimento).

**Sessão**

- [ ] Dado que não tenho sessão, quando acesso `/` direto pela URL, então sou levado ao login e, após autenticar, chego na Home.
- [ ] Dado que recarrego a aba autenticado, então permaneço autenticado.
- [ ] Dado que fecho a aba e reabro, então **não** permaneço autenticado.
- [ ] Dado um token expirado em `sessionStorage`, quando abro o app, então inicio deslogado e nenhuma requisição à API é feita.
- [ ] Dado token expirado durante o uso, quando faço qualquer chamada, então a sessão é encerrada e vou para o login.
- [ ] Dado que faço logout, então o token some da memória e do `sessionStorage`, e voltar pelo histórico não restaura a sessão.

**Interface**

- [ ] Em 375px, login e shell não produzem scroll horizontal; a navegação do shell está recolhida.
- [ ] Todo o fluxo (login, navegação, logout) é percorrível apenas com teclado, com foco visível.
- [ ] Durante o envio do login, o botão indica carregamento e não permite envio duplicado.
- [ ] Nenhum token, senha ou corpo de requisição de login aparece no console.

**Qualidade**

- [ ] `npx tsc --noEmit` limpo, sem `any`.
- [ ] `npm run build` sem erro nem warning novo.
- [ ] Testes cobrindo: restauração de sessão, expiração local, `401` durante o uso e logout.

## 9. Decisões em aberto

Nenhuma. As duas pendências foram resolvidas e incorporadas ao §5:

1. Nome do produto: **Room Reservation**.
2. Destino após o login: **Home** (`/`), com placeholder honesto nesta entrega.

> A direção visual (paleta, tipografia, layout) não é decisão em aberto desta spec: será
> definida na implementação pela skill `frontend-design`, dentro do contrato de tokens
> semânticos e do piso de acessibilidade estabelecidos pela skill `frontend-app`.

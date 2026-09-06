# 005 — Navegação e telas de Salas

| Campo | Valor |
|---|---|
| Status | Implementada |
| Serviços afetados | Frontend |
| Depende de | — (usa endpoints que já existiam; `planSlot` vem da [004](004-catalogo-de-equipamentos-e-planta.md)) |
| Autor / data | Darlan · 2026-09-06 |

## 1. Problema

O frontend tem login e uma Home vazia. Não existe navegação nem nenhuma tela que mostre ou
edite dado de negócio — o sistema é inoperável pela interface.

Salas é a primeira tela possível: `GET`, `POST` e `PATCH /api/rooms` já existem e foram
verificados.

## 2. Resultado esperado

Depois de entrar, a pessoa navega para Salas, vê as salas cadastradas com seus
equipamentos, cadastra uma sala nova e edita nome, número e posição na planta.

## 3. Escopo

**Incluído**

- Barra de navegação no topo do `AppShell`, com Início e Salas
- `/salas` — listagem: tabela acima de 768px, cartões abaixo
- Cadastro de sala, com seleção de equipamentos livres e `planSlot`
- Edição de nome, número e `planSlot`
- Os quatro estados de tela (carregando, vazio, erro, sucesso)

**Fora de escopo**

- Equipamentos e Reservas como seções próprias — spec 006
- A planta — spec 007
- Remover sala, ou alterar equipamentos de uma sala existente: **a API não expõe**

## 4. Contrato

Nenhum endpoint novo. Consome, pelo Gateway:

| Chamada | Uso |
|---|---|
| `GET /api/rooms` | listagem — `400` `"No rooms found."` vira `[]` pelo `requestList` |
| `POST /api/rooms` | cadastro |
| `PATCH /api/rooms/{id}` | edição |
| `GET /api/equipments?unassigned=true` | equipamentos livres para alocar |

## 5. Regras de negócio

Validação no cliente espelha as invariantes de
[domain/model.md](../domain/model.md#room--raiz-roomservice): nome ≥ 3 caracteres, número
> 0, `planSlot` entre 1 e 10 quando informado. **O servidor continua sendo a autoridade** —
`"Room is already registered."` e `"Plan slot is already taken."` só ele sabe, e a
mensagem dele é exibida como está.

Na edição, ao menos um campo deve mudar (o backend recusa com
`"Provide at least one field to update!"`).

Equipamento só pode estar em uma sala: o seletor oferece apenas `unassigned=true`.

## 6. Impacto em dados

Nenhum. Frontend puro.

## 7. Impacto entre serviços

Nenhum. Rotas já cobertas pelo Gateway.

## 8. Critérios de aceite

> 8 testes de componente com `fetch` dublado + verificação na pilha em Docker
> (lista real, navegação, 375px sem scroll horizontal).

- [x] Dado que estou autenticado, então vejo Início e Salas na barra do topo, e a seção atual está indicada.
- [x] Dado nenhuma sala cadastrada, quando abro Salas, então vejo estado vazio com ação de cadastrar — **não** mensagem de erro.
- [x] Dado salas cadastradas, então vejo nome, número, posição na planta e equipamentos de cada uma.
- [x] Dado que cadastro uma sala válida, então ela aparece na lista sem eu recarregar a página.
- [x] Dado nome com 2 caracteres, então vejo erro no campo e nenhuma requisição é enviada.
- [x] Dado um número já usado, então vejo a mensagem do backend junto ao formulário. — mesmo caminho de erro exercitado pelo teste de `planSlot` ocupado.
- [x] Dado um `planSlot` já ocupado, então vejo `"Plan slot is already taken."`.
- [ ] Dado que edito o nome de uma sala, então a lista reflete a mudança. — **não coberto por teste**: o fluxo de edição existe e usa o mesmo caminho do cadastro, mas só foi exercitado manualmente.
- [x] Dado que a API demora, então vejo skeleton com a forma do conteúdo, não spinner solto.
- [x] Dado que a API falha com `500`, então vejo estado de erro com opção de repetir.
- [x] Em 375px não há scroll horizontal, e a tabela vira cartões.
- [ ] Navegação e formulários percorríveis só com teclado, com foco visível. — **não verificado** nesta entrega; o piso de foco visível está no `globals.css` desde a spec 001.
- [x] `npx tsc --noEmit` limpo e `npm test` passando.

## 9. Decisões em aberto

Nenhuma.

# 006 — Cadastro de equipamentos

| Campo | Valor |
|---|---|
| Status | Rascunho |
| Serviços afetados | Frontend (RoomService consumido, sem alteração) |
| Depende de | [004](004-catalogo-de-equipamentos-e-planta.md) (vocabulário e âncoras), [005](005-navegacao-e-salas.md) (barra de navegação) |
| Autor / data | Darlan · 2026-09-06 |

## 1. Problema

O vocabulário de equipamentos existe desde a spec 004 e `GET`/`POST /api/equipments` estão
prontos e roteados pelo Gateway, mas **não há tela**. Hoje o único jeito de cadastrar um
projetor é pelo Swagger do RoomService, montando o JSON à mão.

Isso trava duas coisas a jusante: a spec 005 já oferece um seletor de equipamentos livres
no cadastro de sala, e ele está sempre vazio para quem usa só a interface; e a planta
(spec 009) não tem o que desenhar sem patrimônio cadastrado.

## 2. Resultado esperado

Uma pessoa autenticada abre **Equipamentos** na barra do topo, vê o patrimônio cadastrado —
tipo, marca, número de série, data de compra e em qual sala está —, filtra por tipo ou por
"apenas livres", e cadastra um equipamento novo escolhendo o tipo de uma lista fechada.

## 3. Escopo

**Incluído**

- Item **Equipamentos** na barra de navegação do `AppShell`, apontando para `/equipamentos`
- `/equipamentos` — listagem: tabela acima de 768px, cartões abaixo (mesmo padrão da 005)
- Filtros por `type` e por `unassigned=true`
- Formulário de cadastro: tipo (seleção do vocabulário), marca, número de série, data de compra
- Exibição da **âncora** (`placement`) derivada do tipo, como informação, nunca como campo editável
- Coluna/linha indicando a sala em que o equipamento está, ou "Livre"
- Os quatro estados de tela: carregando, vazio, erro, sucesso

**Fora de escopo**

- **Atribuir equipamento a uma sala** — spec [007](007-atribuir-equipamentos-as-salas.md).
  Esta tela **mostra** a alocação; não a altera.
- Editar ou remover equipamento — a API não expõe (ver
  [room-service.md](../services/room-service.md#lacunas-conhecidas)). Não inventar o botão.
- Upload de foto, patrimônio contábil, manutenção, garantia
- Reservas — spec 008

## 4. Contrato

Nenhum endpoint novo. Consome, pelo Gateway (rota `equipments-route` criada na spec 004):

| Chamada | Uso |
|---|---|
| `GET /api/equipments` | listagem completa |
| `GET /api/equipments?type=<tipo>` | filtro por tipo |
| `GET /api/equipments?unassigned=true` | só os não alocados |
| `POST /api/equipments` | cadastro |

### `GET /api/equipments` — autenticado

`200 OK`:
```json
[{ "id": "…", "type": "Projetor", "placement": "Teto", "brand": "Epson",
   "serialNumber": "SN-123", "purchaseDate": "2024-01-10T00:00:00Z", "roomId": null }]
```

**Lista vazia devolve `200` com `[]`**, não o `400` do [ADR-011](../architecture/decisions.md).
Este endpoint é a exceção deliberada da spec 004. Consequência prática para o cliente: usar
`request<Equipment[]>` e **não** `requestList` — não há mensagem de vazio para interpretar.
Ausência de equipamento é estado vazio da tela, nunca erro.

### `POST /api/equipments` — autenticado

Request:
```json
{ "type": "Projetor", "brand": "Epson", "serialNumber": "SN-123", "purchaseDate": "2024-01-10" }
```

`201 Created` com `{ "equipment": { … } }`.

Erros (`400` `ProblemDetails`, `title: "Erro de negócio"`):

| Condição | `detail` |
|---|---|
| Tipo fora do vocabulário | `"Tipo de equipamento desconhecido. Valores aceitos: …"` |
| Número de série já cadastrado | `"Este equipamento já está cadastrado."` |
| Invariante da entidade | mensagem da `DomainException` |

Roteamento no Gateway: já coberto.

## 5. Regras de negócio

Nenhuma regra nova. A validação do cliente **espelha** as invariantes de
[domain/model.md](../domain/model.md#equipment--raiz-roomservice) e o servidor continua
sendo a autoridade — a mensagem dele é exibida como está, sem tradução no cliente.

| Campo | Regra no cliente |
|---|---|
| `type` | obrigatório; seleção fechada, um dos 11 valores de `EquipmentType` |
| `brand` | obrigatório |
| `serialNumber` | obrigatório |
| `purchaseDate` | obrigatória; não pode ser futura |

**O tipo é uma seleção, não um campo de texto.** O backend aceita `"projetor"` sem
diferenciar caixa, mas oferecer texto livre reintroduz exatamente o problema que a spec 004
resolveu — patrimônio digitado de onze jeitos diferentes, âncora indefinida, planta sem
marcador. A lista de tipos é fixa no cliente e deve ser mantida em sincronia com o enum do
domínio; qualquer divergência é recusada pelo servidor com a mensagem que lista os valores
aceitos.

**`placement` é derivado, nunca informado.** Vem do tipo, calculado no domínio
(`EquipmentPlacements.For`). Aparece na tela como leitura para explicar por que o marcador
vai ficar na parede ou no teto quando a planta existir.

**Data de compra**: o backend normaliza para `DateTimeKind.Utc` desde a spec 004. O cliente
envia data pura (`"2024-01-10"`); não inventar hora nem fuso.

## 6. Impacto em dados

Nenhum. Frontend puro — nenhuma entidade, coluna ou migration.

## 7. Impacto entre serviços

Nenhum. O frontend passa a chamar `/api/equipments` pelo Gateway, rota já existente e
autenticada. Sem ordem de implantação imposta.

Com o RoomService indisponível, a tela mostra estado de erro com opção de repetir — nunca
tela branca, nunca lista vazia mentindo que não há patrimônio.

## 8. Critérios de aceite

- [ ] Dado que estou autenticado, então **Equipamentos** aparece na barra do topo e a seção atual está indicada.
- [ ] Dado nenhum equipamento cadastrado, quando abro Equipamentos, então vejo estado vazio com ação de cadastrar — **não** mensagem de erro.
- [ ] Dado equipamentos cadastrados, então vejo tipo, âncora, marca, número de série, data de compra e a sala (ou "Livre") de cada um.
- [ ] Dado que filtro por um tipo, então vejo apenas equipamentos daquele tipo, e o filtro ativo está visível.
- [ ] Dado que marco "apenas livres", então vejo apenas os equipamentos sem sala.
- [ ] Dado que cadastro um equipamento válido, então ele aparece na lista sem eu recarregar a página.
- [ ] Dado que abro a seleção de tipo, então vejo exatamente os 11 valores do vocabulário e nenhum campo de texto livre.
- [ ] Dado um número de série já usado, então vejo `"Este equipamento já está cadastrado."` junto ao formulário e o restante do que digitei é preservado.
- [ ] Dado data de compra no futuro, então vejo erro no campo e nenhuma requisição é enviada.
- [ ] Dado que a API demora, então vejo skeleton com a forma do conteúdo, não spinner solto.
- [ ] Dado que a API falha com `500`, então vejo estado de erro com opção de repetir.
- [ ] Em 375px não há scroll horizontal, e a tabela vira cartões.
- [ ] Todo o fluxo é percorrível apenas com teclado, com foco visível.
- [ ] `npx tsc --noEmit` limpo, sem `any`, e `npm test` passando.

## 9. Decisões em aberto

Nenhuma.

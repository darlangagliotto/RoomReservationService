# Arquitetura de informação

Mapa do produto: que telas existem, o que cada uma resolve, como se navega entre elas e
**o que cada uma exige do backend que ainda não existe**. Não é spec — não gera código.
É a referência que as specs de tela consultam para não inventar navegação por acidente.

## Para quem, e para quê

Funcionário de escritório que precisa de uma sala para uma reunião. O trabalho central do
produto é **achar uma sala livre no horário que eu quero e reservá-la**. Todo o resto —
cadastro de salas, de equipamentos, gestão das minhas reservas — existe para sustentar
esse ato.

Consequência de projeto: a tela inicial não é um painel de métricas. É a própria busca.

## Decisões tomadas

| Decisão | Razão |
|---|---|
| **Grade de cards de sala, cada um com imagem ilustrativa individual** | Substitui a ideia original de planta única do andar (ver "Histórico da decisão visual"); cresce com o cadastro sem depender de uma arte redesenhada a cada sala nova |
| **Acabamento estilizado, nem esquemático nem fotorrealista** | Ilustração top-down gerada por IA, no tom do tema (petróleo escuro + acento âmbar) — ver "Tratamento visual" |
| **Três estados**: disponível · reservada · em uso | Sala livre agora mas reservada logo não serve para uma reunião longa |
| **Status é derivado das reservas**, nunca um campo | Uma sala não "é" ocupada; ela está ocupada num intervalo |
| **Referência de tempo fixa em "agora + 30 minutos"** nesta versão | Sem slider ainda (ver "Fora de escopo" da spec 009); simples e cobre o caso comum |
| **Navegação no topo**, não lateral | 4 seções não justificam menu lateral |
| **Derivar, não pedir** | Posição pedida no cadastro vira campo vazio em produção. Ver abaixo |

## Tratamento visual

Cada sala é um **card** com uma imagem de fundo ilustrativa, vista de cima (top-down),
estilo semi-flat com sombra suave — nem traço esquemático nem render fotorrealista. As
imagens vêm de um conjunto pequeno e fixo (3 variações, geradas uma vez por IA de imagem),
distribuídas entre as salas cadastradas; **não são fotos reais de cada sala** e não mudam
conforme o equipamento cadastrado nela — equipamento é informação do painel de detalhe,
não da imagem.

| Camada | O que é | Origem |
|---|---|---|
| **Imagem do card** | Uma de 3 ilustrações top-down fixas, distribuída por sala de forma determinística (mesma sala sempre mostra a mesma imagem) | Geradas uma vez, fora do sistema; arquivos em `Frontend/public/rooms/` |
| **Estado** | Bolinha colorida + texto, sobreposta no card | Calculado pela API de disponibilidade (spec 003) |
| **Detalhe** | Painel lateral ao clicar no card: nome, número, status, equipamentos, próximo horário, botão de reserva | Dados do cadastro + disponibilidade |

O estado **nunca** é uma lavagem de cor sobre a imagem inteira — é uma bolinha (`●`) mais
texto, nunca só cor: `--color-success` (verde-petróleo) para Disponível, `--color-accent`
(âmbar) para Reservada, `--color-muted` (cinza) para Em uso — o mesmo mapeamento que os
tokens de `globals.css` já documentam.

Como a planta deixou de ser um mapa único, o modelo **não precisa** de `PlanSlot`,
polígono nem largura/profundidade de sala para esta tela. `Room.PlanSlot` continua existindo
no schema (spec 004) mas fica sem uso funcional aqui — não é removido, só não é mais o
mecanismo de posicionamento visual.

### Histórico da decisão visual

Duas voltas antes de chegar aqui, registradas para quem for mexer nisso depois:

1. Primeira decisão: planta única do andar, um render realista visto de cima, com um
   polígono (`Room.PlanSlot`) por sala sobre essa base. Recusada na prática porque um
   render de andar inteiro não escala — geradores de imagem não garantem um layout com o
   número exato de salas do cadastro, e adicionar uma sala nova exigiria regerar a base
   inteira.
2. Tentativa de estilo 100% vetorial (SVG desenhado em código, flat ou pseudo-3D) para não
   depender de gerar imagem nenhuma — descartada por ficar simples demais para o padrão
   visual desejado.
3. **Decisão atual**: cada sala é um card independente com uma imagem ilustrativa (de um
   pool pequeno, não uma por sala), o que cresce naturalmente com o cadastro e não depende
   de redesenhar nada quando uma sala é criada.

### Derivar, não pedir

Princípio que governa a posição dos objetos:

- **Posição do objeto na sala** é derivada do **tipo**, via âncora semântica:

| Âncora | Tipos | Desenho |
|---|---|---|
| `parede` | TV, monitor, tela | encostado numa parede |
| `teto` | projetor, ar-condicionado | marcado no centro |
| `mesa` | notebook, telefone, dock | sobre a mesa central |
| `piso` | flipchart, cadeira extra | canto livre |

Em ambos os casos o modelo guarda um campo de sobrescrita **nulo**. Precisão vira aditiva
depois, sem migração dolorosa — e provavelmente ninguém vai precisar. **Não** construir
editor de arrastar-e-soltar antes de alguém provar que a posição exata importa.

## Navegação

Barra no topo, dentro do `AppShell` que já existe. Quatro destinos:

```
Room Reservation     Planta   Reservas   Salas   Equipamentos     usuario@email  Sair
```

| Rota | Seção | Resolve |
|---|---|---|
| `/` | **Planta** | achar sala livre e reservar (Home) |
| `/salas/:id` | — (detalhe) | avaliar uma sala e ver sua agenda |
| `/reservas` | **Reservas** | ver e cancelar o que reservei |
| `/salas` | **Salas** | cadastrar e editar salas |
| `/equipamentos` | **Equipamentos** | cadastrar equipamentos |

A barra lista apenas destinos que existem. Hoje: Início, Salas (spec 005), Equipamentos
(spec 006) e Reservas (spec 008); a Planta entra com a spec 009. Em telas estreitas a barra
quebra para a linha de baixo em vez de colapsar num menu — com poucos itens, um hambúrguer
esconderia mais do que ajudaria.

> **Sem papéis.** O JWT não carrega papel e a autorização do sistema é binária: qualquer
> pessoa autenticada cadastra e edita salas e equipamentos. Isso é consequência do backend
> atual, não escolha de produto — e deve virar decisão consciente antes de qualquer uso
> real.

## As telas

### Planta do andar — `/` (Home)

Grade de cards, um por sala cadastrada — não mais um mapa único do andar (ver "Histórico
da decisão visual"). Cada card tem: imagem ilustrativa de fundo, número da sala, nome, e
uma bolinha de estado com texto no canto ("● Livre", "● Reservada", "● Em uso").

- **Estado** por bolinha colorida **mais texto**, nunca lavagem de cor sobre a imagem.
- **Clicar** no card abre painel lateral com equipamentos, status atual e próximo
  horário livre/ocupado, e um botão para reservar aquela sala com o horário pré-preenchido.
- A grade já é responsiva por natureza dos cards — não precisa de uma apresentação
  separada abaixo de 768px como as telas de tabela.
- Sem linha do tempo arrastável nesta versão: a referência é fixa em "agora, próximos 30
  minutos" (ver spec 009, Fora de escopo). Mover essa referência fica para quando fizer
  falta de verdade.

### Detalhe da sala — `/salas/:id`

A sala em tamanho grande, com todos os equipamentos e a agenda do dia. É a tela de decidir
("essa serve?") e de reservar com intervalo preciso.

### Nova reserva — painel sobre a Planta ou o Detalhe

Não é rota própria: nasce do contexto em que a pessoa já está, com sala e horário
pré-preenchidos pelo que ela estava olhando. Datas convertidas para UTC na saída.

### Reservas — `/reservas`

Lista das minhas reservas, com cancelamento. Sem endpoint de "minhas reservas", filtra-se
por `userId` — lido do claim `sub` do token.

### Salas — `/salas` · Equipamentos — `/equipamentos`

Listagem e cadastro. Telas de dados densos: tabela acima de 768px, cartões abaixo. É onde
o `400` de lista vazia precisa virar estado vazio, não erro.

## O que cada tela exige do backend

A coluna da direita é o valor deste documento: nenhuma tela deve ser especificada sem
saber o que falta abaixo dela.

| Tela | Backend | Situação |
|---|---|---|
| Planta | `GET /api/reservations/availability`, `GET /api/rooms` | ✅ pronto e **em uso** (spec 009) |
| Detalhe da sala | `GET /api/rooms/{id}`, `GET /api/reservations?roomId=` | ✅ pronto |
| Nova reserva | `POST /api/reservations` | ✅ pronto e **em uso** (spec 008) |
| Reservas | `GET`, `DELETE /api/reservations`, `GET /api/users/{id}` | ✅ pronto e **em uso** (spec 008) |
| Equipamentos de uma sala | `POST`/`DELETE /api/rooms/{id}/equipments` | ✅ pronto e **em uso** (spec 007, dentro da edição de sala) |
| Salas | `GET`, `POST`, `PATCH /api/rooms` | ✅ pronto e **em uso** (spec 005) |
| Equipamentos | `GET`, `POST /api/equipments` | ✅ pronto |

**O backend deixou de ser o gargalo.** As lacunas estruturais que este documento levantou
foram fechadas pelas specs 002, 003, 004 e 007: consulta por id com propagação de token,
disponibilidade por intervalo com os três estados, vocabulário de equipamento com âncoras
mais `PlanSlot`, e alocação/desalocação de equipamento numa sala já existente.

O que trava a planta agora é **produção de arte**, não código — ver "Pendências de
produção" abaixo.

## Sequência de specs

| # | Spec | Bloco | Depende | Estado |
|---|---|---|---|---|
| 002 | Consulta por id e autenticação serviço-a-serviço | backend | — | ✅ implementada |
| 003 | Disponibilidade por intervalo, com três estados | backend | 002 | ✅ implementada |
| 004 | Catálogo de equipamentos e vínculo com a planta | backend | — | ✅ implementada |
| 005 | **Navegação no topo** + Salas: lista, cadastro, edição | frontend | — | implementada |
| 006 | Cadastro de equipamentos | frontend | 004, 005 | implementada |
| 007 | Atribuir equipamentos às salas | **backend** + frontend | 005, 006 | implementada |
| 008 | Reservar salas: criar, listar, cancelar | frontend | 002, 005 | implementada |
| 009 | Planta do andar: grade de cards com estado e painel de detalhe | frontend | 003, 004, 008 | implementada |
| 010 | Detalhe da sala em rota própria | frontend | 004, 009 | a escrever |

**A 005 não depende de backend nenhum.** `GET`, `POST` e `PATCH /api/rooms` já existem e
funcionam; o `planSlot` da spec 004 entra depois como campo adicional. Ou seja: a barra de
navegação e a primeira tela de dados podem ser construídas **em paralelo** com o bloco de
backend, não depois dele.

A navegação nasce junto da 005 porque é ali que existe o primeiro destino real. Barra de
menu antes disso apontaria para lugar nenhum.

## Pendências de produção

Itens que não são código e têm prazo de entrega próprio. A spec 009 não começa sem eles.

| Item | Quem | Situação |
|---|---|---|
| Render do andar visto de cima, realista, planta fixa, até 10 salas | Fora do sistema — ferramenta 3D ou arte encomendada | **pendente, caminho crítico** |
| Marcadores dos tipos de equipamento (11 tipos da spec 004) | idem | pendente |
| ~~Levantamento do que dá para obter pronto na internet~~ | Claude, a pedido | ✅ entregue — [planta-baixa-materiais.md](planta-baixa-materiais.md) |

O levantamento comparou três caminhos para o render (IA de imagem, ferramenta 3D com
export, encomenda) e mapeou a cobertura de bibliotecas de ícones livres para os 11 tipos
de equipamento. Falta a decisão de qual caminho seguir para o render e a produção em si —
isso continua fora do sistema.

Duas observações sobre a ordem:

- A **005 é o seguro**: entrega o sistema funcionando — cadastrar, listar, reservar — sem
  depender da planta. Se a planta atrasar ou mudar de forma, ainda há produto.
- A **004 é independente da 002 e 003** e pode correr em paralelo.

## Adiado conscientemente

Múltiplos andares · editor de posição por arrastar · capacidade da sala · papéis e
permissões · reserva recorrente · convidados numa reserva.

Cada um destes é conceito de domínio que não existe. Nenhum deve ser introduzido de
improviso dentro de uma spec de tela — precisa de decisão própria.

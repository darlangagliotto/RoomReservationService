# Specs

Uma spec por unidade entregável, nomeada `NNN-nome-curto.md` com numeração sequencial.
Use [../sdd/spec-template.md](../sdd/spec-template.md) e siga o ciclo descrito em
[../sdd/workflow.md](../sdd/workflow.md).

Candidatos priorizados estão em [../sdd/backlog.md](../sdd/backlog.md).

| # | Spec | Status | Serviços |
|---|---|---|---|
| 001 | [Login e sessão autenticada](001-login-e-sessao.md) | Implementada | Frontend, AuthService (consumo) |
| 002 | [Consulta por id e autenticação serviço-a-serviço](002-consulta-por-id-e-auth-servico.md) | Em verificação | User, Room, Reservation |
| 003 | [Disponibilidade de salas por intervalo](003-disponibilidade-de-salas.md) | Rascunho | Reservation, Room |
| 004 | [Catálogo de equipamentos e vínculo com a planta](004-catalogo-de-equipamentos-e-planta.md) | Rascunho | Room, Gateway |

Ordem e dependências entre specs: [product/information-architecture.md](../product/information-architecture.md#sequência-de-specs).

Ao criar uma spec, adicione a linha nesta tabela; ao concluí-la, marque `Implementada`
e remova o item correspondente do backlog.

# Documentação — RoomReservationService

Documentação de arquitetura obtida por engenharia reversa do código em `main`
(commit `cb6840f`). Tudo aqui descreve **o que existe hoje**; lacunas e defeitos
estão isolados em [`sdd/backlog.md`](sdd/backlog.md) e nas seções "Lacunas conhecidas"
de cada serviço, sempre com o arquivo que comprova a afirmação.

## Mapa

```
docs/
├── architecture/
│   ├── overview.md        Componentes, portas, fluxos ponta a ponta, matriz de dependências
│   ├── conventions.md     Camadas, padrão de UseCase, Result<T>, entidades, DI, controllers, testes
│   ├── cross-cutting.md   JWT, erros, health, EF/Npgsql, configuração, Docker
│   └── decisions.md       ADRs (o porquê de cada escolha estrutural)
├── product/
│   └── information-architecture.md  Telas, navegação e o que cada uma exige do backend
├── domain/
│   └── model.md           Linguagem ubíqua, agregados, invariantes, schema físico por banco
├── services/
│   ├── gateway.md         API Gateway (YARP)
│   ├── auth-service.md    Emissão de JWT
│   ├── user-service.md    Cadastro e validação de credenciais
│   ├── room-service.md    Salas e equipamentos
│   └── reservation-service.md  Reservas
├── sdd/
│   ├── workflow.md        Como conduzir uma feature (spec → plano → código → verificação)
│   ├── spec-template.md   Template de especificação
│   └── backlog.md         Lacunas e defeitos priorizados, com evidência
└── specs/                 Specs de features (uma por arquivo)
```

## Convenções da documentação

- **Sem redundância**: cada fato mora em um único arquivo. Regras de negócio só em
  `domain/model.md`; contratos HTTP só no arquivo do serviço; configuração só em
  `architecture/cross-cutting.md`.
- **Sem suposição**: afirmações sobre comportamento citam o arquivo de origem.
  Comportamento não verificado é marcado explicitamente como hipótese.
- **Português** para a prosa; **inglês** para identificadores, rotas e mensagens de
  erro (o código foi padronizado em inglês no commit `5d835f5`).

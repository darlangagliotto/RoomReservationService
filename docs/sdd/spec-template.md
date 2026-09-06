# NNN — <Título da feature>

| Campo | Valor |
|---|---|
| Status | Rascunho \| Aprovada \| Implementada |
| Serviços afetados | ex.: UserService, ReservationService |
| Depende de | ex.: spec 001, ou "—" |
| Autor / data | |

## 1. Problema

O que não funciona ou não existe hoje, e para quem isso importa. Uma a três frases,
factual. Cite o arquivo/comportamento atual quando for correção.

## 2. Resultado esperado

O que passa a ser possível quando esta spec estiver implementada. Escreva do ponto de
vista de quem consome a API.

## 3. Escopo

**Incluído**
- …

**Fora de escopo**
- … (o que alguém razoavelmente esperaria e que esta spec deliberadamente não faz)

## 4. Contrato

Para cada endpoint novo ou alterado:

### `<MÉTODO> /api/<rota>` — <anônimo | autenticado>

Request:
```json
{ }
```

`200/201`:
```json
{ }
```

Erros (`400` `ProblemDetails`, `title: "Erro de negócio"`):

| Condição | `detail` |
|---|---|
| | |

Roteamento no Gateway: <rota já coberta por `/api/x/*` | precisa de rota nova>.

## 5. Regras de negócio

Invariantes (na entidade) e regras de aplicação (no caso de uso), separadas. Se a regra
já existe, referencie `docs/domain/model.md` em vez de reescrever.

## 6. Impacto em dados

Entidades e colunas afetadas; migration necessária (nome sugerido); índices; efeito
sobre dados existentes; reversibilidade.

## 7. Impacto entre serviços

Quem passa a chamar quem, ordem de implantação, e o que acontece se o serviço a jusante
estiver indisponível.

## 8. Critérios de aceite

Verificáveis, um por linha, no formato "dado / quando / então".

- [ ] Dado …, quando …, então …
- [ ] …

## 9. Decisões em aberto

Perguntas que precisam de resposta antes da implementação. Spec com item aberto não vai
para "Aprovada".

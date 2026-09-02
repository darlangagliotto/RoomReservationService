# Workflow SDD (Spec-Driven Development)

Como uma mudança nasce e termina neste repositório. O ciclo é curto de propósito: a
spec existe para tornar a implementação mecânica, não para virar documento de projeto.

## Ciclo

```
1. Spec        docs/specs/NNN-nome-curto.md  (spec-template.md)
2. Revisão     o autor humano aprova a spec antes de qualquer código
3. Plano       lista de arquivos a criar/alterar, derivada da spec
4. Implementação  um serviço por vez, seguindo conventions.md
5. Verificação    critérios de aceite da spec, executados
6. Documentação   atualizar os arquivos afetados em docs/
```

## 1. Spec

Um arquivo por unidade entregável em `docs/specs/`, numerado sequencialmente
(`001-user-get-by-id.md`). A spec responde **o que** e **por quê**, e fixa contratos;
não descreve implementação linha a linha.

Regra de escopo: se a spec toca mais de um serviço, ela lista explicitamente o contrato
entre eles e a ordem de implantação (provedor antes do consumidor).

## 2. Revisão

Spec aprovada = contrato congelado. Mudança de contrato durante a implementação volta
para a spec — não é decidida no código.

## 3. Plano

Antes de escrever código, produza a lista de arquivos por camada. Para um endpoint novo
em um serviço existente, o conjunto típico é:

```
Domain/Repositories/I<Entity>Repository.cs        (se precisar de nova consulta)
Application/UseCases/<Caso>/I<Caso>UseCase.cs
Application/UseCases/<Caso>/<Caso>Request.cs
Application/UseCases/<Caso>/<Caso>Response.cs
Application/UseCases/<Caso>/<Caso>UseCase.cs
Application/DependencyInjection/DependencyInjection.cs   (registrar o caso de uso)
Infrastructure/Repositories/<Entity>Repository.cs        (implementar a consulta)
Api/Controllers/<Entity>Controller.cs                    (nova action)
tests/<Service>.UnitTests/...                            (casos de aceite)
```

Mudança de entidade ou de mapeamento → **sempre** uma migration nova; nunca editar uma
migration já aplicada.

## 4. Implementação

- Siga [conventions.md](../architecture/conventions.md) literalmente.
- Não introduza biblioteca nova sem ADR.
- Não compartilhe código entre serviços (ADR-003).
- Divergir de uma convenção exige ADR novo em
  [decisions.md](../architecture/decisions.md) na mesma entrega.

## 5. Verificação

Definition of Done:

- [ ] Todos os critérios de aceite da spec verificados (teste automatizado quando o
      comportamento for de domínio ou de caso de uso).
- [ ] `dotnet build RoomReservationService.sln` sem erros.
- [ ] `dotnet test` do(s) serviço(s) tocado(s) passando.
- [ ] Se a mudança atravessa serviços: fluxo exercitado com `docker compose up --build`.
- [ ] Erros de negócio retornam `400` com `ProblemDetails` e mensagem em inglês.
- [ ] Rotas novas conferidas quanto a autenticação (público vs. autenticado) **e**
      quanto ao roteamento no Gateway.
- [ ] Migration gerada e aplicando sobre banco existente.

## 6. Documentação

| Mudou | Atualize |
|---|---|
| Endpoint, contrato ou erro | `docs/services/<serviço>.md` |
| Invariante, entidade ou schema | `docs/domain/model.md` |
| Auth, config, erro, EF, Docker | `docs/architecture/cross-cutting.md` |
| Padrão de código | `docs/architecture/conventions.md` |
| Escolha estrutural | `docs/architecture/decisions.md` (ADR novo) |
| Item entregue | remova de `docs/sdd/backlog.md` |

Uma informação, um lugar. Se estiver escrevendo algo que já existe em outro arquivo,
crie um link em vez de repetir.

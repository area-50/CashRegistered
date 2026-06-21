# Domain Layer - Backend

The core of the system, containing business entities and their logic.

## Diretrizes de Arquitetura (CRÍTICO)

1. **Uso de Repositórios:** Cada Use Case só pode injetar e utilizar o repositório que pertence ao seu próprio domínio.
2. **Proibição de Repositórios Cruzados:** É estritamente proibido injetar ou chamar um repositório de outro domínio dentro de um Use Case.
3. **Comunicação entre Domínios:** Toda comunicação ou obtenção de dados de outro domínio deve ser realizada exclusivamente através da injeção da **interface do Use Case** correspondente (ex: `IWarehouseUseCase` em vez de `IWarehouseRepository`). Esta regra deve ser seguida à risca em todos os momentos do desenvolvimento.

## Responsibilities
- **Entities:** Rich domain models (e.g., `User`, `CashFlow`, `Expense`).
- **Enums:** Domain-specific enumerations (e.g., `UserRole`, `Gender`).
- **Value Objects:** Simple types without identity (e.g., `Address`).
- **Interfaces:** Definitions for repositories and services (e.g., `ICashFlowRepository`).
- **Validations:** Business rules and domain constraints.

## Guidelines for Gemini
- Domain entities should be self-validating (e.g., using `Validate` method in constructor).
- This layer MUST have no dependencies on external frameworks or other layers (except `Shared`).
- Use `BaseEntity` from `Shared.Abstractions` for all entities.
- Keep business logic inside the Domain whenever possible (Domain-Driven Design).

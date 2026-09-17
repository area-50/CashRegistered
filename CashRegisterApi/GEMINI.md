# API Layer - Backend

The entry point of the ASP.NET Core application.

## Responsibilities
- **Controllers:** Exposing REST endpoints (`AuthController`, `CashFlowController`, etc.).
- **Middlewares:** Handling cross-cutting concerns (e.g., `ExceptionHandlingMiddleware`).
- **Program.cs:** Application configuration, DI registration, and middleware pipeline.
- **Appsettings:** Configuration settings for different environments.

## Guidelines for Gemini
- Keep Controllers thin. Most logic should reside in the Application layer.
- **DTO Only for Response**: Controllers must only return DTOs in HTTP responses.
- **No Entity Exposure**: Never allow Domain Entities to be serialized in the API response.
- **Naming Patterns (for Responses)**:
    - Consultas: `Get[Nome]Response`.
    - Criação: `CreateResponse`.
    - Atualização: `UpdateResponse`.
    - Deleção: `DeleteResponse`.
- **No Mapping in Controllers**: Controllers must not perform mapping of entities to DTOs; they should receive the ready DTO from the Use Case.
- Use `ActionResult<T>` for consistent API responses.
- Ensure proper use of HTTP verbs (GET, POST, PUT, DELETE).
- Verify CORS and authentication settings in `Program.cs` when changing frontend/backend interaction.
- **Soberania de Identidade do Token JWT**: Endpoints para ações do próprio usuário logado (`GetMe`, `ChangePassword`, `UpdateUserProfile`, `UpdateTimezone`) JAMAIS devem aceitar o ID do usuário como parâmetro de rota ou body. O ID DEVE ser extraído pelo controller exclusivamente do Token JWT (`User.FindFirstValue(ClaimTypes.NameIdentifier)`).
- **Autorização por Método Obrigatória**: É estritamente PROIBIDO declarar atributos `[Authorize]` sobre a classe do Controller. Todas as permissões DEVEM ser explicitadas individualmente sobre cada método/action do controller.
- não use repository nos controllers
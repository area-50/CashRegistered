using System.Data.Common;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.Middleware;

namespace CashRegister.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (!context.Response.HasStarted)
            {
                switch (context.Response.StatusCode)
                {
                    case (int)HttpStatusCode.Forbidden:
                        await HandleForbiddenAsync(context);
                        break;
                    case (int)HttpStatusCode.Unauthorized:
                        await HandleUnauthorizedAsync(context);
                        break;
                }
            }
        }
        catch (DomainException ex)
        {
            await HandleDomainExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception caught: {ex.GetType().Name} - {ex.Message}");
            if (IsInfrastructureException(ex))
            {
                await HandleInfrastructureExceptionAsync(context, ex);
            }
            else
            {
                await HandleGenericExceptionAsync(context, ex);
            }
        }
    }

    private static bool IsInfrastructureException(Exception ex)
    {
        // Verifica se a exceção é relacionada a falhas de conexão ou banco de dados
        var message = ex.Message.ToLower();
        return ex is DbUpdateException || 
               ex is DbException || 
               message.Contains("database") ||
               message.Contains("connection") ||
               (ex.InnerException != null && IsInfrastructureException(ex.InnerException));
    }

    private static Task HandleDomainExceptionAsync(HttpContext context, BaseException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new ErrorResponse
        {
            Title = "Atenção",
            Message = ex.Message,
            Errors = ex.Errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleInfrastructureExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

        var response = new ErrorResponse
        {
            Title = "Serviço Indisponível",
            Message = "Ocorreu um erro de conexão com o banco de dados."
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponse
        {
            Title = "Erro Interno do Servidor",
            Message = "Ocorreu um erro inesperado no sistema."
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }



    private static Task HandleForbiddenAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var response = new ErrorResponse
        {
            Title = "Acesso Negado",
            Message = "O usuário não tem autorização para esta ação."
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleUnauthorizedAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var response = new ErrorResponse
        {
            Title = "Não Autenticado",
            Message = "Autenticação necessária para acessar este recurso."
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace CashRegisterApi.Middlewares;

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

        var response = new
        {
            Type = ex.GetType().Name,
            Message = ex.Message,
            ex.Errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleInfrastructureExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

        var response = new
        {
            Type = "ServiceUnavailable",
            Message = "Ocorreu um erro de conexão com o banco de dados."
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            Type = "InternalServerError",
            Message = ex.Message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
using System.Reflection;
using System.Text;
using Application.Decorators;
using Application.Services;
using Domain.Identity.Enums;
using FluentValidation;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Abstractions;

namespace Application.Services;

public static class ServiceExtensions
{
    public static void ConfigurationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<IUnitOfWork>(provider =>
        {
            var dbContext = provider.GetRequiredService<CashRegisterDbContext>();
            var exceptionHandler = provider.GetRequiredService<PersistenceExceptionHandler>();
            return new UnitOfWorkDecorator(dbContext, exceptionHandler);
        });

        services.AddAutoMapper(
            cfg => { }, Assembly.GetExecutingAssembly()
        );
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var cookieToken = context.Request.Cookies["access_token"];

                        if (!string.IsNullOrEmpty(cookieToken)) context.Token = cookieToken;
                        
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => 
                policy.RequireRole(UserRole.Admin.ToString()))
            
            .AddPolicy("FinancialOnly", policy => 
                policy.RequireRole(
                    UserRole.Financial.ToString(), 
                    UserRole.Admin.ToString()
                ))
            
            .AddPolicy("LogisticsOnly", policy => 
                policy.RequireRole(
                    UserRole.Logistics.ToString(), 
                    UserRole.Admin.ToString()
                ))
            
            .AddPolicy("ComercialOnly", policy => 
            policy.RequireRole(
                UserRole.Business.ToString(), 
                UserRole.Admin.ToString()
            ));
    }
}
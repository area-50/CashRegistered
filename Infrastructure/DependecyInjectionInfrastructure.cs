using Domain.Financial.Repositories;
using Domain.Identity.Repositories;
using Domain.Inventory.Repositories;
using Domain.Security.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Financial.Repositories;
using Infrastructure.Identity.Repositories;
using Infrastructure.Inventory.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Security.Services;
using Infrastructure.Security.Repositories;
using Infrastructure.Utils;
using Infrastructure.Utils.Interfaces;
using Shared.Notifications;
using Shared.Abstractions;

namespace Infrastructure;

public static class DependencyInjectionInfrastructure
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<NotificationContext>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<ISqlUtils, SqlUtils>(); 

        services.AddDbContext<CashRegisterDbContext>(options =>
            options.UseNpgsql(connectionString)); 
        
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CashRegisterDbContext>());

        services.AddScoped<IPasswordHasher, Argon2Services>();
       
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<ICashFlowRepository, CashFlowRepository>();
        
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        
        services.AddScoped<IPersonRepository, PersonRepository>();

        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        
        services.AddScoped<IUomConversionRepository, UomConversionRepository>();
        
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        services.AddScoped<ITagRepository, TagRepository>();
        
        services.AddScoped<IProductRepository, ProductRepository>();
        
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        
        services.AddScoped<IStockBalanceRepository, StockBalanceRepository>();
        
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();

        return services;
    }
}
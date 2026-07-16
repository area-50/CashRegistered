using Application.Identity.Interfaces;
using Application.Security.Interfaces;
using Application.Financial.Interfaces;
using Application.Identity.UseCases;
using Application.Security.UseCases;
using Application.Financial.UseCases;
using Application.Inventory.Interfaces;
using Application.Inventory.UseCases;
using Application.Interfaces;
using Application.Services;
using Application.Services.Strategies;
using Application.Inventory.UseCases.Strategies;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Domain.Shared.Interfaces;

namespace Application;

public static class DependencyInjectionUseCases
{
    public static void AddUseCase(this IServiceCollection services)
    {
        services.AddScoped<IUserUseCase, UserUseCase>();
        
        services.AddScoped<ICashFlowUseCase, CashFlowUseCase>();
        
        services.AddScoped<IExpenseUseCase, ExpenseUseCase>();
        
        services.AddScoped<IAuthAppService, AuthAppService>();
        
        services.AddScoped<ITokenGenerator, TokenService>();
        
        services.AddScoped<IPersonUseCase, PersonUseCase>();

        services.AddScoped<IUnitOfMeasureUseCase, UnitOfMeasureUseCase>();
        
        services.AddScoped<IUomConversionUseCase, UomConversionUseCase>();
        
        services.AddScoped<ICategoryUseCase, CategoryUseCase>();
        
        services.AddScoped<ITagUseCase, TagUseCase>();
        
        services.AddScoped<IProductUseCase, ProductUseCase>();
        
        services.AddScoped<IWarehouseUseCase, WarehouseUseCase>();
        
        services.AddScoped<IStockBalanceUseCase, StockBalanceUseCase>();
        
        // Estratégias de Movimentação de Estoque
        services.AddScoped<IInventoryTransactionStrategy, EntryTransactionStrategy>();
        
        services.AddScoped<IInventoryTransactionStrategy, ExitTransactionStrategy>();
        
        services.AddScoped<IInventoryTransactionStrategy, TransferTransactionStrategy>();
        
        services.AddScoped<IInventoryTransactionUseCase, InventoryTransactionUseCase>();
        
        services.AddScoped<IInventoryRequisitionUseCase, InventoryRequisitionUseCase>();
        
        services.AddScoped<ISupplierUseCase, SupplierUseCase>();
        
        services.AddScoped<IPersistenceExceptionStrategy, UniqueConstraintStrategy>();
        
        services.AddScoped<IPersistenceExceptionStrategy, ForeignKeyViolationStrategy>();
        
        services.AddScoped<IPersistenceExceptionStrategy, InfrastructureFailureStrategy>();
        
        services.AddScoped<PersistenceExceptionHandler>();
        
        services.AddScoped<IInventoryRequisitionUseCase, InventoryRequisitionUseCase>();
        
        // Registro Nativo do Event Dispatcher
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Escaneia e registra automaticamente todos os INotificationHandler do assembly
        var assembly = typeof(DependencyInjectionUseCases).Assembly;
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && 
                        t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)));

        foreach (var type in handlerTypes)
        {
            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));
            
            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddTransient(handlerInterface, type);
            }
        }
    }
}
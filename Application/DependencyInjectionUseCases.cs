using Application.Identity.Interfaces;
using Application.Security.Interfaces;
using Application.Financial.Interfaces;
using Application.Identity.UseCases;
using Application.Security.UseCases;
using Application.Financial.UseCases;
using Application.Inventory.Interfaces;
using Application.Inventory.UseCases;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
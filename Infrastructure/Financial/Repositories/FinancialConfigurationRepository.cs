using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Financial.Repositories;

public class FinancialConfigurationRepository(CashRegisterDbContext context) : IFinancialConfigurationRepository
{
    public async Task<FinancialConfiguration?> GetActiveConfigurationAsync()
    {
        return await context.FinancialConfigurations
            .FirstOrDefaultAsync(x => x.IsActive);
    }

    public async Task CreateAsync(FinancialConfiguration configuration)
    {
        await context.FinancialConfigurations.AddAsync(configuration);
    }

    public void Update(FinancialConfiguration configuration)
    {
        context.FinancialConfigurations.Update(configuration);
    }
}

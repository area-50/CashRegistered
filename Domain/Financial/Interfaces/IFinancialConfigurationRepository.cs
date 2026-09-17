using Domain.Financial.Entities;

namespace Domain.Financial.Interfaces;

public interface IFinancialConfigurationRepository
{
    Task<FinancialConfiguration?> GetActiveConfigurationAsync();
    Task CreateAsync(FinancialConfiguration configuration);
    void Update(FinancialConfiguration configuration);
}

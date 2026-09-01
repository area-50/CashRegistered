using Domain.Financial.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Financial.Interfaces;

public interface IFinancialAccountRepository : IRepository<FinancialAccount>
{
    Task<PagedResponse<FinancialAccount>> SearchAsync(SearchFinancialAccountRequest request);
    Task<bool> ExistsByNameAsync(string name, int? ignoreId = null);
}

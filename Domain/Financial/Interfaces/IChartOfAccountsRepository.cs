using Domain.Financial.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;


namespace Domain.Financial.Interfaces;

public interface IChartOfAccountsRepository : IRepository<ChartOfAccounts>
{
    Task<PagedResponse<ChartOfAccounts>> SearchAsync(SearchChartOfAccountsRequest request);
    Task<bool> ExistsByCodeAsync(string code, int? ignoreId = null);
    Task<bool> ExistsByNameAsync(string name, int? ignoreId = null);
}

using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IUomConversionRepository : IRepository<UomConversion>
{
    Task<PagedResponse<UomConversion>> SearchAsync(SearchUomConversionRequest request);

    Task<UomConversion?> GetRuleAsync(int fromUomId, int toUomId, int? productId = null);
    Task<IEnumerable<UomConversion>> GetRulesForProductAsync(int productId);
}
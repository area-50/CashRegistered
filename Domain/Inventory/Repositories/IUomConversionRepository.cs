using Domain.Inventory.Entities;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IUomConversionRepository : IRepository<UomConversion>
{
    Task<PagedResponse<UomConversion>> SearchAsync(SearchUomConversionRequest request);

    Task<UomConversion?> GetRuleAsync(int fromUomId, int toUomId, int? productId = null);
    Task<IEnumerable<UomConversion>> GetRulesForProductAsync(int productId);
}
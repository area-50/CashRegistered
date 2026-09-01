using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;
using Domain.Shared.Response;

namespace Application.Inventory.UseCases;

public class UomConversionUseCase(
    IUomConversionRepository repository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : IUomConversionUseCase
{
    public async Task<CreateResponse> CreateUomConversion(CreateUomConversionRequest request)
    {
        var uom = new UomConversion(
            request.FromUomId,
            request.ToUomId,
            request.Multiplier,
            request.ProductId
        );

        if (uom.IsInvalid)
        {
            notificationContext.AddNotifications(uom.Notifications);
            return new CreateResponse
            {
                Id = 0
            };
        }

        await repository.CreateAsync(uom);
        await unitOfWork.CommitAsync();
        
        return new CreateResponse
        {
            Id = uom.Id
        };
    }

    public async Task<PagedResponse<GetSearchUomConversionResponse>> SearchUomConversion(
        SearchUomConversionRequest request
    )
    {
        var pagedUomSearches = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchUomConversionResponse>
        {
            Items = pagedUomSearches.Items.Select(uom => new GetSearchUomConversionResponse
                {
                    Id = uom.Id,
                    FromUnitName = uom.FromUom.Name,
                    FromUnitSymbol = uom.FromUom.Code,
                    FromUnitSymbolId = uom.FromUom.Id,
                    ToUnitName = uom.ToUom.Name,
                    ToUnitSymbol = uom.ToUom.Code,
                    ToUnitSymbolId = uom.ToUom.Id,
                    Multiplier = uom.Multiplier,
                    ProductName = uom.Product?.Name,
                    ProductId = uom.ProductId,
                    IsActive = uom.IsActive
                }
            ),
            Page = pagedUomSearches.Page,
            PageSize = pagedUomSearches.PageSize,
            TotalCount = pagedUomSearches.TotalCount
        };
    }

    public async Task<GetUomConversionByIdResponse> GetUomConversionById(int uomId)
    {
        var uom = await repository.GetByIdAsync(uomId);

        if (UomConversion.NotExists(uom, notificationContext)) return new GetUomConversionByIdResponse();

        return new GetUomConversionByIdResponse
        {
            Id = uom!.Id,
            FromUomId = uom.FromUomId,
            ToUomId = uom.ToUomId,
            Multiplier = uom.Multiplier,
            ProductId = uom.ProductId,
            IsActive = uom.IsActive
        };
    }

    public async Task<UpdateResponse> UpdateUomConversion(int id, UpdateUomConversionRequest request)
    {
        var uom = await repository.GetByIdAsync(id);

        if (UomConversion.NotExists(uom, notificationContext)) return new UpdateResponse {Id = 0};

        uom!.Update(
            request.FromUomId,
            request.ToUomId,
            request.Multiplier,
            request.IsActive,
            request.ProductId
        );

        repository.Update(uom);
        await unitOfWork.CommitAsync();
        return new UpdateResponse {Id = uom.Id};
    }

    public async Task DeactivateUomConversion(int uomId)
    {
        var uom = await repository.GetByIdAsync(uomId);

        if (UomConversion.NotExists(uom, notificationContext)) return;
        
        if (uom is { IsActive: false })
        {
            notificationContext.AddNotification("Desativar", "Essa conversão já está inativa.");
            return;
        }
        
        uom!.Deactivate();

        repository.Update(uom);
        await unitOfWork.CommitAsync();
    }

    public async Task<GetUomConversionRuleResponse?> GetConversionRule(GetUomConversionRuleRequest request)
    {
        var rule = await repository.GetRuleAsync(request.FromUomId, request.ToUomId, request.ProductId);
        
        // Se não achar a regra específica do produto, tenta achar uma regra global (ProductId nulo)
        if (rule == null && request.ProductId.HasValue)
        {
            rule = await repository.GetRuleAsync(request.FromUomId, request.ToUomId, null);
        }

        if (rule == null || !rule.IsActive)            return null;

        return new GetUomConversionRuleResponse
        {
            Id = rule.Id,
            Multiplier = rule.Multiplier,
            IsActive = rule.IsActive
        };
    }

    public async Task<IEnumerable<ProductConversionItemResponse>> GetRulesForProductAsync(int productId, int baseUomId)
    {
        var allRules = await repository.GetRulesForProductAsync(productId);
        var distinctRules = allRules.ToList();

        var reverseGraph = distinctRules
            .GroupBy(r => r.ToUomId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var forwardGraph = distinctRules
            .GroupBy(r => r.FromUomId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var queue = new Queue<(int CurrentUomId, decimal AccumulatedMultiplier, string CurrentRuleType)>();
        queue.Enqueue((baseUomId, 1m, "Base"));

        var resolvedConversions = new Dictionary<string, Shared.Inventory.Response.ProductConversionItemResponse>();

        while (queue.Count > 0)
        {
            var (currentUomId, currentMultiplier, currentPathRuleType) = queue.Dequeue();

            // Grafo Reverso (Busca pelas unidades de origem que convertem para a atual) -> Multiplicação
            if (reverseGraph.TryGetValue(currentUomId, out var incomingRules))
            {
                foreach (var rule in incomingRules)
                {
                    string edgeType = rule.ProductId.HasValue ? "ProductSpecific" : "Global";
                    string newPathRuleType = currentPathRuleType == "Base" ? edgeType : currentPathRuleType;
                    
                    string cacheKey = $"{rule.FromUomId}_{newPathRuleType}";

                    if (!resolvedConversions.ContainsKey(cacheKey) && rule.FromUomId != baseUomId) 
                    {
                        var absoluteMultiplier = currentMultiplier * rule.Multiplier;
                        
                        resolvedConversions[cacheKey] = new Shared.Inventory.Response.ProductConversionItemResponse
                        {
                            UomId = rule.FromUomId,
                            UomSymbol = rule.FromUom.Code,
                            Multiplier = absoluteMultiplier,
                            RuleType = newPathRuleType
                        };
                        
                        queue.Enqueue((rule.FromUomId, absoluteMultiplier, newPathRuleType));
                    }
                }
            }

            // Grafo Direto (Busca pelas unidades de destino que são convertidas a partir da atual) -> Divisão
            if (forwardGraph.TryGetValue(currentUomId, out var outgoingRules))
            {
                foreach (var rule in outgoingRules)
                {
                    string edgeType = rule.ProductId.HasValue ? "ProductSpecific" : "Global";
                    string newPathRuleType = currentPathRuleType == "Base" ? edgeType : currentPathRuleType;
                    
                    string cacheKey = $"{rule.ToUomId}_{newPathRuleType}";

                    if (!resolvedConversions.ContainsKey(cacheKey) && rule.ToUomId != baseUomId) 
                    {
                        var absoluteMultiplier = currentMultiplier / rule.Multiplier;
                        
                        resolvedConversions[cacheKey] = new Shared.Inventory.Response.ProductConversionItemResponse
                        {
                            UomId = rule.ToUomId,
                            UomSymbol = rule.ToUom.Code,
                            Multiplier = absoluteMultiplier,
                            RuleType = newPathRuleType
                        };
                        
                        queue.Enqueue((rule.ToUomId, absoluteMultiplier, newPathRuleType));
                    }
                }
            }
        }

        return resolvedConversions.Values;
    }
}
using Domain.Shared.DTOs;
using Domain.Shared.Response;
using Shared.Financial.Request;
using Shared.Financial.Response;


namespace Application.Financial.Interfaces;

public interface IChartOfAccountsUseCase
{
    Task<CreateResponse> CreateChartOfAccounts(CreateChartOfAccountsRequest request);
    Task<PagedResponse<GetSearchChartOfAccountsResponse>> SearchChartOfAccounts(SearchChartOfAccountsRequest request);
    Task<GetChartOfAccountsByIdResponse> GetChartOfAccountsById(int id);
    Task<UpdateResponse> UpdateChartOfAccounts(int id, UpdateChartOfAccountsRequest request);
    Task DeactivateChartOfAccounts(int id);
}

namespace Application.Financial.Interfaces;

public interface IFinancialAccountUseCase
{
    Task<CreateResponse> CreateFinancialAccount(CreateFinancialAccountRequest request);
    
    Task<PagedResponse<GetSearchFinancialAccountResponse>> SearchFinancialAccounts(SearchFinancialAccountRequest request);
    
    Task<GetFinancialAccountByIdResponse> GetFinancialAccountById(int id);
    
    Task<UpdateResponse> UpdateFinancialAccount(int id, UpdateFinancialAccountRequest request);
    
    Task DeactivateFinancialAccount(int id);
}

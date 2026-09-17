using Domain.Shared.DTOs;
using Domain.Shared.Response;
using Shared.Financial.Request;
using Shared.Financial.Response;

namespace Application.Financial.Interfaces;

public interface IFinancialDocumentUseCase
{
    Task<CreateResponse> CreateFinancialDocument(CreateFinancialDocumentRequest request);
    Task<PagedResponse<GetSearchFinancialDocumentResponse>> SearchFinancialDocuments(SearchFinancialDocumentRequest request);
    Task<GetFinancialDocumentByIdResponse> GetFinancialDocumentById(int id);
    Task<UpdateResponse> UpdateFinancialDocument(int id, UpdateFinancialDocumentRequest request);
    Task DeactivateFinancialDocument(int id);
    Task<CreateResponse> RegisterPaymentTransaction(CreatePaymentTransactionRequest request, int userId);
    Task<CreateBatchPaymentTransactionResponse> RegisterBatchPaymentTransaction(CreateBatchPaymentTransactionRequest request, int userId);
    Task<UpdateResponse> ApproveFinancialDocument(int id, int approvalUserId);
    Task<GetFinancialSettlementPreviewResponse> GetFinancialSettlementPreview(int documentId, DateTime paymentDate);
    Task<IEnumerable<GetPaymentTransactionHistoryResponse>> GetPaymentTransactionsByDocumentId(int documentId);
}



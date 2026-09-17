using Domain.Financial.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Domain.Financial.Interfaces;

public interface IFinancialDocumentRepository : IRepository<FinancialDocument>
{
    Task<PagedResponse<FinancialDocument>> SearchAsync(SearchFinancialDocumentRequest request);
    Task<FinancialInstallment?> GetInstallmentByIdAsync(int installmentId);
    Task CreatePaymentTransactionAsync(PaymentTransaction transaction);
    Task<IEnumerable<PaymentTransaction>> GetPaymentTransactionsByDocumentIdAsync(int documentId);
    void UpdateInstallment(FinancialInstallment installment);
}


using System.Linq.Expressions;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Domain.Shared.DTOs;
using Domain.Shared.Response;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Financial.Repositories;

public class FinancialDocumentRepository(CashRegisterDbContext context, ISqlUtils sqlUtils) : IFinancialDocumentRepository
{
    public async Task CreateAsync(FinancialDocument entity)
    {
        await context.FinancialDocuments.AddAsync(entity);
    }

    public async Task<FinancialDocument?> GetByIdAsync(int id)
    {
        return await context.FinancialDocuments
            .Include(f => f.Person)
            .Include(f => f.ChartOfAccounts)
            .Include(f => f.CostCenter)
            .Include(f => f.Installments)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<FinancialDocument>> FindAsync(Expression<Func<FinancialDocument, bool>> predicate)
    {
        return await context.FinancialDocuments
            .Include(f => f.Person)
            .Include(f => f.ChartOfAccounts)
            .Include(f => f.CostCenter)
            .Include(f => f.Installments)
            .Where(predicate)
            .ToListAsync();
    }

    public void Update(FinancialDocument entity)
    {
        context.FinancialDocuments.Update(entity);
    }

    public void Delete(FinancialDocument entity)
    {
        context.FinancialDocuments.Remove(entity);
    }

    public async Task<PagedResponse<FinancialDocument>> SearchAsync(SearchFinancialDocumentRequest request)
    {
        var query = context.FinancialDocuments
            .Include(f => f.Person)
            .Include(f => f.ChartOfAccounts)
            .Include(f => f.CostCenter)
            .Include(f => f.Installments)
            .AsNoTracking();

        query = sqlUtils.WhereAnd(query, request.DocumentType.HasValue, f => f.DocumentType == request.DocumentType!.Value);
        query = sqlUtils.WhereAnd(query, request.PersonId.HasValue, f => f.PersonId == request.PersonId!.Value);
        query = sqlUtils.WhereAnd(query, request.ChartOfAccountsId.HasValue, f => f.ChartOfAccountsId == request.ChartOfAccountsId!.Value);
        query = sqlUtils.WhereAnd(query, request.CostCenterId.HasValue, f => f.CostCenterId == request.CostCenterId!.Value);
        query = sqlUtils.WhereAnd(query, request.Status.HasValue, f => f.Status == request.Status!.Value);
        query = sqlUtils.WhereAnd(query, request.IsActive.HasValue, f => f.IsActive == request.IsActive!.Value);
        query = sqlUtils.WhereAnd(query, !string.IsNullOrWhiteSpace(request.DocumentNumber), f => EF.Functions.ILike(f.DocumentNumber, sqlUtils.SqlLikeContains(request.DocumentNumber!.Trim())));

        // Dynamic date filter (DueDate / IssueDate / PaymentDate)
        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            var startDate = request.StartDate ?? DateTime.MinValue;
            var endDate = request.EndDate ?? DateTime.MaxValue;

            query = request.DateType == "IssueDate" ? query.Where(f => f.IssueDate >= startDate && f.IssueDate <= endDate) :
                query.Where(f => f.DueDate >= startDate && f.DueDate <= endDate);
        }

        return await query.OrderByDescending(f => f.Id).ToPagedResponseAsync(request.Page, request.PageSize);
    }

    public async Task<FinancialInstallment?> GetInstallmentByIdAsync(int installmentId)
    {
        return await context.FinancialInstallments
            .Include(i => i.FinancialDocument)
                .ThenInclude(d => d.Installments)
            .FirstOrDefaultAsync(i => i.Id == installmentId);
    }

    public async Task CreatePaymentTransactionAsync(PaymentTransaction transaction)
    {
        await context.PaymentTransactions.AddAsync(transaction);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetPaymentTransactionsByDocumentIdAsync(int documentId)
    {
        return await context.PaymentTransactions
            .Include(pt => pt.FinancialInstallment)
            .Include(pt => pt.FinancialAccount)
            .Include(pt => pt.CreatedByUser)
                .ThenInclude(u => u.Person)
            .Where(pt => pt.FinancialInstallment.FinancialDocumentId == documentId)
            .OrderBy(pt => pt.PaymentDate)
            .ToListAsync();
    }

    public void UpdateInstallment(FinancialInstallment installment)
    {
        context.FinancialInstallments.Update(installment);
    }
}


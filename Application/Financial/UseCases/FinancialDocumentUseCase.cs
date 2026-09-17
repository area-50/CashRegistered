using Application.Financial.Interfaces;
using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Domain.Financial.Interfaces;

namespace Application.Financial.UseCases;

public class FinancialDocumentUseCase(
    IFinancialDocumentRepository repository,
    IFinancialAccountRepository financialAccountRepository,
    IFinancialConfigurationUseCase configurationUseCase,
    NotificationContext notificationContext,
    IUnitOfWork unitOfWork
) : IFinancialDocumentUseCase
{
    public async Task<CreateResponse> CreateFinancialDocument(CreateFinancialDocumentRequest request)
    {
        var config = await configurationUseCase.GetFinancialConfiguration();
        bool requiresApproval = request.DocumentType == DocumentType.Payable &&
                                config.EnableApprovalWorkflow &&
                                request.TotalAmount > config.ApprovalThresholdAmount;

        decimal fineRate = request.FineRate;
        decimal interestDailyRate = request.InterestDailyRate;
        
        if (request.DocumentType == DocumentType.Receivable)
        {
            bool hasIndividualFine = request.FineRate > 0;
            bool hasDefaultFine = config.DefaultFineRate > 0;

            if (hasIndividualFine)
            {
                fineRate = request.FineRate;
            }
            else if (hasDefaultFine)
            {
                fineRate = config.DefaultFineRate;
            }

            bool hasIndividualInterest = request.InterestDailyRate > 0;
            bool hasDefaultInterest = config.DefaultInterestDailyRate > 0;

            if (hasIndividualInterest)
            {
                interestDailyRate = request.InterestDailyRate;
            }
            else if (hasDefaultInterest)
            {
                interestDailyRate = config.DefaultInterestDailyRate;
            }
        }

        var document = new FinancialDocument(
            documentType: request.DocumentType,
            documentNumber: request.DocumentNumber,
            personId: request.PersonId,
            chartOfAccountsId: request.ChartOfAccountsId,
            issueDate: request.IssueDate,
            dueDate: request.DueDate,
            totalAmount: request.TotalAmount,
            costCenterId: request.CostCenterId,
            discountAmount: request.DiscountAmount,
            interestAmount: request.InterestAmount,
            fineAmount: request.FineAmount,
            fineRate: fineRate,
            interestDailyRate: interestDailyRate,
            notes: request.Notes,
            requiresApproval: requiresApproval
        );

        if (document.IsInvalid)
        {
            notificationContext.AddNotifications(document.Notifications);
            return new CreateResponse { Id = 0 };
        }

        // Generate Installments automatically
        int installmentsCount = Math.Max(1, request.InstallmentsCount);
        
        decimal installmentAmount = Math.Round(document.NetAmount / installmentsCount, 2);
        decimal remainder = document.NetAmount - installmentAmount * installmentsCount;

        for (int i = 1; i <= installmentsCount; i++)
        {
            decimal currentAmount = i == installmentsCount ? installmentAmount + remainder : installmentAmount;
            DateTime installmentDueDate = request.DueDate.AddMonths(i - 1);

            var installment = new FinancialInstallment(
                installmentNumber: i,
                dueDate: installmentDueDate,
                amount: currentAmount
            );

            document.AddInstallment(installment);
        }

        await repository.CreateAsync(document);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = document.Id };
    }

    public async Task<PagedResponse<GetSearchFinancialDocumentResponse>> SearchFinancialDocuments(SearchFinancialDocumentRequest request)
    {
        var pagedDocuments = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchFinancialDocumentResponse>
        {
            Items = pagedDocuments.Items.Select(d => new GetSearchFinancialDocumentResponse
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                DocumentTypeName = d.DocumentType.ToString(),
                DocumentNumber = d.DocumentNumber,
                PersonId = d.PersonId,
                PersonName = d.Person?.TradeName ?? d.Person?.Name?.FirstName ?? string.Empty,
                ChartOfAccountsId = d.ChartOfAccountsId,
                ChartOfAccountsName = d.ChartOfAccounts?.Name ?? string.Empty,
                CostCenterId = d.CostCenterId,
                CostCenterName = d.CostCenter?.Name,
                IssueDate = d.IssueDate,
                DueDate = d.DueDate,
                TotalAmount = d.TotalAmount,
                PaidAmount = d.PaidAmount,
                NetAmount = d.NetAmount,
                FineRate = d.FineRate,
                InterestDailyRate = d.InterestDailyRate,
                Status = d.Status,
                StatusName = d.Status.ToString(),
                InstallmentsCount = d.Installments.Count,
                IsActive = d.IsActive
            }),
            Page = pagedDocuments.Page,
            PageSize = pagedDocuments.PageSize,
            TotalCount = pagedDocuments.TotalCount
        };
    }

    public async Task<GetFinancialDocumentByIdResponse> GetFinancialDocumentById(int id)
    {
        var document = await repository.GetByIdAsync(id);

        if (FinancialDocument.NotExists(document, notificationContext)) return new GetFinancialDocumentByIdResponse();

        return new GetFinancialDocumentByIdResponse
        {
            Id = document!.Id,
            DocumentType = document.DocumentType,
            DocumentNumber = document.DocumentNumber,
            PersonId = document.PersonId,
            PersonName = document.Person?.TradeName ?? document.Person?.Name?.FirstName ?? string.Empty,
            ChartOfAccountsId = document.ChartOfAccountsId,
            ChartOfAccountsName = document.ChartOfAccounts?.Name ?? string.Empty,
            CostCenterId = document.CostCenterId,
            CostCenterName = document.CostCenter?.Name,
            IssueDate = document.IssueDate,
            DueDate = document.DueDate,
            TotalAmount = document.TotalAmount,
            DiscountAmount = document.DiscountAmount,
            InterestAmount = document.InterestAmount,
            FineAmount = document.FineAmount,
            FineRate = document.FineRate,
            InterestDailyRate = document.InterestDailyRate,
            NetAmount = document.NetAmount,
            PaidAmount = document.PaidAmount,
            Status = document.Status,
            Notes = document.Notes,
            IsActive = document.IsActive,
            Installments = document.Installments.Select(i => new FinancialInstallmentResponse
            {
                Id = i.Id,
                InstallmentNumber = i.InstallmentNumber,
                DueDate = i.DueDate,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                DiscountAmount = i.DiscountAmount,
                InterestAmount = i.InterestAmount,
                FineAmount = i.FineAmount,
                Status = i.Status,
                StatusName = i.Status.ToString()
            }).ToList()
        };
    }

    public async Task<UpdateResponse> UpdateFinancialDocument(int id, UpdateFinancialDocumentRequest request)
    {
        var document = await repository.GetByIdAsync(id);

        if (FinancialDocument.NotExists(document, notificationContext)) return new UpdateResponse { Id = 0 };

        if (request.IsActive && !document!.IsActive)
            document.Activate();
        else if (!request.IsActive && document!.IsActive)
            document.Deactivate();

        repository.Update(document!);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = document!.Id };
    }

    public async Task DeactivateFinancialAccount(int id)
    {
        var document = await repository.GetByIdAsync(id);
        if (FinancialDocument.NotExists(document, notificationContext)) return;

        document!.Deactivate();
        repository.Update(document);
        await unitOfWork.CommitAsync();
    }

    public async Task DeactivateFinancialDocument(int id)
    {
        var document = await repository.GetByIdAsync(id);
        if (FinancialDocument.NotExists(document, notificationContext)) return;

        document!.Deactivate();
        repository.Update(document);
        await unitOfWork.CommitAsync();
    }

    public async Task<CreateResponse> RegisterPaymentTransaction(CreatePaymentTransactionRequest request, int userId)
    {
        var installment = await repository.GetInstallmentByIdAsync(request.FinancialInstallmentId);
        if (installment == null)
        {
            notificationContext.AddNotification("Parcela", "A parcela informada não foi encontrada.");
            return new CreateResponse { Id = 0 };
        }

        if (installment.FinancialDocument.Status == DocumentStatus.PendingApproval)
        {
            notificationContext.AddNotification("Aprovação", $"A parcela nº {installment.InstallmentNumber} pertence ao documento {installment.FinancialDocument.DocumentNumber} que encontra-se pendente de aprovação de alçada.");
            return new CreateResponse { Id = 0 };
        }

        var account = await financialAccountRepository.GetByIdAsync(request.FinancialAccountId);
        if (account == null)
        {
            notificationContext.AddNotification("ContaFinanceira", "A conta financeira informada não foi encontrada.");
            return new CreateResponse { Id = 0 };
        }

        var calc = installment.CalculateSettlement(
            request.PaymentDate,
            installment.FinancialDocument.FineRate,
            installment.FinancialDocument.InterestDailyRate,
            overrideDiscount: request.DiscountApplied,
            overrideInterest: request.InterestApplied,
            overrideFine: request.FineApplied
        );

        decimal finalAmountPaid = request.AmountPaid > 0 ? request.AmountPaid : calc.SuggestedAmountPaid;

        var transaction = new PaymentTransaction(
            financialInstallmentId: request.FinancialInstallmentId,
            financialAccountId: request.FinancialAccountId,
            paymentDate: request.PaymentDate,
            amountPaid: finalAmountPaid,
            paymentMethod: request.PaymentMethod,
            createdByUserId: userId,
            discountApplied: calc.DiscountToApply,
            interestApplied: calc.InterestToApply,
            fineApplied: calc.FineToApply,
            transactionReceiptNumber: request.TransactionReceiptNumber,
            notes: request.Notes
        );

        if (transaction.IsInvalid)
        {
            notificationContext.AddNotifications(transaction.Notifications);
            return new CreateResponse { Id = 0 };
        }

        // Register Payment on Installment
        installment.RegisterPayment(finalAmountPaid, calc.DiscountToApply, calc.InterestToApply, calc.FineToApply);
        if (installment.IsInvalid)
        {
            notificationContext.AddNotifications(installment.Notifications);
            return new CreateResponse { Id = 0 };
        }
        repository.UpdateInstallment(installment);

        // Impact Financial Account Balance
        if (installment.FinancialDocument.DocumentType == DocumentType.Payable)
        {
            account.DebitBalance(finalAmountPaid);
        }
        else
        {
            account.CreditBalance(finalAmountPaid);
        }

        if (account.IsInvalid)
        {
            notificationContext.AddNotifications(account.Notifications);
            return new CreateResponse { Id = 0 };
        }
        financialAccountRepository.Update(account);

        // Recalculate Document Totals
        installment.FinancialDocument.RecalculateTotals();
        repository.Update(installment.FinancialDocument);

        await repository.CreatePaymentTransactionAsync(transaction);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = transaction.Id };
    }

    public async Task<CreateBatchPaymentTransactionResponse> RegisterBatchPaymentTransaction(CreateBatchPaymentTransactionRequest request, int userId)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            notificationContext.AddNotification("Itens", "Selecione ao menos uma parcela para liquidação em lote.");
            return new CreateBatchPaymentTransactionResponse { Id = 0 };
        }

        var account = await financialAccountRepository.GetByIdAsync(request.FinancialAccountId);
        if (account == null)
        {
            notificationContext.AddNotification("ContaFinanceira", "A conta financeira informada não foi encontrada.");
            return new CreateBatchPaymentTransactionResponse { Id = 0 };
        }

        decimal totalAmountPaid = 0;
        int lastTransactionId = 0;

        var modifiedDocuments = new HashSet<FinancialDocument>();

        foreach (var item in request.Items)
        {
            var installment = await repository.GetInstallmentByIdAsync(item.FinancialInstallmentId);
            if (installment == null)
            {
                notificationContext.AddNotification("Parcela", "A parcela selecionada não foi encontrada no sistema.");
                return new CreateBatchPaymentTransactionResponse { Id = 0 };
            }

            if (installment.Status == InstallmentStatus.Paid || installment.Status == InstallmentStatus.Canceled)
            {
                notificationContext.AddNotification("Parcela", $"A parcela nº {installment.InstallmentNumber} (Vencimento: {installment.DueDate:dd/MM/yyyy} - Valor: {installment.Amount:C}) já se encontra quitada ou cancelada.");
                return new CreateBatchPaymentTransactionResponse { Id = 0 };
            }

            if (installment.FinancialDocument.Status == DocumentStatus.PendingApproval)
            {
                notificationContext.AddNotification("Aprovação", $"A parcela nº {installment.InstallmentNumber} (Vencimento: {installment.DueDate:dd/MM/yyyy} - Valor: {installment.Amount:C}) pertence ao documento {installment.FinancialDocument.DocumentNumber} que encontra-se pendente de aprovação de alçada.");
                return new CreateBatchPaymentTransactionResponse { Id = 0 };
            }

            var calc = installment.CalculateSettlement(
                request.PaymentDate,
                installment.FinancialDocument.FineRate,
                installment.FinancialDocument.InterestDailyRate,
                overrideDiscount: item.DiscountApplied,
                overrideInterest: item.InterestApplied,
                overrideFine: item.FineApplied
            );

            decimal finalAmountPaid = item.AmountPaid > 0 ? item.AmountPaid : calc.SuggestedAmountPaid;

            var transaction = new PaymentTransaction(
                financialInstallmentId: item.FinancialInstallmentId,
                financialAccountId: request.FinancialAccountId,
                paymentDate: request.PaymentDate,
                amountPaid: finalAmountPaid,
                paymentMethod: request.PaymentMethod,
                createdByUserId: userId,
                discountApplied: calc.DiscountToApply,
                interestApplied: calc.InterestToApply,
                fineApplied: calc.FineToApply,
                transactionReceiptNumber: request.TransactionReceiptNumber,
                notes: request.Notes
            );

            if (transaction.IsInvalid)
            {
                notificationContext.AddNotifications(transaction.Notifications);
                return new CreateBatchPaymentTransactionResponse { Id = 0 };
            }

            installment.RegisterPayment(finalAmountPaid, calc.DiscountToApply, calc.InterestToApply, calc.FineToApply);
            if (installment.IsInvalid)
            {
                notificationContext.AddNotifications(installment.Notifications);
                return new CreateBatchPaymentTransactionResponse { Id = 0 };
            }

            repository.UpdateInstallment(installment);

            if (installment.FinancialDocument.DocumentType == DocumentType.Payable)
            {
                account.DebitBalance(finalAmountPaid);
            }
            else
            {
                account.CreditBalance(finalAmountPaid);
            }

            if (account.IsInvalid)
            {
                notificationContext.AddNotifications(account.Notifications);
                return new CreateBatchPaymentTransactionResponse { Id = 0 };
            }

            modifiedDocuments.Add(installment.FinancialDocument);
            await repository.CreatePaymentTransactionAsync(transaction);

            totalAmountPaid += finalAmountPaid;
            lastTransactionId = transaction.Id;
        }

        foreach (var document in modifiedDocuments)
        {
            document.RecalculateTotals();
            repository.Update(document);
        }

        financialAccountRepository.Update(account);
        await unitOfWork.CommitAsync();

        return new CreateBatchPaymentTransactionResponse
        {
            Id = lastTransactionId > 0 ? lastTransactionId : 1,
            TransactionsCount = request.Items.Count,
            TotalAmountPaid = totalAmountPaid
        };
    }

    public async Task<UpdateResponse> ApproveFinancialDocument(int id, int approvalUserId)
    {
        var document = await repository.GetByIdAsync(id);
        if (FinancialDocument.NotExists(document, notificationContext))
            return new UpdateResponse { Id = 0 };

        document!.Approve(approvalUserId);

        if (document.IsInvalid)
        {
            notificationContext.AddNotifications(document.Notifications);
            return new UpdateResponse { Id = 0 };
        }

        repository.Update(document);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = document.Id };
    }

    public async Task<GetFinancialSettlementPreviewResponse> GetFinancialSettlementPreview(int documentId, DateTime paymentDate)
    {
        var document = await repository.GetByIdAsync(documentId);
        if (FinancialDocument.NotExists(document, notificationContext))
            return new GetFinancialSettlementPreviewResponse();

        var previewItems = document!.Installments.Select(inst =>
        {
            var calc = inst.CalculateSettlement(
                paymentDate,
                document.FineRate,
                document.InterestDailyRate
            );

            return new FinancialSettlementPreviewItemResponse
            {
                InstallmentId = inst.Id,
                InstallmentNumber = inst.InstallmentNumber,
                DueDate = inst.DueDate,
                Amount = inst.Amount,
                PaidAmount = inst.PaidAmount,
                RemainingBalance = calc.RemainingBalance,
                Status = inst.Status.ToString(),
                IsOverdue = calc.IsOverdue,
                OverdueDays = calc.OverdueDays,
                DailyInterestAmount = calc.DailyInterestAmount,
                CalculatedFineAmount = calc.CalculatedFineAmount,
                CalculatedInterestAmount = calc.CalculatedInterestAmount,
                SuggestedAmountPaid = calc.SuggestedAmountPaid
            };
        }).ToList();

        return new GetFinancialSettlementPreviewResponse
        {
            DocumentId = document.Id,
            DocumentNumber = document.DocumentNumber,
            DocumentType = document.DocumentType.ToString(),
            PaymentDate = paymentDate,
            DocumentFineRate = document.FineRate,
            DocumentInterestDailyRate = document.InterestDailyRate,
            Items = previewItems
        };
    }

    public async Task<IEnumerable<GetPaymentTransactionHistoryResponse>> GetPaymentTransactionsByDocumentId(int documentId)
    {
        var transactions = await repository.GetPaymentTransactionsByDocumentIdAsync(documentId);
        return transactions.Select(pt => new GetPaymentTransactionHistoryResponse
        {
            Id = pt.Id,
            FinancialInstallmentId = pt.FinancialInstallmentId,
            InstallmentNumber = pt.FinancialInstallment?.InstallmentNumber ?? 1,
            FinancialAccountId = pt.FinancialAccountId,
            FinancialAccountName = pt.FinancialAccount?.Name ?? string.Empty,
            PaymentDate = pt.PaymentDate,
            AmountPaid = pt.AmountPaid,
            DiscountApplied = pt.DiscountApplied,
            InterestApplied = pt.InterestApplied,
            FineApplied = pt.FineApplied,
            PaymentMethod = pt.PaymentMethod,
            PaymentMethodName = pt.PaymentMethod.ToString(),
            TransactionReceiptNumber = pt.TransactionReceiptNumber,
            Notes = pt.Notes,
            CreatedByUserName = pt.CreatedByUser?.Person?.TradeName 
                ?? pt.CreatedByUser?.Person?.Name?.FirstName 
                ?? pt.CreatedByUser?.UserName 
                ?? string.Empty
        });
    }
}



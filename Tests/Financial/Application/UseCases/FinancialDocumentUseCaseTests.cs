using Application.Financial.Interfaces;
using Application.Financial.UseCases;
using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Domain.Financial.Interfaces;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;
using Domain.Shared.Response;
using FluentAssertions;
using Moq;
using Shared.Financial.Request;
using Shared.Financial.Response;
using Xunit;

namespace Tests.Financial.Application.UseCases;

public class FinancialDocumentUseCaseTests
{
    private readonly Mock<IFinancialDocumentRepository> _repositoryMock;
    private readonly Mock<IFinancialAccountRepository> _financialAccountRepositoryMock;
    private readonly Mock<IFinancialConfigurationUseCase> _configurationUseCaseMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly FinancialDocumentUseCase _useCase;

    public FinancialDocumentUseCaseTests()
    {
        _repositoryMock = new Mock<IFinancialDocumentRepository>();
        _financialAccountRepositoryMock = new Mock<IFinancialAccountRepository>();
        _configurationUseCaseMock = new Mock<IFinancialConfigurationUseCase>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();

        _configurationUseCaseMock.Setup(c => c.GetFinancialConfiguration())
            .ReturnsAsync(new GetFinancialConfigurationResponse
            {
                Id = 1,
                ApprovalThresholdAmount = 1000.00m,
                EnableApprovalWorkflow = false,
                IsActive = true
            });

        _useCase = new FinancialDocumentUseCase(
            _repositoryMock.Object,
            _financialAccountRepositoryMock.Object,
            _configurationUseCaseMock.Object,
            _notificationContext,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateFinancialDocument_ValidData_ShouldCreateDocumentAndInstallments()
    {
        // Arrange
        var request = new CreateFinancialDocumentRequest
        {
            DocumentType = DocumentType.Payable,
            DocumentNumber = "NF-1001",
            PersonId = 5,
            ChartOfAccountsId = 12,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 1500.00m,
            InstallmentsCount = 3
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialDocument>()))
            .Callback<FinancialDocument>(d => typeof(BaseEntity).GetProperty("Id")?.SetValue(d, 1));

        // Act
        var result = await _useCase.CreateFinancialDocument(request);

        // Assert
        result.Id.Should().Be(1);
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<FinancialDocument>(d => d.Installments.Count == 3)), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateFinancialDocument_Payable_ShouldKeepRatesFromRequestDirectly()
    {
        // Arrange
        var request = new CreateFinancialDocumentRequest
        {
            DocumentType = DocumentType.Payable,
            DocumentNumber = "NF-5555",
            PersonId = 5,
            ChartOfAccountsId = 12,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(15),
            TotalAmount = 2000.00m,
            FineRate = 2.5m,
            InterestDailyRate = 0.05m
        };

        FinancialDocument? createdDoc = null;
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialDocument>()))
            .Callback<FinancialDocument>(d =>
            {
                createdDoc = d;
                typeof(BaseEntity).GetProperty("Id")?.SetValue(d, 99);
            });

        // Act
        var result = await _useCase.CreateFinancialDocument(request);

        // Assert
        result.Id.Should().Be(99);
        _notificationContext.IsInvalid.Should().BeFalse();
        createdDoc.Should().NotBeNull();
        createdDoc!.FineRate.Should().Be(2.5m);
        createdDoc.InterestDailyRate.Should().Be(0.05m);
    }

    [Fact]
    public async Task SearchFinancialDocuments_ShouldReturnPagedResponse()
    {
        // Arrange
        var request = new SearchFinancialDocumentRequest { Page = 1, PageSize = 10, DocumentType = DocumentType.Payable };
        var doc = new FinancialDocument(DocumentType.Payable, "NF-2002", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 500.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(doc, 10);

        var pagedResponse = new PagedResponse<FinancialDocument>
        {
            Items = new List<FinancialDocument> { doc },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _repositoryMock.Setup(r => r.SearchAsync(request)).ReturnsAsync(pagedResponse);

        // Act
        var result = await _useCase.SearchFinancialDocuments(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Id.Should().Be(10);
        result.Items.First().DocumentNumber.Should().Be("NF-2002");
    }

    [Fact]
    public async Task RegisterPaymentTransaction_ValidData_ShouldProcessPaymentAndUpdateBalance()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-3003", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 1000.00m);
        var installment = new FinancialInstallment(1, DateTime.UtcNow.AddDays(30), 1000.00m);
        installment.AttachToDocument(1);
        doc.AddInstallment(installment);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(installment, doc);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(installment, 100);

        var account = new FinancialAccount("Banco Itaú", FinancialAccountType.CheckingAccount, 12, initialBalance: 5000.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 2);

        _repositoryMock.Setup(r => r.GetInstallmentByIdAsync(100)).ReturnsAsync(installment);
        _financialAccountRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(account);
        _repositoryMock.Setup(r => r.CreatePaymentTransactionAsync(It.IsAny<PaymentTransaction>()))
            .Callback<PaymentTransaction>(t => typeof(BaseEntity).GetProperty("Id")?.SetValue(t, 50));

        var paymentReq = new CreatePaymentTransactionRequest
        {
            FinancialInstallmentId = 100,
            FinancialAccountId = 2,
            PaymentDate = DateTime.UtcNow,
            AmountPaid = 1000.00m,
            PaymentMethod = PaymentMethod.Pix
        };

        // Act
        var result = await _useCase.RegisterPaymentTransaction(paymentReq, userId: 1);

        // Assert
        result.Id.Should().Be(50);
        installment.Status.Should().Be(InstallmentStatus.Paid);
        doc.Status.Should().Be(DocumentStatus.Paid);
        account.CurrentBalance.Should().Be(4000.00m); // 5000 - 1000 = 4000
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterPaymentTransaction_InsufficientAccountBalance_ShouldReturnErrorNotification()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-3004", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 1000.00m);
        var installment = new FinancialInstallment(1, DateTime.UtcNow.AddDays(30), 1000.00m);
        doc.AddInstallment(installment);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(installment, doc);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(installment, 101);

        // Account with low balance (100.00)
        var account = new FinancialAccount("Banco Itaú", FinancialAccountType.CheckingAccount, 12, initialBalance: 100.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 2);

        _repositoryMock.Setup(r => r.GetInstallmentByIdAsync(101)).ReturnsAsync(installment);
        _financialAccountRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(account);

        var paymentReq = new CreatePaymentTransactionRequest
        {
            FinancialInstallmentId = 101,
            FinancialAccountId = 2,
            PaymentDate = DateTime.UtcNow,
            AmountPaid = 1000.00m,
            PaymentMethod = PaymentMethod.Pix
        };

        // Act
        var result = await _useCase.RegisterPaymentTransaction(paymentReq, userId: 1);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Message.Contains("Saldo insuficiente"));
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task RegisterBatchPaymentTransaction_ValidData_ShouldProcessBatchAndReturnResponse()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-4004", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 2000.00m);
        var inst1 = new FinancialInstallment(1, DateTime.UtcNow.AddDays(30), 1000.00m);
        var inst2 = new FinancialInstallment(2, DateTime.UtcNow.AddDays(60), 1000.00m);
        inst1.AttachToDocument(1);
        inst2.AttachToDocument(1);
        doc.AddInstallment(inst1);
        doc.AddInstallment(inst2);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(inst1, doc);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(inst2, doc);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(inst1, 201);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(inst2, 202);

        var account = new FinancialAccount("Caixa Central", FinancialAccountType.Cash, 12, initialBalance: 5000.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 5);

        _repositoryMock.Setup(r => r.GetInstallmentByIdAsync(201)).ReturnsAsync(inst1);
        _repositoryMock.Setup(r => r.GetInstallmentByIdAsync(202)).ReturnsAsync(inst2);
        _financialAccountRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);
        _repositoryMock.Setup(r => r.CreatePaymentTransactionAsync(It.IsAny<PaymentTransaction>()))
            .Callback<PaymentTransaction>(t => typeof(BaseEntity).GetProperty("Id")?.SetValue(t, 99));

        var batchReq = new CreateBatchPaymentTransactionRequest
        {
            FinancialAccountId = 5,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Items = new List<PaymentTransactionItemRequest>
            {
                new PaymentTransactionItemRequest { FinancialInstallmentId = 201, AmountPaid = 1000.00m },
                new PaymentTransactionItemRequest { FinancialInstallmentId = 202, AmountPaid = 1000.00m }
            }
        };

        // Act
        var result = await _useCase.RegisterBatchPaymentTransaction(batchReq, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(99);
        result.TransactionsCount.Should().Be(2);
        result.TotalAmountPaid.Should().Be(2000.00m);
        inst1.Status.Should().Be(InstallmentStatus.Paid);
        inst2.Status.Should().Be(InstallmentStatus.Paid);
        doc.Status.Should().Be(DocumentStatus.Paid);
        account.CurrentBalance.Should().Be(3000.00m); // 5000 - 2000 = 3000
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterBatchPaymentTransaction_AlreadyPaidInstallment_ShouldReturnIdZeroAndHumanizedNotification()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-5005", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 1000.00m);
        var inst = new FinancialInstallment(1, DateTime.UtcNow.AddDays(30), 1000.00m);
        inst.RegisterPayment(1000.00m, 0, 0, 0); // Mark as Paid
        doc.AddInstallment(inst);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(inst, doc);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(inst, 301);

        var account = new FinancialAccount("Caixa Central", FinancialAccountType.Cash, 12, initialBalance: 5000.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 5);

        _repositoryMock.Setup(r => r.GetInstallmentByIdAsync(301)).ReturnsAsync(inst);
        _financialAccountRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);

        var batchReq = new CreateBatchPaymentTransactionRequest
        {
            FinancialAccountId = 5,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            Items = new List<PaymentTransactionItemRequest>
            {
                new PaymentTransactionItemRequest { FinancialInstallmentId = 301, AmountPaid = 1000.00m }
            }
        };

        // Act
        var result = await _useCase.RegisterBatchPaymentTransaction(batchReq, userId: 1);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Message.Contains("nº 1") && n.Message.Contains("já se encontra quitada"));
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateFinancialDocument_AboveThresholdAndWorkflowEnabled_ShouldSetPendingApprovalStatus()
    {
        // Arrange
        _configurationUseCaseMock.Setup(c => c.GetFinancialConfiguration())
            .ReturnsAsync(new GetFinancialConfigurationResponse
            {
                Id = 1,
                ApprovalThresholdAmount = 1000.00m,
                EnableApprovalWorkflow = true,
                IsActive = true
            });

        var request = new CreateFinancialDocumentRequest
        {
            DocumentType = DocumentType.Payable,
            DocumentNumber = "NF-ALCADA-01",
            PersonId = 5,
            ChartOfAccountsId = 12,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 5500.00m,
            InstallmentsCount = 2
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialDocument>()))
            .Callback<FinancialDocument>(d => typeof(BaseEntity).GetProperty("Id")?.SetValue(d, 99));

        // Act
        var result = await _useCase.CreateFinancialDocument(request);

        // Assert
        result.Id.Should().Be(99);
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<FinancialDocument>(d => d.Status == DocumentStatus.PendingApproval)), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterPaymentTransaction_PendingApprovalDocument_ShouldReturnIdZeroAndHumanizedNotification()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-ALCADA-02", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 5000.00m, requiresApproval: true);
        var installment = new FinancialInstallment(1, DateTime.UtcNow.AddDays(30), 5000.00m);
        doc.AddInstallment(installment);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(installment, doc);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(installment, 105);

        var account = new FinancialAccount("Banco Itaú", FinancialAccountType.CheckingAccount, 12, initialBalance: 10000.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 2);

        _repositoryMock.Setup(r => r.GetInstallmentByIdAsync(105)).ReturnsAsync(installment);

        var paymentReq = new CreatePaymentTransactionRequest
        {
            FinancialInstallmentId = 105,
            FinancialAccountId = 2,
            PaymentDate = DateTime.UtcNow,
            AmountPaid = 5000.00m,
            PaymentMethod = PaymentMethod.Pix
        };

        // Act
        var result = await _useCase.RegisterPaymentTransaction(paymentReq, userId: 1);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Message.Contains("pendente de aprovação de alçada"));
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task RegisterBatchPaymentTransaction_PendingApprovalDocument_ShouldReturnIdZeroAndHumanizedNotification()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-ALCADA-03", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 5000.00m, requiresApproval: true);
        var installment = new FinancialInstallment(1, DateTime.UtcNow.AddDays(30), 5000.00m);
        doc.AddInstallment(installment);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(installment, doc);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(installment, 106);

        var account = new FinancialAccount("Banco Itaú", FinancialAccountType.CheckingAccount, 12, initialBalance: 10000.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 2);

        _repositoryMock.Setup(r => r.GetInstallmentByIdAsync(106)).ReturnsAsync(installment);
        _financialAccountRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(account);

        var batchReq = new CreateBatchPaymentTransactionRequest
        {
            FinancialAccountId = 2,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Pix,
            Items = new List<PaymentTransactionItemRequest>
            {
                new PaymentTransactionItemRequest { FinancialInstallmentId = 106, AmountPaid = 5000.00m }
            }
        };

        // Act
        var result = await _useCase.RegisterBatchPaymentTransaction(batchReq, userId: 1);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Message.Contains("pendente de aprovação de alçada"));
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ApproveFinancialDocument_PendingApprovalDocument_ShouldApproveDocumentAndChangeStatusToOpen()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-ALCADA-04", 5, 12, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 5000.00m, requiresApproval: true);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(doc, 77);

        _repositoryMock.Setup(r => r.GetByIdAsync(77)).ReturnsAsync(doc);

        // Act
        var result = await _useCase.ApproveFinancialDocument(77, approvalUserId: 99);

        // Assert
        result.Id.Should().Be(77);
        doc.Status.Should().Be(DocumentStatus.Open);
        doc.ApprovalUserId.Should().Be(99);
        doc.ApprovedAt.Should().NotBeNull();
        _repositoryMock.Verify(r => r.Update(doc), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateFinancialDocument_ReceivableType_ShouldApplyDefaultFineFromConfiguration()
    {
        // Arrange
        _configurationUseCaseMock.Setup(c => c.GetFinancialConfiguration())
            .ReturnsAsync(new GetFinancialConfigurationResponse
            {
                Id = 1,
                DefaultFineRate = 2.00m,
                IsActive = true
            });

        var request = new CreateFinancialDocumentRequest
        {
            DocumentType = DocumentType.Receivable,
            DocumentNumber = "REC-1001",
            PersonId = 5,
            ChartOfAccountsId = 12,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 1000.00m,
            FineAmount = 0m
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialDocument>()))
            .Callback<FinancialDocument>(d => typeof(BaseEntity).GetProperty("Id")?.SetValue(d, 88));

        // Act
        var result = await _useCase.CreateFinancialDocument(request);

        // Assert
        result.Id.Should().Be(88);
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<FinancialDocument>(d => d.FineRate == 2.00m)), Times.Once);
    }

    [Fact]
    public async Task CreateFinancialDocument_ReceivableType_WithIndividualRates_ShouldUseIndividualRatesAsMandatory()
    {
        // Arrange
        _configurationUseCaseMock.Setup(c => c.GetFinancialConfiguration())
            .ReturnsAsync(new GetFinancialConfigurationResponse
            {
                Id = 1,
                DefaultFineRate = 2.00m,
                DefaultInterestDailyRate = 0.0333m,
                IsActive = true
            });

        var request = new CreateFinancialDocumentRequest
        {
            DocumentType = DocumentType.Receivable,
            DocumentNumber = "REC-1001",
            PersonId = 5,
            ChartOfAccountsId = 12,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 1000.00m,
            FineRate = 5.00m, // Individual mandatório
            InterestDailyRate = 0.1000m // Individual mandatório
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialDocument>()))
            .Callback<FinancialDocument>(d => typeof(BaseEntity).GetProperty("Id")?.SetValue(d, 88));

        // Act
        var result = await _useCase.CreateFinancialDocument(request);

        // Assert
        result.Id.Should().Be(88);
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<FinancialDocument>(d => d.FineRate == 5.00m && d.InterestDailyRate == 0.1000m)), Times.Once);
    }

    [Fact]
    public async Task CreateFinancialDocument_ReceivableType_WithoutIndividualRates_ShouldApplyDefaultFromConfiguration()
    {
        // Arrange
        _configurationUseCaseMock.Setup(c => c.GetFinancialConfiguration())
            .ReturnsAsync(new GetFinancialConfigurationResponse
            {
                Id = 1,
                DefaultFineRate = 2.00m,
                DefaultInterestDailyRate = 0.0333m,
                IsActive = true
            });

        var request = new CreateFinancialDocumentRequest
        {
            DocumentType = DocumentType.Receivable,
            DocumentNumber = "REC-1002",
            PersonId = 5,
            ChartOfAccountsId = 12,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 1000.00m,
            FineRate = 0m,
            InterestDailyRate = 0m
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialDocument>()))
            .Callback<FinancialDocument>(d => typeof(BaseEntity).GetProperty("Id")?.SetValue(d, 89));

        // Act
        var result = await _useCase.CreateFinancialDocument(request);

        // Assert
        result.Id.Should().Be(89);
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<FinancialDocument>(d => d.FineRate == 2.00m && d.InterestDailyRate == 0.0333m)), Times.Once);
    }

    [Fact]
    public async Task CreateFinancialDocument_ReceivableType_NoRatesExist_ShouldKeepRatesAsZero()
    {
        // Arrange
        _configurationUseCaseMock.Setup(c => c.GetFinancialConfiguration())
            .ReturnsAsync(new GetFinancialConfigurationResponse
            {
                Id = 1,
                DefaultFineRate = 0m,
                DefaultInterestDailyRate = 0m,
                IsActive = true
            });

        var request = new CreateFinancialDocumentRequest
        {
            DocumentType = DocumentType.Receivable,
            DocumentNumber = "REC-1003",
            PersonId = 5,
            ChartOfAccountsId = 12,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 1000.00m,
            FineRate = 0m,
            InterestDailyRate = 0m
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialDocument>()))
            .Callback<FinancialDocument>(d => typeof(BaseEntity).GetProperty("Id")?.SetValue(d, 90));

        // Act
        var result = await _useCase.CreateFinancialDocument(request);

        // Assert
        result.Id.Should().Be(90);
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<FinancialDocument>(d => d.FineRate == 0m && d.InterestDailyRate == 0m)), Times.Once);
    }

    [Fact]
    public async Task GetFinancialSettlementPreview_ValidDocument_ShouldReturnCalculatedPreviewItems()
    {
        // Arrange
        var doc = new FinancialDocument(
            documentType: DocumentType.Receivable,
            documentNumber: "DOC-PREVIEW-1",
            personId: 10,
            chartOfAccountsId: 5,
            issueDate: DateTime.UtcNow.AddDays(-20),
            dueDate: DateTime.UtcNow.AddDays(-10),
            totalAmount: 1000.00m,
            fineRate: 2.0m,
            interestDailyRate: 0.1m
        );
        typeof(BaseEntity).GetProperty("Id")?.SetValue(doc, 50);

        var inst = new FinancialInstallment(1, DateTime.UtcNow.AddDays(-10), 1000.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(inst, 101);
        doc.AddInstallment(inst);

        _repositoryMock.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(doc);

        // Act
        var result = await _useCase.GetFinancialSettlementPreview(50, DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().Be(50);
        result.Items.Should().HaveCount(1);

        var item = result.Items.First();
        item.InstallmentId.Should().Be(101);
        item.IsOverdue.Should().BeTrue();
        item.OverdueDays.Should().Be(10);
        item.CalculatedFineAmount.Should().Be(20.00m);
        item.DailyInterestAmount.Should().Be(1.00m);
        item.CalculatedInterestAmount.Should().Be(10.00m);
        item.SuggestedAmountPaid.Should().Be(1030.00m);
    }

    [Fact]
    public async Task GetPaymentTransactionsByDocumentId_ValidDocument_ShouldReturnMappedHistory()
    {
        // Arrange
        var doc = new FinancialDocument(
            documentType: DocumentType.Payable,
            documentNumber: "DOC-HIST-1",
            personId: 10,
            chartOfAccountsId: 5,
            issueDate: DateTime.UtcNow.AddDays(-20),
            dueDate: DateTime.UtcNow.AddDays(-10),
            totalAmount: 1000.00m
        );
        typeof(BaseEntity).GetProperty("Id")?.SetValue(doc, 60);

        var inst = new FinancialInstallment(1, DateTime.UtcNow.AddDays(-10), 1000.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(inst, 201);
        typeof(FinancialInstallment).GetProperty("FinancialDocument")?.SetValue(inst, doc);

        var account = new FinancialAccount("Banco Itaú", FinancialAccountType.CheckingAccount, 12);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 3);
        typeof(FinancialAccount).GetProperty("Name")?.SetValue(account, "Banco Itaú");

        var transaction = new PaymentTransaction(
            financialInstallmentId: 201,
            financialAccountId: 3,
            paymentDate: DateTime.UtcNow.AddDays(-5),
            amountPaid: 1000.00m,
            paymentMethod: PaymentMethod.Pix,
            createdByUserId: 1,
            discountApplied: 50.00m,
            interestApplied: 10.00m,
            fineApplied: 5.00m,
            transactionReceiptNumber: "PIX-123456",
            notes: "Pagamento quitado"
        );
        typeof(PaymentTransaction).GetProperty("FinancialInstallment")?.SetValue(transaction, inst);
        typeof(PaymentTransaction).GetProperty("FinancialAccount")?.SetValue(transaction, account);

        _repositoryMock.Setup(r => r.GetPaymentTransactionsByDocumentIdAsync(60))
            .ReturnsAsync(new List<PaymentTransaction> { transaction });

        // Act
        var result = await _useCase.GetPaymentTransactionsByDocumentId(60);

        // Assert
        result.Should().NotBeNull();
        var list = result.ToList();
        list.Should().HaveCount(1);

        var historyItem = list.First();
        historyItem.FinancialInstallmentId.Should().Be(201);
        historyItem.InstallmentNumber.Should().Be(1);
        historyItem.FinancialAccountId.Should().Be(3);
        historyItem.FinancialAccountName.Should().Be("Banco Itaú");
        historyItem.AmountPaid.Should().Be(1000.00m);
        historyItem.DiscountApplied.Should().Be(50.00m);
        historyItem.InterestApplied.Should().Be(10.00m);
        historyItem.FineApplied.Should().Be(5.00m);
        historyItem.PaymentMethod.Should().Be(PaymentMethod.Pix);
        historyItem.PaymentMethodName.Should().Be("Pix");
        historyItem.TransactionReceiptNumber.Should().Be("PIX-123456");
        historyItem.Notes.Should().Be("Pagamento quitado");
    }
}



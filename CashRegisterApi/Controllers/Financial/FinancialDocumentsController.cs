using System.Security.Claims;
using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers.Financial;

[ApiController]
[Route("api/financial-documents")]
public class FinancialDocumentsController(IFinancialDocumentUseCase useCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> CreateFinancialDocument([FromBody] CreateFinancialDocumentRequest request)
    {
        var response = await useCase.CreateFinancialDocument(request);
        return Ok(response);
    }

    [HttpGet("search")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> SearchFinancialDocuments([FromQuery] SearchFinancialDocumentRequest request)
    {
        var response = await useCase.SearchFinancialDocuments(request);
        return Ok(response);
    }

    [HttpGet("{id}/GetFinancialDocumentById")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetFinancialDocumentById([FromRoute] int id)
    {
        var response = await useCase.GetFinancialDocumentById(id);
        return Ok(response);
    }

    [HttpPut("{id}/Update")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> UpdateFinancialDocument(
        [FromRoute] int id, [FromBody] UpdateFinancialDocumentRequest request
    )
    {
        var response = await useCase.UpdateFinancialDocument(id, request);
        return Ok(response);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> DeactivateFinancialDocument([FromRoute] int id)
    {
        await useCase.DeactivateFinancialDocument(id);
        return Ok();
    }

    [HttpPost("payment-transactions")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> RegisterPaymentTransaction([FromBody] CreatePaymentTransactionRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1";
        int.TryParse(userIdString, out int userId);
        if (userId <= 0) userId = 1;

        var response = await useCase.RegisterPaymentTransaction(request, userId);
        return Ok(response);
    }

    [HttpPost("register-batch-payment")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> RegisterBatchPaymentTransaction([FromBody] CreateBatchPaymentTransactionRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1";
        int.TryParse(userIdString, out int userId);
        if (userId <= 0) userId = 1;

        var response = await useCase.RegisterBatchPaymentTransaction(request, userId);
        return Ok(response);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> ApproveFinancialDocument([FromRoute] int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1";
        int.TryParse(userIdString, out int userId);
        if (userId <= 0) userId = 1;

        var response = await useCase.ApproveFinancialDocument(id, userId);
        return Ok(response);
    }

    [HttpGet("{id}/settlement-preview")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetSettlementPreview([FromRoute] int id, [FromQuery] DateTime? paymentDate)
    {
        var targetDate = paymentDate ?? DateTime.UtcNow;
        var response = await useCase.GetFinancialSettlementPreview(id, targetDate);
        return Ok(response);
    }

    [HttpGet("{id}/payment-transactions")]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> GetPaymentTransactions([FromRoute] int id)
    {
        var response = await useCase.GetPaymentTransactionsByDocumentId(id);
        return Ok(response);
    }
}



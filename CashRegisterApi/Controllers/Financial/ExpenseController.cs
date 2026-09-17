using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers.Financial;

[Obsolete("Funcionalidade depreciada.")]
[Route("api/[controller]")]
[ApiController]
public class ExpenseController(IExpenseUseCase expenseUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "FinancialOnly")]
    public async Task<IActionResult> CreateExpense(CreateExpenseRequest request)
    {
        await expenseUseCase.CreateExpense(request);
        return Created();
    }
}
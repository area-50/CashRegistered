using System.Threading.Tasks;
using Application.Financial.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Financial.Request;

namespace CashRegister.Controllers.Financial;

[Obsolete("Funcionalidade depreciada.")]
[Route("api/[controller]")]
[ApiController]
public class ExpenseController(IExpenseUseCase expenseUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateExpense(CreateExpenseRequest request)
    {
        await expenseUseCase.CreateExpense(request);
        return Created();
    }
}
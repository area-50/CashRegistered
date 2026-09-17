namespace Shared.Financial.Response;

public class CreateBatchPaymentTransactionResponse
{
    public int Id { get; set; }
    public int TransactionsCount { get; set; }
    public decimal TotalAmountPaid { get; set; }
}

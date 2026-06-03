namespace Shared.Inventory.Response;

public class GetUomConversionByIdResponse
{
    public int Id { get; set; }

    public int FromUomId { get; set; }

    public int ToUomId { get; set; }

    public decimal Multiplier { get; set; }

    public int? ProductId { get; set; }

    public bool IsActive { get; set; }
}

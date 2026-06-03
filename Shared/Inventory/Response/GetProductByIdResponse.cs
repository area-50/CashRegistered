namespace Shared.Inventory.Response;

public class GetProductByIdResponse
{
    public int  Id { get; set; }
    
    public string Name { get; set; }
    
    public string Sku { get; set; }

    public string? Description { get; set; }

    public string? NcmCode  { get; set; }

    public int CategoryId { get; set; }

    public int BaseUomId { get; set; }

    public  bool IsActive { get; set; }

    public IList<int> TagIds { get; set; } = new List<int>();
}
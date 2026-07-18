using System.ComponentModel.DataAnnotations;

namespace Shared.Inventory.Request;

public class CreateCostCenterRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public int ManagerId { get; set; }
}

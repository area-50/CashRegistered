using System.ComponentModel.DataAnnotations;

namespace Shared.Inventory.Request;

public class UpdateCostCenterRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public int ManagerId { get; set; }

    public bool IsActive { get; set; } = true;
}

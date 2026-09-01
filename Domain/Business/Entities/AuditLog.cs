using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Business.Entities;

public class AuditLog : BaseEntity
{
    public AuditLog(
        int userId,
        string actionType,
        string affectedTable,
        int affectedRecordId,
        string? oldDataJson = null,
        string? newDataJson = null
    )
    {
        UserId = userId;
        ActionType = actionType;
        AffectedTable = affectedTable;
        AffectedRecordId = affectedRecordId;
        OldDataJson = oldDataJson;
        NewDataJson = newDataJson;
    }

    protected AuditLog() { }

    public int UserId { get; set; }
    public User User { get; set; }
    public string ActionType { get; set; }
    public string AffectedTable { get; set; }
    public int AffectedRecordId { get; set; }
    public string? OldDataJson { get; set; }
    public string? NewDataJson { get; set; }
}

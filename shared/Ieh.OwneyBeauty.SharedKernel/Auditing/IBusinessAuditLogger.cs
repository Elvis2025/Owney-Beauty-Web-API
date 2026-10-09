using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ieh.OwneyBeauty.SharedKernel.Auditing;

/// <summary>
/// Auditoría de negocio inmutable (OB-49-001): pagos, KYC, roles, suspensiones, reembolsos, disputas.
/// Complementa al AuditLogging técnico de ABP. La implementación vive en el módulo Platform;
/// los demás módulos solo dependen de esta abstracción.
/// </summary>
public interface IBusinessAuditLogger
{
    Task LogAsync(BusinessAuditEntry entry);
}

public sealed record BusinessAuditEntry(
    string Module,
    string Action,
    string EntityType,
    string? EntityId,
    bool Succeeded = true,
    string? Reason = null,
    IReadOnlyDictionary<string, string>? Data = null);

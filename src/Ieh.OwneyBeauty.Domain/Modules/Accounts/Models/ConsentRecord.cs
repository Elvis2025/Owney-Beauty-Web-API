using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.Models;

/// <summary>
/// Registro de consentimiento solo-anexar (ACC-M03, OB-48-004): aceptación de un documento legal
/// o concesión/retiro de un tipo de consentimiento. El estado vigente es el registro más reciente.
/// </summary>
public class ConsentRecord : Entity<Guid>, IHasCreationTime
{
    public Guid UserId { get; private set; }

    public Guid? LegalDocumentId { get; private set; }

    public string? LegalDocumentVersion { get; private set; }

    public ConsentType? ConsentType { get; private set; }

    public bool Granted { get; private set; }

    public string? ClientIpAddress { get; private set; }

    public DateTime CreationTime { get; private set; }

    protected ConsentRecord()
    {
    }

    private ConsentRecord(Guid id, Guid userId, DateTime now, string? clientIp)
        : base(id)
    {
        UserId = userId;
        CreationTime = now;
        ClientIpAddress = clientIp is { Length: > AccountsConsts.MaxClientIpLength } ? clientIp[..AccountsConsts.MaxClientIpLength] : clientIp;
    }

    public static ConsentRecord ForDocument(Guid id, Guid userId, LegalDocument document, DateTime now, string? clientIp)
    {
        Check.NotNull(document, nameof(document));
        if (!document.IsPublished)
        {
            throw new BusinessException(AccountsErrorCodes.LegalDocumentNotPublished);
        }

        return new ConsentRecord(id, userId, now, clientIp)
        {
            LegalDocumentId = document.Id,
            LegalDocumentVersion = document.Version,
            Granted = true,
        };
    }

    public static ConsentRecord ForConsent(Guid id, Guid userId, ConsentType type, bool granted, DateTime now, string? clientIp) =>
        new(id, userId, now, clientIp)
        {
            ConsentType = type,
            Granted = granted,
        };
}

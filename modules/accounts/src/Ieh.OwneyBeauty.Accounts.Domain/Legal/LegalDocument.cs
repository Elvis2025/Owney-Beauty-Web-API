using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Ieh.OwneyBeauty.Accounts.Legal;

/// <summary>
/// Documento legal versionado (ACC-M02): términos, privacidad, cancelación, acuerdo con profesionales.
/// Un documento publicado es inmutable; para cambiarlo se crea una versión nueva.
/// Por tipo e idioma solo una versión es la vigente (IsCurrent).
/// </summary>
public class LegalDocument : FullAuditedAggregateRoot<Guid>
{
    public LegalDocumentType Type { get; private set; }

    public string Language { get; private set; } = null!;

    public string Version { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    /// <summary>Contenido en Markdown. Alternativamente <see cref="ExternalUrl"/>.</summary>
    public string? Content { get; private set; }

    public string? ExternalUrl { get; private set; }

    /// <summary>Si es true, los usuarios deben aceptarlo para seguir usando la plataforma.</summary>
    public bool RequiresAcceptance { get; private set; }

    public DateTime? PublishedTime { get; private set; }

    public bool IsCurrent { get; private set; }

    public bool IsPublished => PublishedTime.HasValue;

    protected LegalDocument()
    {
    }

    public LegalDocument(Guid id, LegalDocumentType type, string language, string version, string title, string? content, string? externalUrl, bool requiresAcceptance)
        : base(id)
    {
        Type = type;
        Language = Check.NotNullOrWhiteSpace(language, nameof(language), AccountsConsts.MaxLanguageLength).ToLowerInvariant();
        Version = Check.NotNullOrWhiteSpace(version, nameof(version), AccountsConsts.MaxLegalVersionLength).Trim();
        SetContent(title, content, externalUrl, requiresAcceptance);
    }

    public void SetContent(string title, string? content, string? externalUrl, bool requiresAcceptance)
    {
        if (IsPublished)
        {
            throw new BusinessException(AccountsErrorCodes.LegalDocumentAlreadyPublished);
        }

        if (content.IsNullOrWhiteSpace() && externalUrl.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Se requiere contenido o una URL externa.", nameof(content));
        }

        Title = Check.NotNullOrWhiteSpace(title, nameof(title), AccountsConsts.MaxLegalTitleLength).Trim();
        Content = Check.Length(content, nameof(content), AccountsConsts.MaxLegalContentLength);
        ExternalUrl = Check.Length(externalUrl, nameof(externalUrl), AccountsConsts.MaxLegalUrlLength);
        RequiresAcceptance = requiresAcceptance;
    }

    /// <summary>Solo desde <see cref="LegalDocumentManager"/>, que retira la versión vigente anterior.</summary>
    internal void Publish(DateTime now)
    {
        if (IsPublished)
        {
            throw new BusinessException(AccountsErrorCodes.LegalDocumentAlreadyPublished);
        }

        PublishedTime = now;
        IsCurrent = true;
    }

    internal void Supersede()
    {
        IsCurrent = false;
    }
}

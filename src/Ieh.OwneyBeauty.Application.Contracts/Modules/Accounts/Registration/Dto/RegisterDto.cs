using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.Registration.Dto;

public class RegisterDto : IValidatableObject
{
    [EmailAddress, StringLength(AccountsConsts.MaxEmailLength)]
    public string? Email { get; set; }

    /// <summary>Formato E.164, p. ej. +18095551234.</summary>
    [RegularExpression(AccountsConsts.PhoneRegex), StringLength(AccountsConsts.MaxPhoneLength)]
    public string? PhoneNumber { get; set; }

    /// <summary>Opcional. Si no se envía se genera a partir del correo/teléfono.</summary>
    [StringLength(AccountsConsts.MaxUserNameLength, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9._]+$")]
    public string? UserName { get; set; }

    [Required, StringLength(AccountsConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [StringLength(AccountsConsts.MaxNameLength)]
    public string? Surname { get; set; }

    [Required, StringLength(AccountsConsts.MaxPasswordLength, MinimumLength = AccountsConsts.MinPasswordLength)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    /// <summary>Cliente o Profesional (Negocio llega en V2).</summary>
    public AccountMode InitialMode { get; set; } = AccountMode.Client;

    /// <summary>Ids de los documentos legales vigentes que el usuario acepta.</summary>
    public List<Guid> AcceptedLegalDocumentIds { get; set; } = new();

    public bool MarketingConsent { get; set; }

    /// <summary>Idioma preferido (es/en) para comunicaciones y documentos legales.</summary>
    [StringLength(AccountsConsts.MaxLanguageLength)]
    public string Language { get; set; } = "es";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber))
        {
            yield return new ValidationResult("Email o teléfono requerido.", new[] { nameof(Email), nameof(PhoneNumber) });
        }
    }
}

using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace Ieh.OwneyBeauty.SharedKernel;

/// <summary>
/// Importe monetario con moneda ISO 4217. Inmutable. Nunca usar float/double para dinero (ADR-05).
/// </summary>
public sealed class Money : ValueObject
{
    public const int CurrencyLength = 3;
    public const string DefaultCurrency = "DOP";

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    private Money()
    {
        Currency = DefaultCurrency;
    }

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        Check.NotNullOrWhiteSpace(currency, nameof(currency), CurrencyLength, CurrencyLength);
        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = DefaultCurrency) => new(0m, currency);

    public bool IsZero => Amount == 0m;

    public bool IsNegative => Amount < 0m;

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    /// <summary>Aplica un porcentaje expresado como fracción (0.18 = 18 %).</summary>
    public Money Percentage(decimal rate) => Multiply(rate);

    public static Money operator +(Money a, Money b) => a.Add(b);

    public static Money operator -(Money a, Money b) => a.Subtract(b);

    private void EnsureSameCurrency(Money other)
    {
        Check.NotNull(other, nameof(other));
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new BusinessException(OwneyErrorCodes.Shared.CurrencyMismatch)
                .WithData("left", Currency)
                .WithData("right", other.Currency);
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:0.00}";
}

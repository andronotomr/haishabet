namespace Haishabet.Domain;

/// <summary>
/// Valor monetário em centavos (BRL). Imutável.
/// Sempre usar centavos para evitar arredondamento de float/double com dinheiro.
/// </summary>
public readonly record struct Money(long Cents)
{
    public static readonly Money Zero = new(0);

    public static Money FromReais(decimal reais) => new((long)Math.Round(reais * 100m, MidpointRounding.AwayFromZero));
    public static Money FromCents(long cents) => new(cents);

    public decimal Reais => Cents / 100m;

    public bool IsZero => Cents == 0;
    public bool IsPositive => Cents > 0;
    public bool IsNegative => Cents < 0;

    public override string ToString() => $"R$ {Reais.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}";

    public static Money operator +(Money a, Money b) => new(a.Cents + b.Cents);
    public static Money operator -(Money a, Money b) => new(a.Cents - b.Cents);
    public static Money operator *(Money m, long factor) => new(m.Cents * factor);
    public static Money operator -(Money m) => new(-m.Cents);
    public static bool operator >(Money a, Money b) => a.Cents > b.Cents;
    public static bool operator <(Money a, Money b) => a.Cents < b.Cents;
    public static bool operator >=(Money a, Money b) => a.Cents >= b.Cents;
    public static bool operator <=(Money a, Money b) => a.Cents <= b.Cents;
}

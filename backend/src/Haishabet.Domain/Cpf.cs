namespace Haishabet.Domain;

/// <summary>
/// CPF brasileiro normalizado para 11 dígitos. Valida formato e checksum.
/// </summary>
public readonly record struct Cpf
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Cpf Parse(string raw)
    {
        if (TryParse(raw, out var cpf)) return cpf;
        throw new ArgumentException($"CPF inválido: {raw}", nameof(raw));
    }

    public static bool TryParse(string? raw, out Cpf cpf)
    {
        cpf = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length != 11) return false;

        // Sequências como 11111111111 são inválidas
        if (digits.Distinct().Count() == 1) return false;

        // Validação dos dois dígitos verificadores
        if (!IsChecksumValid(digits)) return false;

        cpf = new Cpf(digits);
        return true;
    }

    private static bool IsChecksumValid(string digits)
    {
        int sum1 = 0;
        for (int i = 0; i < 9; i++) sum1 += (digits[i] - '0') * (10 - i);
        int dig1 = (sum1 * 10) % 11;
        if (dig1 == 10) dig1 = 0;
        if (dig1 != digits[9] - '0') return false;

        int sum2 = 0;
        for (int i = 0; i < 10; i++) sum2 += (digits[i] - '0') * (11 - i);
        int dig2 = (sum2 * 10) % 11;
        if (dig2 == 10) dig2 = 0;
        return dig2 == digits[10] - '0';
    }

    public string Formatted() => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    public override string ToString() => Value;
}

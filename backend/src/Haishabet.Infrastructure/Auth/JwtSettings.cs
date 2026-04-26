namespace Haishabet.Infrastructure.Auth;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "haishabet";
    public string Audience { get; set; } = "haishabet-app";
    public string AccessSecret { get; set; } = "";
    public string RefreshSecret { get; set; } = "";
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 30;
}

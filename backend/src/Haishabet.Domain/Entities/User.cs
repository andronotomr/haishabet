namespace Haishabet.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; init; }
    public required string PasswordHash { get; set; }
    public required Cpf Cpf { get; init; }
    public required string FullName { get; init; }
    public required DateOnly BirthDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    public int AgeYears(DateOnly today)
    {
        int age = today.Year - BirthDate.Year;
        if (BirthDate > today.AddYears(-age)) age--;
        return age;
    }

    public bool IsAdult(DateOnly today) => AgeYears(today) >= 18;
}

public enum UserStatus
{
    Active = 0,
    Blocked = 1,
    SelfExcluded = 2,
    Closed = 3,
}

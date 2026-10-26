namespace StajSistemi.Domain.Entities;

public class LoginAttempt
{
    public int Id { get; set; }
    public string UserNameOrEmail { get; set; } = "";
    public DateTime AttemptedAt { get; set; }
    public bool Success { get; set; }
    public string? IpAddress { get; set; }
}
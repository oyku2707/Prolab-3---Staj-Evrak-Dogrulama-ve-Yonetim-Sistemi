using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Entities;

public abstract class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; private set; } = "";
    public int FailedLoginCount { get; private set; }
    public bool IsLocked { get; private set; }
    public abstract RoleType Role { get; }
}
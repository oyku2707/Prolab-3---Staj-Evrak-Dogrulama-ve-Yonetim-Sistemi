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


    public const int MaxFailedLogins = 5;

    public void SetPasswordHash(string hash)
    {
        PasswordHash = hash;
    }

    public void RegisterFailedLogin()
    {
        FailedLoginCount++;
        if (FailedLoginCount >= MaxFailedLogins)
        {
            IsLocked = true;
        }
    }

    public void ResetFailedLogins()
    {
        FailedLoginCount = 0;
    }

    public void Unlock()
    {
        IsLocked = false;
        FailedLoginCount = 0;
    }
}
namespace StajSistemi.Services;

// Kişi 1'in girişi hazır olana kadar kullanılan geçici servis.
public class FakeCurrentUserService : ICurrentUserService
{
    private readonly bool _isAdmin;

    public FakeCurrentUserService(IConfiguration configuration)
    {
        _isAdmin = configuration.GetValue<bool>("FakeUser:IsAdmin");
    }

    // dev-ogrenci Id = 1, dev-admin Id = 2
    public int? UserId => _isAdmin ? 2 : 1;
    public bool IsAdmin => _isAdmin;
}
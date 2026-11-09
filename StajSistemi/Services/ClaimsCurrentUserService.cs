using System.Security.Claims;

namespace StajSistemi.Services;

// Giriş yapan kullanıcıyı cookie içindeki claim'lerden okur (Kişi 1'in sözleşmesi).
public class ClaimsCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public ClaimsCurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAdmin => Principal?.IsInRole("ROLE_ADMIN") ?? false;
}
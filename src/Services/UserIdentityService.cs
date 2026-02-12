using Microsoft.AspNetCore.Components.Authorization;

namespace InvoiceGenerator.Services;

public class UserIdentityService : IUserIdentityService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public UserIdentityService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<string?> GetCurrentUserId()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
}

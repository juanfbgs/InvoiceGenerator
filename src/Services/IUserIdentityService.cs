namespace InvoiceGenerator.Services;

public interface IUserIdentityService
{
    Task<string?> GetCurrentUserId();
}
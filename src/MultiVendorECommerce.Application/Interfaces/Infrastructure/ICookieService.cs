namespace MultiVendorECommerce.Application.Interfaces.Infrastructure;

public interface ICookieService
{
    void SetCookie(string key, string value, int? expireTime = null);
    string? GetCookie(string key);
    void DeleteCookie(string key);
}
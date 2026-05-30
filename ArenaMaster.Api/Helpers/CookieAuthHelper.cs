namespace ArenaMaster.Api.Helpers;

public static class CookieAuthHelper
{
    public const string AccessCookie = "access_token";
    public const string RefreshCookie = "refresh_token";

    public static void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken, bool isDevelopment)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        response.Cookies.Append(AccessCookie, accessToken, new CookieOptions(cookieOptions)
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions(cookieOptions)
        {
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public static void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete(AccessCookie);
        response.Cookies.Delete(RefreshCookie);
    }
}

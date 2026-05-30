using System.Security.Claims;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.DTOs.Auth;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Models;
using ArenaMaster.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArenaMaster.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/refresh", Refresh);
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapGet("/confirm-email", ConfirmEmail);
        group.MapGet("/oauth/{provider}", InitiateOAuth);
        group.MapGet("/oauth/{provider}/callback", OAuthCallback);
    }

    private static async Task<IResult> Register(
        RegisterRequest req,
        AppDbContext db,
        EmailSender email,
        IValidator<RegisterRequest> validator,
        UnsplashClient unsplash,
        SmtpSettings smtp)
    {
        var validation = await validator.ValidateAsync(req);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        if (await db.Users.AnyAsync(u => u.Email == req.Email || u.Username == req.Username))
            return Results.BadRequest(new { message = "Email або нікнейм вже зайняті" });

        var userId = Guid.NewGuid();
        var token = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Id = userId,
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, 12),
            Role = "player",
            EmailConfirmed = string.IsNullOrWhiteSpace(smtp.User),
            EmailConfirmToken = string.IsNullOrWhiteSpace(smtp.User) ? null : token,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var avatarUrl = await unsplash.DownloadAndSaveAsync("avatars", userId, "gaming portrait avatar");
        user.AvatarUrl = avatarUrl ?? PlaceholderImageGenerator.WriteUserAvatar(req.Username, userId);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        if (string.IsNullOrWhiteSpace(smtp.User))
            return Results.Ok(new { message = "Реєстрація успішна. Email підтверджено автоматично." });

        await email.SendConfirmEmailAsync(user.Email, token);
        return Results.Ok(new { message = "Реєстрація успішна. Перевірте email для підтвердження." });
    }

    private static async Task<IResult> Login(
        LoginRequest req,
        AppDbContext db,
        JwtHelper jwt,
        HttpContext ctx,
        IValidator<LoginRequest> validator,
        IWebHostEnvironment env)
    {
        var validation = await validator.ValidateAsync(req);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null || user.IsBlocked || user.PasswordHash is null ||
            !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Results.Unauthorized();

        if (!user.EmailConfirmed)
            return Results.BadRequest(new { message = "Підтвердіть email перед входом" });

        return await IssueTokens(user, db, jwt, ctx, env.IsDevelopment());
    }

    private static async Task<IResult> Refresh(AppDbContext db, JwtHelper jwt, HttpContext ctx, IWebHostEnvironment env)
    {
        var refreshToken = ctx.Request.Cookies[CookieAuthHelper.RefreshCookie];
        if (string.IsNullOrEmpty(refreshToken))
            return Results.Unauthorized();

        var stored = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored is null)
            return Results.Unauthorized();

        if (stored.Revoked)
        {
            var allUserTokens = await db.RefreshTokens.Where(r => r.UserId == stored.UserId).ToListAsync();
            foreach (var t in allUserTokens) t.Revoked = true;
            await db.SaveChangesAsync();
            CookieAuthHelper.ClearAuthCookies(ctx.Response);
            return Results.Unauthorized();
        }

        if (stored.ExpiresAt < DateTime.UtcNow)
            return Results.Unauthorized();

        stored.Revoked = true;
        return await IssueTokens(stored.User, db, jwt, ctx, env.IsDevelopment());
    }

    private static async Task<IResult> Logout(AppDbContext db, HttpContext ctx, ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();
        if (userId is null) return Results.Unauthorized();

        var refreshToken = ctx.Request.Cookies[CookieAuthHelper.RefreshCookie];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (stored is not null) stored.Revoked = true;
            await db.SaveChangesAsync();
        }

        CookieAuthHelper.ClearAuthCookies(ctx.Response);
        return Results.Ok(new { message = "Вихід виконано" });
    }

    private static async Task<IResult> ConfirmEmail([FromQuery] string token, AppDbContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.EmailConfirmToken == token);
        if (user is null) return Results.BadRequest(new { message = "Невірний токен" });

        user.EmailConfirmed = true;
        user.EmailConfirmToken = null;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Email підтверджено" });
    }

    private static IResult InitiateOAuth(string provider, HttpContext ctx, IConfiguration config)
    {
        var (scheme, clientIdKey) = provider.ToLower() switch
        {
            "google" => ("Google", "GOOGLE_CLIENT_ID"),
            "discord" => ("Discord", "DISCORD_CLIENT_ID"),
            _ => (null, null)
        };
        if (scheme is null) return Results.BadRequest(new { message = "Невідомий провайдер" });
        if (string.IsNullOrEmpty(config[clientIdKey]))
            return Results.BadRequest(new { message = $"Провайдер {provider} не налаштований. Додайте {clientIdKey} в змінні середовища." });

        return Results.Challenge(new AuthenticationProperties { RedirectUri = $"/api/auth/oauth/{provider}/callback" }, [scheme]);
    }

    private static async Task<IResult> OAuthCallback(
        string provider,
        AppDbContext db,
        JwtHelper jwt,
        HttpContext ctx,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        var scheme = provider.ToLower() switch
        {
            "google" => "Google",
            "discord" => "Discord",
            _ => null
        };
        if (scheme is null || string.IsNullOrEmpty(config[$"{provider.ToUpper()}_CLIENT_ID"]))
            return Results.Redirect($"{config["FRONTEND_URL"]}/login?error=oauth_not_configured");

        var result = await ctx.AuthenticateAsync(scheme);

        if (!result.Succeeded)
            return Results.Redirect($"{config["FRONTEND_URL"]}/login?error=oauth");

        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var providerUserId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = result.Principal?.FindFirstValue(ClaimTypes.Name) ?? "player";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerUserId))
            return Results.Redirect($"{config["FRONTEND_URL"]}/login?error=oauth");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            var username = await GenerateUsername(db, name);
            user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = null,
                Role = "player",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
        }

        var oauthExists = await db.OAuthAccounts.AnyAsync(o =>
            o.Provider == provider && o.ProviderUserId == providerUserId);
        if (!oauthExists)
        {
            db.OAuthAccounts.Add(new OAuthAccount
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = provider,
                ProviderUserId = providerUserId
            });
        }

        await db.SaveChangesAsync();
        await IssueTokens(user, db, jwt, ctx, env.IsDevelopment());
        return Results.Redirect($"{config["FRONTEND_URL"]}/");
    }

    private static async Task<IResult> IssueTokens(User user, AppDbContext db, JwtHelper jwt, HttpContext ctx, bool isDev)
    {
        var access = jwt.GenerateAccessToken(user);
        var refresh = JwtHelper.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Revoked = false
        });
        await db.SaveChangesAsync();

        CookieAuthHelper.SetAuthCookies(ctx.Response, access, refresh, isDev);
        return Results.Ok(new AuthUserDto(user.Id, user.Username, user.Email, user.Role, user.AvatarUrl, user.EmailConfirmed));
    }

    private static async Task<string> GenerateUsername(AppDbContext db, string name)
    {
        var baseName = SlugHelper.Generate(name).Replace("-", "_");
        if (baseName.Length < 3) baseName = "player";
        return await SlugHelper.EnsureUniqueAsync(
            async s => await db.Users.AnyAsync(u => u.Username == s),
            baseName.Length > 50 ? baseName[..50] : baseName);
    }
}

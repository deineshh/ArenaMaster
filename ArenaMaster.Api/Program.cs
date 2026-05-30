using System.Text;
using ArenaMaster.Api.Data;
using ArenaMaster.Api.Data.Seeders;
using ArenaMaster.Api.Endpoints;
using ArenaMaster.Api.Helpers;
using ArenaMaster.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=arenamaster;Username=postgres;Password=postgres";

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource));

var jwtSettings = new JwtSettings
{
    Secret = builder.Configuration["JWT_SECRET"] ?? "dev-secret-key-min-32-chars-long!!",
    Issuer = builder.Configuration["JWT_ISSUER"] ?? "arenamaster.ua",
    Audience = builder.Configuration["JWT_AUDIENCE"] ?? "arenamaster.ua"
};
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton(new SmtpSettings
{
    Host = builder.Configuration["SMTP_HOST"] ?? "smtp.gmail.com",
    Port = int.TryParse(builder.Configuration["SMTP_PORT"], out var p) ? p : 587,
    User = builder.Configuration["SMTP_USER"] ?? "",
    Pass = builder.Configuration["SMTP_PASS"] ?? ""
});
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<UnsplashClient>();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(CookieAuthHelper.AccessCookie, out var token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

var googleClientId = builder.Configuration["GOOGLE_CLIENT_ID"];
if (!string.IsNullOrEmpty(googleClientId))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = builder.Configuration["GOOGLE_CLIENT_SECRET"] ?? "";
        options.CallbackPath = "/api/auth/oauth/google/callback";
    });
}

var discordClientId = builder.Configuration["DISCORD_CLIENT_ID"];
if (!string.IsNullOrEmpty(discordClientId))
{
    authBuilder.AddDiscord(options =>
    {
        options.ClientId = discordClientId;
        options.ClientSecret = builder.Configuration["DISCORD_CLIENT_SECRET"] ?? "";
        options.CallbackPath = "/api/auth/oauth/discord/callback";
    });
}

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var frontendUrl = builder.Configuration["FRONTEND_URL"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "uploads"));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    for (var i = 0; i < 10; i++)
    {
        try
        {
            await db.Database.MigrateAsync();
            break;
        }
        catch (Npgsql.NpgsqlException ex) when (i < 9)
        {
            logger.LogWarning(ex, "Database not ready, retrying in 2s... ({Attempt}/10)", i + 1);
            await Task.Delay(2000);
        }
    }
    if (app.Environment.IsDevelopment())
        await DataSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapTeamEndpoints();
app.MapTournamentEndpoints();
app.MapMatchEndpoints();
app.MapNotificationEndpoints();
app.MapDisciplineEndpoints();
app.MapAdminEndpoints();

app.MapGet("/", () => Results.Ok(new { name = "ArenaMaster API", version = "1.0" }));

app.Run();

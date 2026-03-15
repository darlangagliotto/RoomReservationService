using AuthService.Application.DependencyInjection;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.DependencyInjection;
using AuthService.Infrastructure.Security;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using System.Net.Sockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("Configurações Jwt não encontrada");

jwtOptions.Validate(); 

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Use: Bearer {seu_token}"
    });

});

builder.Services.AddFluentValidationAutoValidation();
var app = builder.Build();


// ------------------------
// Middleware
// ------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<AuthService.API.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await MigrateDatabaseWithRetryAsync(app);

app.Run();

static async Task MigrateDatabaseWithRetryAsync(WebApplication app)
{
    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            await db.Database.MigrateAsync();
            return;
        }
        catch (Exception ex) when (IsTransientDatabaseStartupFailure(ex) && attempt < maxRetries)
        {
            app.Logger.LogWarning(
                ex,
                "Database unavailable during startup. Retrying migration in {DelaySeconds}s ({Attempt}/{MaxRetries}).",
                delay.TotalSeconds,
                attempt,
                maxRetries);

            await Task.Delay(delay);
        }
    }

    using var finalScope = app.Services.CreateScope();
    var finalDb = finalScope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await finalDb.Database.MigrateAsync();
}

static bool IsTransientDatabaseStartupFailure(Exception exception)
{
    return exception switch
    {
        NpgsqlException => true,
        SocketException => true,
        _ when exception.InnerException is not null => IsTransientDatabaseStartupFailure(exception.InnerException),
        _ => false
    };
}
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PcComponentsApi.Data;
using PcComponentsApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — 1 политика со всеми доменами
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins(
                "https://project-site.vercel.app",
                "http://localhost:5173",
                "http://localhost:5124"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Автовыбор БД: SQLite или PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString?.Contains("Data Source=") == true || connectionString?.EndsWith(".db") == true)
{
    // SQLite для локальной разработки
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=app.db"));
}
else if (!string.IsNullOrEmpty(connectionString))
{
    // PostgreSQL для продакшена (Supabase/Render)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Fallback на SQLite, если ничего не указано
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=app.db"));
}

// JWT аутентификация
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Политики авторизации
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("StandardOnly", policy => policy.RequireRole("standard"));
});

// Билд приложения
var app = builder.Build();

// Порт для Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware 
app.UseCors("AllowAll");  
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var appTask = app.RunAsync();
//  Инициализация БД 
async Task InitializeDatabaseAsync()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    if (!await db.Users.AnyAsync(u => u.Email == "admin@example.com"))
    {
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@example.com",
            PasswordHash = PasswordService.HashPassword("admin12345"),
            CreatedAt = DateTime.UtcNow,
            Role = "admin"
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
await InitializeDatabaseAsync();

await appTask;
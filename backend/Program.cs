using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// CORS — 1 политика со всеми доменами
builder.Services.AddCors(options =>
{
   options.AddPolicy("AllowAll", policy =>
    policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
);
});

// Автовыбор БД: SQLite или PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? builder.Configuration["DATABASE_URL"];

if (string.IsNullOrEmpty(connectionString))
{
    // Fallback на SQLite если нет переменной окружения
    connectionString = "Data Source=app.db";
}

// Определение типа БД
bool isSqlite = connectionString.Contains("Data Source=") && !connectionString.Contains("Host=");

// Если строка содержит "Host=", это точно не SQLite, даже если там нет префикса postgresql://
if (connectionString.Contains("Host=") || connectionString.Contains("Server="))
{
    isSqlite = false;
}

if (isSqlite)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
    Console.WriteLine("[DB] Using SQLite database.");
}
else
{
    // Для PostgreSQL
    string finalConnectionString = connectionString;

  
    try 
    {
        // распарсить через билдер для нормализации и добавления дефолтных настроек
        var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        
        //  доверие к сертификату 
        npgsqlBuilder.TrustServerCertificate = true;
        
        if (!npgsqlBuilder.ContainsKey("GssEncryptionMode"))
        {
             npgsqlBuilder.GssEncryptionMode = Npgsql.GssEncryptionMode.Disable;
        }

        finalConnectionString = npgsqlBuilder.ConnectionString;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB] Warning: Could not parse connection string with NpgsqlConnectionStringBuilder: {ex.Message}");
     
        if (!connectionString.Contains("Trust Server Certificate"))
        {
            finalConnectionString += ";Trust Server Certificate=true;GssEncryptionMode=Disable";
        }
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(finalConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));
    
    Console.WriteLine("[DB] Using PostgreSQL database.");
    Console.WriteLine($"[DB] Connection Host: {new NpgsqlConnectionStringBuilder(finalConnectionString).Host}");
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
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
var appTask = app.RunAsync();
//  Инициализация БД 

async Task InitializeDatabaseAsync()
{
    Console.WriteLine("[DB] Starting initialization...");
    
    int maxRetries = 5; 
    int retryDelayMs = 2000; 

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.SetCommandTimeout(120);

            Console.WriteLine($"[DB] Attempting to connect and migrate (Attempt {i + 1}/{maxRetries})...");
            

            bool canConnect = await db.Database.CanConnectAsync();
            if (!canConnect) throw new Exception("Cannot connect to database");

           
            var pending = await db.Database.GetPendingMigrationsAsync();

            if (pending.Any())
            {
                Console.WriteLine($"[DB] Applying {pending.Count()} migrations...");
                await db.Database.MigrateAsync();
                Console.WriteLine("[DB] Migrations applied successfully");
            }
            else
            {
                Console.WriteLine("[DB] No migrations needed");
            }

            // Создание тестового админа
            if (!await db.Users.AnyAsync(u => u.Email == "admin@example.com"))
            {
                var admin = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = PasswordService.HashPassword("admin12345"),
                    CreatedAt = DateTime.UtcNow,
                    Role = "admin"
                };
                db.Users.Add(admin);
                await db.SaveChangesAsync();
                Console.WriteLine("[DB] Admin user created");
            }

            Console.WriteLine("[DB] Initialization completed successfully.");
            return; // Выход из функции, если все успешно

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB] ERROR on attempt {i + 1}: {ex.Message}");
            if (i == maxRetries - 1)
            {
                Console.WriteLine("[DB] Max retries reached. Crashing application.");
                throw;
            }
            
            Console.WriteLine($"[DB] Retrying in {retryDelayMs}ms...");
            await Task.Delay(retryDelayMs);
        }
    }
}
await InitializeDatabaseAsync();

await appTask;


/*async Task InitializeDatabaseAsync()
{
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.SetCommandTimeout(60);
            await db.Database.MigrateAsync();

            // Создание тестового админа 
            if (!await db.Users.AnyAsync(u => u.Email == "admin@example.com"))
            {
                var admin = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin",  
                    Email = "admin@example.com",
                    PasswordHash = PasswordService.HashPassword("admin12345"),
                    CreatedAt = DateTime.UtcNow,
                    Role = "admin"
                };
                db.Users.Add(admin);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEV] DB init warning: {ex.Message}");
        }
    }

}*/
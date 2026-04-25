using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using System.Text;
using Npgsql;

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
    connectionString = "Data Source=app.db";
}

bool isSqlite = !connectionString.Contains("Host=") && !connectionString.Contains("Server=");

if (isSqlite)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
    Console.WriteLine("[DB] Using SQLite database.");
}
else
{
    string finalConnectionString = connectionString;
    try
    {
        var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        npgsqlBuilder.TrustServerCertificate = true;
        if (!npgsqlBuilder.ContainsKey("GssEncryptionMode"))
        {
            npgsqlBuilder.GssEncryptionMode = Npgsql.GssEncryptionMode.Disable;
        }
        finalConnectionString = npgsqlBuilder.ConnectionString;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB] Warning: Could not parse connection string: {ex.Message}");
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
}

// JWT аутентификация
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt__Key"]
                 ?? builder.Configuration["Jwt:Key"]
                 ?? "SuperSecretKey12345SuperSecretKey12345";

    var key = Encoding.UTF8.GetBytes(jwtKey);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Политики авторизации
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("StandardOnly", policy => policy.RequireRole("standard"));
});

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

//Инициализация БД
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
            Console.WriteLine($"[DB] Attempting to connect (Attempt {i + 1}/{maxRetries})...");

            bool canConnect = await db.Database.CanConnectAsync();
            if (!canConnect) throw new Exception("Cannot connect to database");

            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                Console.WriteLine($"[DB] Applying {pending.Count()} migrations...");
                await db.Database.MigrateAsync();
                Console.WriteLine("[DB] Migrations applied successfully");
            }

            // Seed данных (только если таблица пуста)
            if (!await db.Components.AnyAsync())
            {
                Console.WriteLine("[DB] Seeding components...");
                db.Components.AddRange(
                    // --- Intel Platform ---
                    new Component
                    {
                        Id = "cpu-intel-1",
                        Name = "Intel Core i9-13900K",
                        Category = ComponentCategory.Processor,
                        Price = 580,
                        Currency = "$",
                        Specifications = "LGA1700, 24 cores, 32 threads",
                        Socket = "LGA1700",
                        PowerConsumption = 253
                    },
                    new Component
                    {
                        Id = "mobo-intel-1",
                        Name = "ASUS ROG Maximus Z790 Hero",
                        Category = ComponentCategory.Motherboard,
                        Price = 600,
                        Currency = "$",
                        Specifications = "ATX, DDR5 Support, LGA1700",
                        Socket = "LGA1700"
                    },

                    // --- AMD Platform (для проверки несовместимости) ---
                    new Component
                    {
                        Id = "cpu-amd-1",
                        Name = "AMD Ryzen 9 7950X",
                        Category = ComponentCategory.Processor,
                        Price = 550,
                        Currency = "$",
                        Specifications = "AM5, 16 cores",
                        Socket = "AM5",
                        PowerConsumption = 170
                    },
                    new Component
                    {
                        Id = "mobo-amd-1",
                        Name = "Gigabyte X670 Aorus Elite",
                        Category = ComponentCategory.Motherboard,
                        Price = 300,
                        Currency = "$",
                        Specifications = "ATX, DDR5 Support, AM5",
                        Socket = "AM5"
                    },

                    // --- RAM (DDR4 и DDR5) ---
                    new Component
                    {
                        Id = "ram-ddr5-1",
                        Name = "Kingston Fury Beast 32GB DDR5",
                        Category = ComponentCategory.Ram,
                        Price = 140,
                        Currency = "$",
                        Specifications = "DDR5 6000MHz"
                    },
                    new Component
                    {
                        Id = "ram-ddr4-1",
                        Name = "Corsair Vengeance 32GB DDR4",
                        Category = ComponentCategory.Ram,
                        Price = 90,
                        Currency = "$",
                        Specifications = "DDR4 3200MHz"
                    },

                    // --- GPU ---
                    new Component
                    {
                        Id = "gpu-nvidia-1",
                        Name = "NVIDIA GeForce RTX 4090",
                        Category = ComponentCategory.Videocard,
                        Price = 1600,
                        Currency = "$",
                        Specifications = "24GB GDDR6X",
                        PowerConsumption = 450
                    },
                    new Component
                    {
                        Id = "gpu-amd-1",
                        Name = "AMD Radeon RX 7900 XTX",
                        Category = ComponentCategory.Videocard,
                        Price = 900,
                        Currency = "$",
                        Specifications = "24GB GDDR6",
                        PowerConsumption = 355
                    },

                    // --- Storage & Cooling ---
                    new Component
                    {
                        Id = "storage-1",
                        Name = "Samsung 980 PRO 1TB",
                        Category = ComponentCategory.Storage,
                        Price = 100,
                        Currency = "$",
                        Specifications = "NVMe M.2 SSD"
                    },
                    new Component
                    {
                        Id = "cooling-1",
                        Name = "DeepCool AK620",
                        Category = ComponentCategory.Cooling,
                        Price = 65,
                        Currency = "$",
                        Specifications = "Air Cooler, 260W TDP"
                    }
                );
                await db.SaveChangesAsync();
                Console.WriteLine(">>> Database seeded with test components (Intel/AMD/DDR4/DDR5)!");
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
                Console.WriteLine("[DB] Admin user created (admin@example.com / admin12345)");
            }

            Console.WriteLine("[DB] Initialization completed successfully.");
            return;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB] ERROR on attempt {i + 1}: {ex.Message}");
            if (i == maxRetries - 1)
            {
                Console.WriteLine("[DB] Max retries reached. Crashing application.");
                throw;
            }
            await Task.Delay(retryDelayMs);
        }
    }
}

// Запуск инициализации перед стартом приложения
await InitializeDatabaseAsync();
await app.RunAsync();
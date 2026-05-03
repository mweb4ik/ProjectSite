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


// ================= CORS =================

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ================= DB =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));


// ================= JWT =================
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            if (context.Request.Method == "OPTIONS") context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("StandardOnly", policy => policy.RequireRole("standard"));
});

var app = builder.Build();

// ================= PIPELINE =================
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

app.MapControllers();
app.MapGet("/health", () => "OK");


// ================= INIT METHOD =================

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
                        PowerConsumption = 253,
                        ImageUrl = "wwwroot/images/IntelCorei9-13-900K.png"
                    },
                    new Component
                    {
                        Id = "mobo-intel-1",
                        Name = "ASUS ROG Maximus Z790 Hero",
                        Category = ComponentCategory.Motherboard,
                        Price = 600,
                        Currency = "$",
                        Specifications = "ATX, DDR5 Support, LGA1700",
                        Socket = "LGA1700",
                        ImageUrl = "wwwroot/images/ASUS-ROG-Maximus-Z790-Hero.png"
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
                        PowerConsumption = 170,
                        ImageUrl = "wwwroot/images/Ryzen_9_3950X_4.jpg"
                    },
                    new Component
                    {
                        Id = "mobo-amd-1",
                        Name = "Gigabyte X670 Aorus Elite",
                        Category = ComponentCategory.Motherboard,
                        Price = 300,
                        Currency = "$",
                        Specifications = "ATX, DDR5 Support, AM5",
                        Socket = "AM5",
                        ImageUrl = "wwwroot/images/Gigabyte-X670-Aorus-Elite.jpg"
                    },

                    // --- RAM (DDR4 и DDR5) ---
                    new Component
                    {
                        Id = "ram-ddr5-1",
                        Name = "Kingston Fury Beast 32GB DDR5",
                        Category = ComponentCategory.Ram,
                        Price = 140,
                        Currency = "$",
                        Specifications = "DDR5 6000MHz",
                        ImageUrl = "wwwroot/images/Kingston-FuryBeast-32GB-DDR5.png"
                    },
                    new Component
                    {
                        Id = "ram-ddr4-1",
                        Name = "Corsair Vengeance 32GB DDR4",
                        Category = ComponentCategory.Ram,
                        Price = 90,
                        Currency = "$",
                        Specifications = "DDR4 3200MHz",
                        ImageUrl = "wwwroot/images/Corsair-Vengeance-32GB-DDR4.png"
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
                        PowerConsumption = 450,
                        ImageUrl = "wwwroot/images/NVIDIA-GeForce-RTX-4090.png"
                    },
                    new Component
                    {
                        Id = "gpu-amd-1",
                        Name = "AMD Radeon RX 7900 XTX",
                        Category = ComponentCategory.Videocard,
                        Price = 900,
                        Currency = "$",
                        Specifications = "24GB GDDR6",
                        PowerConsumption = 355,
                        ImageUrl = "wwwroot/images/AMD-Radeon-RX7900-XTX.png"
                    },

                    // --- Storage & Cooling ---
                    new Component
                    {
                        Id = "storage-1",
                        Name = "Samsung 980 PRO 1TB",
                        Category = ComponentCategory.Storage,
                        Price = 100,
                        Currency = "$",
                        Specifications = "NVMe M.2 SSD",
                        ImageUrl = "wwwroot/images/Samsung-980-PRO-1TB.png"
                    },
                    new Component
                    {
                        Id = "cooling-1",
                        Name = "DeepCool AK620",
                        Category = ComponentCategory.Cooling,
                        Price = 65,
                        Currency = "$",
                        Specifications = "Air Cooler, 260W TDP",
                        ImageUrl = "wwwroot/images/DeepCool-620AK.png"
                    }
                );
                await db.SaveChangesAsync();
                Console.WriteLine(">>> Database seeded with test components (Intel/AMD/DDR4/DDR5)!");
            }

            //Создание материнской платы
if (!await db.Set<Motherboard>().AnyAsync())
{
    db.Set<Motherboard>().AddRange(
        new Motherboard
        {
            Id = "mobo-intel-1",
            Name = "ASUS ROG Maximus Z790 Hero",
            Socket = "LGA1700",
            Chipset = "Z790"
        },
        new Motherboard
        {
            Id = "mobo-amd-1",
            Name = "Gigabyte X670 Aorus Elite",
            Socket = "AM5",
            Chipset = "X670"
        }
    );

    await db.SaveChangesAsync();
}
            //Вопросы квиза
            if (!await db.QuizQuestions.AnyAsync())
        {
            Console.WriteLine("[DB] Seeding quiz questions...");

            db.QuizQuestions.AddRange(
        new QuizQuestion
        {
            Question = "Какой сокет у Intel i9-13900K?",
            Options = new[] { "AM4", "LGA1700", "AM5", "LGA1200" },
            CorrectOptionIndex = 1,
            Difficulty = "easy"
        },
        new QuizQuestion
        {
            Question = "Что важнее для стабильного разгона?",
            Options = new[] { "RGB подсветка", "Напряжение", "Цвет корпуса", "SSD" },
            CorrectOptionIndex = 1,
            Difficulty = "medium"
        },
        new QuizQuestion
        {
            Question = "Что произойдет при слишком высоком напряжении?",
            Options = new[] {
                "Улучшится FPS",
                "Снизится температура",
                "Повреждение CPU",
                "Ничего"
            },
            CorrectOptionIndex = 2,
            Difficulty = "hard"
        },
        new QuizQuestion
        {
            Question = "Какой тип памяти у современных систем?",
            Options = new[] { "DDR3", "DDR4/DDR5", "SDRAM", "GDDR3" },
            CorrectOptionIndex = 1,
            Difficulty = "easy"
        }
    );

    await db.SaveChangesAsync();
}
//Создание BIOS
if (!await db.BiosVersions.AnyAsync())
{
    Console.WriteLine("[DB] Seeding BIOS versions...");

    db.BiosVersions.AddRange(
        new BiosVersion
        {
            Id = "bios-1",
            MotherboardId = "mobo-intel-1",
            Version = "F1",
            ReleaseDate = DateTime.UtcNow.AddMonths(-6),
            Description = "Initial release",
            Stability = 80,
            IsBeta = false
        },
        new BiosVersion
        {
            Id = "bios-2",
            MotherboardId = "mobo-intel-1",
            Version = "F2",
            ReleaseDate = DateTime.UtcNow.AddMonths(-3),
            Description = "Improved CPU support",
            Stability = 90,
            IsBeta = false
        }
    );

    await db.SaveChangesAsync();
}
//Добавление CpuSupport 
if (!await db.CpuSupports.AnyAsync())
{
    Console.WriteLine("[DB] Seeding CPU support...");

    db.CpuSupports.AddRange(
        new CpuSupport
        {
            CpuId = "cpu-intel-1",
            BiosVersionId = "bios-1",
            IsSupported = true
        },
        new CpuSupport
        {
            CpuId = "cpu-intel-1",
            BiosVersionId = "bios-2",
            IsSupported = true
        },
        new CpuSupport
        {
            CpuId = "cpu-amd-1",
            BiosVersionId = "bios-1",
            IsSupported = false
        }
    );

    await db.SaveChangesAsync();
}
//Тестовые результаты по усмолчанию 
if (!await db.QuizResults.AnyAsync())
{
    db.QuizResults.Add(new QuizResult
    {
        UserId = "test-user",
        Score = 2,
        TotalQuestions = 4,
        CompletedAt = DateTime.UtcNow
    });

    await db.SaveChangesAsync();
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
                    Role = "admin",
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
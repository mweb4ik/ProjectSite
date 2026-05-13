using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using System.Text.Json.Serialization;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);

// ==================== 1. CONTROLLERS + JSON ====================
builder.Services.AddControllers()
    .AddJsonOptions(opt => {
        opt.JsonSerializerOptions.PropertyNamingPolicy = null; 
        opt.JsonSerializerOptions.DictionaryKeyPolicy = null;
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==================== 2. CORS ====================
builder.Services.AddCors(opt => opt.AddPolicy("AllowFrontend", p =>
    p.WithOrigins(
        "http://localhost:5173",
        "https://pc-components-app.vercel.app",
       "https://projectsite-luml.onrender.com"
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));
// ==================== 3. DATABASE ====================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

// ==================== 4. JWT ====================
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    NameClaimType = ClaimTypes.Name,
    RoleClaimType = ClaimTypes.Role
};
});

// ==================== 5. AUTHORIZATION ====================
builder.Services.AddAuthorization(opt => {
    opt.AddPolicy("AdminOnly", p => p.RequireRole("admin"));
    opt.AddPolicy("StandardOnly", p => p.RequireRole("standard"));
});

builder.Services.AddScoped<PasswordService>();

var app = builder.Build();

// ==================== 6. PIPELINE ====================
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => "OK");

// ==================== 7. DATABASE INIT ====================
await InitializeDatabaseAsync(app);

app.Run(); 

// =================================================================
// 🔧 МЕТОД ИНИЦИАЛИЗАЦИИ БД (все проверки + async/await)
// =================================================================
async Task InitializeDatabaseAsync(WebApplication app)
{
    Console.WriteLine("[DB] Starting initialization...");
    
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    db.Database.EnsureCreated();
    
    // 🔹 Components с описанием взаимодействия
    if (!await db.Components.AnyAsync())
    {
        Console.WriteLine("[DB] Seeding components...");
        db.Components.AddRange(
            // === PROCESSOR (CPU) ===
            new Component { 
                Id = "cpu-intel-1", 
                Name = "Intel Core i9-13900K", 
                Category = ComponentCategory.Processor, 
                Price = 580, 
                Currency = "$", 
                Specifications = "LGA1700, 24 cores (8P+16E), 32 threads, до 5.8 GHz. Взаимодействует с материнской платой через сокет LGA1700, требует совместимого чипсета Z690/Z790. Работает с DDR4/DDR5 памятью через контроллер памяти на материнской плате. Интегрированная графика UHD 770 может работать без дискретной видеокарты.", 
                Socket = "LGA1700", 
                PowerConsumption = 253, 
                ImageUrl = "/images/IntelCorei9-13-900K.png" 
            },
            new Component { 
                Id = "cpu-amd-1", 
                Name = "AMD Ryzen 9 7950X", 
                Category = ComponentCategory.Processor, 
                Price = 550, 
                Currency = "$", 
                Specifications = "AM5, 16 cores, 32 threads, до 5.7 GHz. Взаимодействует с материнской платой через сокет AM5, требует чипсета X670/B650. Поддерживает только DDR5 память. Не имеет интегрированной графики (требуется дискретная видеокарта).", 
                Socket = "AM5", 
                PowerConsumption = 170, 
                ImageUrl = "/images/Ryzen_9_3950X_4.jpg" 
            },
            
            // === MOTHERBOARD ===
            new Component { 
                Id = "mobo-intel-1", 
                Name = "ASUS ROG Maximus Z790 Hero", 
                Category = ComponentCategory.Motherboard, 
                Price = 600, 
                Currency = "$", 
                Specifications = "ATX, LGA1700, чипсет Z790, 4 слота DDR5 (до 128GB, 7000+ MHz), 2x PCIe 5.0 x16 для GPU, 5x M.2 NVMe для накопителей. Взаимодействует: CPU через сокет LGA1700, RAM через слоты DDR5, GPU через PCIe x16, накопители через M.2/SATA, БП через 24-pin + 8-pin EPS.", 
                Socket = "LGA1700", 
                PowerConsumption = 50, 
                ImageUrl = "/images/ASUS-ROG-Maximus-Z790-Hero.png" 
            },
            new Component { 
                Id = "mobo-amd-1", 
                Name = "Gigabyte X670 Aorus Elite", 
                Category = ComponentCategory.Motherboard, 
                Price = 300, 
                Currency = "$", 
                Specifications = "ATX, AM5, чипсет X670, 4 слота DDR5 (до 128GB, 6666+ MHz), 2x PCIe 4.0/5.0 x16 для GPU, 4x M.2 NVMe. Взаимодействует: CPU через сокет AM5, RAM через слоты DDR5, GPU через PCIe x16, накопители через M.2/SATA.", 
                Socket = "AM5", 
                PowerConsumption = 45, 
                ImageUrl = "/images/Gigabyte-X670-Aorus-Elite.jpg" 
            },
            
            // === RAM (ОЗУ) ===
            new Component { 
                Id = "ram-ddr5-1", 
                Name = "Kingston Fury Beast 32GB DDR5", 
                Category = ComponentCategory.Ram, 
                Price = 140, 
                Currency = "$", 
                Specifications = "DDR5-6000, 32GB (2x16GB), CL36, 1.35V. Взаимодействует с материнской платой через слоты DDR5 (не совместима с DDR4). Процессор получает данные от RAM через контроллер памяти на материнской плате. Двухканальный режим повышает пропускную способность.", 
                Socket = "", 
                PowerConsumption = 10, 
                ImageUrl = "/images/Kingston-FuryBeast-32GB-DDR5.png" 
            },
            new Component { 
                Id = "ram-ddr4-1", 
                Name = "Corsair Vengeance 32GB DDR4", 
                Category = ComponentCategory.Ram, 
                Price = 90, 
                Currency = "$", 
                Specifications = "DDR4-3200, 32GB (2x16GB), CL16, 1.35V. Взаимодействует с материнской платой через слоты DDR4 (не совместима с DDR5). Требует материнскую плату с поддержкой DDR4.", 
                Socket = "", 
                PowerConsumption = 8, 
                ImageUrl = "/images/Corsair-Vengeance-32GB-DDR4.png" 
            },
            
            // === VIDEOCARD (GPU) ===
            new Component { 
                Id = "gpu-nvidia-1", 
                Name = "NVIDIA GeForce RTX 4090", 
                Category = ComponentCategory.Videocard, 
                Price = 1600, 
                Currency = "$", 
                Specifications = "24GB GDDR6X, 16384 CUDA ядер, boost до 2.52 GHz. Взаимодействует с материнской платой через PCIe x16 слот, требует 3x 8-pin или 1x 16-pin (12VHPWR) питание от БП. Выводит изображение через DisplayPort/HDMI на монитор. Требует БП от 850W.", 
                Socket = "", 
                PowerConsumption = 450, 
                ImageUrl = "/images/NVIDIA-GeForce-RTX-4090.png" 
            },
            new Component { 
                Id = "gpu-amd-1", 
                Name = "AMD Radeon RX 7900 XTX", 
                Category = ComponentCategory.Videocard, 
                Price = 900, 
                Currency = "$", 
                Specifications = "24GB GDDR6, 6144 потоковых процессоров, boost до 2.5 GHz. Взаимодействует через PCIe x16, требует 2x 8-pin питание от БП. Поддержка DisplayPort 2.1 и HDMI 2.1. Требует БП от 750W.", 
                Socket = "", 
                PowerConsumption = 355, 
                ImageUrl = "/images/AMD-Radeon-RX7900-XTX.png" 
            },
            
            // === STORAGE (Накопитель) ===
            new Component { 
                Id = "storage-1", 
                Name = "Samsung 980 PRO 1TB", 
                Category = ComponentCategory.Storage, 
                Price = 100, 
                Currency = "$", 
                Specifications = "NVMe M.2 2280, PCIe 4.0 x4, чтение до 7000 MB/s, запись до 5100 MB/s. Взаимодействует с материнской платой через M.2 слот (ключ M), подключается напрямую к CPU или чипсету. Требует поддержки NVMe в BIOS/UEFI.", 
                Socket = "", 
                PowerConsumption = 7, 
                ImageUrl = "/images/Samsung-980-PRO-1TB.png" 
            },
            
            // === COOLING (Охлаждение) ===
            new Component { 
                Id = "cooling-1", 
                Name = "DeepCool AK620", 
                Category = ComponentCategory.Cooling, 
                Price = 65, 
                Currency = "$", 
                Specifications = "Башенный кулер, 2 вентилятора 120mm, TDP 260W. Взаимодействует с CPU через крепление на сокет (LGA1700/AM5 в комплекте). Отводит тепло от процессора через тепловые трубки к радиатору. Требует совместимого крепления и достаточного пространства в корпусе.", 
                Socket = "", 
                PowerConsumption = 5, 
                ImageUrl = "/images/DeepCool-620AK.png" 
            }
        );
        await db.SaveChangesAsync();
    }

    // 🔹 Motherboards
    if (!await db.Set<Motherboard>().AnyAsync())
    {
        db.Set<Motherboard>().AddRange(
            new Motherboard { Id = "mobo-intel-1", Name = "ASUS ROG Maximus Z790 Hero", Socket = "LGA1700", Chipset = "Z790" },
            new Motherboard { Id = "mobo-amd-1", Name = "Gigabyte X670 Aorus Elite", Socket = "AM5", Chipset = "X670" }
        );
        await db.SaveChangesAsync();
    }

    // 🔹 Quiz Questions
    if (!await db.QuizQuestions.AnyAsync())
    {
        db.QuizQuestions.AddRange(
            new QuizQuestion { Question = "Какой сокет у Intel i9-13900K?", Options = new[] { "AM4", "LGA1700", "AM5", "LGA1200" }, CorrectOptionIndex = 1, Difficulty = "easy" },
            new QuizQuestion { Question = "Что важнее для стабильного разгона?", Options = new[] { "RGB подсветка", "Напряжение", "Цвет корпуса", "SSD" }, CorrectOptionIndex = 1, Difficulty = "medium" },
            new QuizQuestion { Question = "Что произойдет при слишком высоком напряжении?", Options = new[] { "Улучшится FPS", "Снизится температура", "Повреждение CPU", "Ничего" }, CorrectOptionIndex = 2, Difficulty = "hard" },
            new QuizQuestion { Question = "Какой тип памяти у современных систем?", Options = new[] { "DDR3", "DDR4/DDR5", "SDRAM", "GDDR3" }, CorrectOptionIndex = 1, Difficulty = "easy" }
        );
        await db.SaveChangesAsync();
    }

    // 🔹 BIOS Versions
    if (!await db.BiosVersions.AnyAsync())
    {
        db.BiosVersions.AddRange(
            new BiosVersion { Id = "bios-1", MotherboardId = "mobo-intel-1", Version = "F1", ReleaseDate = DateTime.UtcNow.AddMonths(-6), Description = "Initial release", Stability = 80, IsBeta = false },
            new BiosVersion { Id = "bios-2", MotherboardId = "mobo-intel-1", Version = "F2", ReleaseDate = DateTime.UtcNow.AddMonths(-3), Description = "Improved CPU support", Stability = 90, IsBeta = false }
        );
        await db.SaveChangesAsync();
    }

    // 🔹 CPU Support
    if (!await db.CpuSupports.AnyAsync())
    {
        db.CpuSupports.AddRange(
            new CpuSupport { CpuId = "cpu-intel-1", BiosVersionId = "bios-1", IsSupported = true },
            new CpuSupport { CpuId = "cpu-intel-1", BiosVersionId = "bios-2", IsSupported = true },
            new CpuSupport { CpuId = "cpu-amd-1", BiosVersionId = "bios-1", IsSupported = false }
        );
        await db.SaveChangesAsync();
    }

    // 🔹 Quiz Results
    if (!await db.QuizResults.AnyAsync())
    {
        db.QuizResults.Add(new QuizResult { UserId = "test-user", Score = 2, TotalQuestions = 4, CompletedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    // 🔹 Admin User (ОБЯЗАТЕЛЬНО для авторизации)
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
        Console.WriteLine("✅ Admin: admin@example.com / admin12345");
    }
    
    Console.WriteLine("[DB] Initialization completed.");
}
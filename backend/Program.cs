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

    // 🔹 Quiz Questions — 40 вопросов по теме "Устройство ПК"
if (!await db.QuizQuestions.AnyAsync())
{
    db.QuizQuestions.AddRange(
        // === CPU / ПРОЦЕССОРЫ (7 вопросов) ===
        new QuizQuestion { 
            Question = "Какой сокет используется у процессоров Intel Core 12-14 поколения?", 
            Options = new[] { "AM4", "LGA1700", "AM5", "LGA1200" }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Что означает аббревиатура TDP у процессора?", 
            Options = new[] { 
                "Total Data Processing", 
                "Thermal Design Power", 
                "Turbo Dynamic Performance", 
                "Technical Data Package" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Какой параметр процессора напрямую влияет на многозадачность?", 
            Options = new[] { "Тактовая частота", "Количество ядер и потоков", "Объём кэша L1", "TDP" }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Что такое гиперпоточность (Hyper-Threading)?", 
            Options = new[] { 
                "Увеличение тактовой частоты", 
                "Технология, позволяющая одному ядру обрабатывать два потока", 
                "Автоматический разгон процессора", 
                "Снижение энергопотребления" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Какой бренд процессоров использует сокет AM5?", 
            Options = new[] { "Intel", "AMD", "Apple", "Qualcomm" }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Что произойдёт, если установить процессор с неподходящим сокетом?", 
            Options = new[] { 
                "Он будет работать на пониженной частоте", 
                "Физически не установится или повредится", 
                "Система автоматически подберёт драйверы", 
                "Ничего, все сокеты универсальны" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Какой параметр важнее для игр: высокая частота или много ядер?", 
            Options = new[] { 
                "Только много ядер", 
                "Только высокая частота", 
                "Баланс: частота важнее, но 6+ ядер желательно", 
                "Ни то, ни другое — важна только видеокарта" 
            }, 
            CorrectOptionIndex = 2, 
            Difficulty = "medium" 
        },

        // === MOTHERBOARD / МАТЕРИНСКАЯ ПЛАТА (6 вопросов) ===
        new QuizQuestion { 
            Question = "Какой чипсет Intel поддерживает разгон процессоров серии K?", 
            Options = new[] { "B760", "H770", "Z790", "H610" }, 
            CorrectOptionIndex = 2, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Что означает форм-фактор ATX?", 
            Options = new[] { 
                "Тип сокета процессора", 
                "Стандарт размера и крепления материнской платы", 
                "Версия BIOS", 
                "Тип поддержки памяти" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Для чего нужен чипсет на материнской плате?", 
            Options = new[] { 
                "Только для красоты", 
                "Управление взаимодействием компонентов (CPU, RAM, PCIe, USB)", 
                "Только для подключения интернета", 
                "Хранение данных пользователя" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Что такое M.2 слот на материнской плате?", 
            Options = new[] { 
                "Слот для оперативной памяти", 
                "Слот для NVMe SSD накопителей", 
                "Слот для видеокарты", 
                "Слот для звуковой карты" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Какой разъём питания материнской платы является основным?", 
            Options = new[] { "4-pin CPU", "24-pin ATX", "6-pin PCIe", "SATA" }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Можно ли использовать DDR5 память в плате с поддержкой только DDR4?", 
            Options = new[] { 
                "Да, если обновить BIOS", 
                "Нет, физически несовместимы", 
                "Да, но на пониженной частоте", 
                "Только с переходником" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },

        // === RAM / ОПЕРАТИВНАЯ ПАМЯТЬ (6 вопросов) ===
        new QuizQuestion { 
            Question = "Что означает маркировка DDR5-6000?", 
            Options = new[] { 
                "Объём памяти 6000 МБ", 
                "Эффективная частота 6000 МГц", 
                "Тайминги CL60", 
                "Напряжение 6.00В" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Что такое двухканальный режим памяти?", 
            Options = new[] { 
                "Установка двух модулей в слоты одного цвета для увеличения пропускной способности", 
                "Использование памяти от двух разных брендов", 
                "Работа памяти на двух частотах одновременно", 
                "Подключение RAM к двум процессорам" 
            }, 
            CorrectOptionIndex = 0, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Какой параметр памяти влияет на задержки (латентность)?", 
            Options = new[] { "Частота", "Объём", "Тайминги (CL)", "Напряжение" }, 
            CorrectOptionIndex = 2, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Сколько памяти рекомендуется для современных игр в 2024 году?", 
            Options = new[] { "4 ГБ", "8 ГБ", "16 ГБ", "64 ГБ" }, 
            CorrectOptionIndex = 2, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Что такое XMP/EXPO профиль в BIOS?", 
            Options = new[] { 
                "Профиль энергосбережения", 
                "Автоматический разгон памяти до заявленной частоты", 
                "Режим тихой работы вентиляторов", 
                "Настройка приоритета загрузки" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Можно ли смешивать модули RAM с разной частотой?", 
            Options = new[] { 
                "Нет, никогда", 
                "Да, но все модули будут работать на частоте самого медленного", 
                "Да, система автоматически разгонит медленный модуль", 
                "Только если они одного бренда" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },

        // === GPU / ВИДЕОКАРТЫ (6 вопросов) ===
        new QuizQuestion { 
            Question = "Что означает VRAM в характеристиках видеокарты?", 
            Options = new[] { 
                "Virtual RAM", 
                "Video Random Access Memory — память видеокарты", 
                "Voltage Regulation Module", 
                "Variable Refresh Rate Memory" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Какой интерфейс подключения видеокарты является стандартом?", 
            Options = new[] { "PCI", "PCIe x16", "AGP", "USB-C" }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Что такое трассировка лучей (Ray Tracing)?", 
            Options = new[] { 
                "Технология улучшения текстуры", 
                "Метод реалистичного расчёта освещения и отражений", 
                "Способ разгона видеокарты", 
                "Режим энергосбережения" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Для чего нужен разъём 12VHPWR на видеокартах RTX 40-й серии?", 
            Options = new[] { 
                "Подключение монитора", 
                "Питание видеокарты (до 600 Вт)", 
                "Синхронизация с материнской платой", 
                "Подключение RGB-подсветки" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Что означает аббревиатура DLSS?", 
            Options = new[] { 
                "Deep Learning Super Sampling — ИИ-апскейлинг", 
                "Direct Low-latency Sound System", 
                "Dynamic Load Sharing Service", 
                "Digital Light Shadow System" 
            }, 
            CorrectOptionIndex = 0, 
            Difficulty = "hard" 
        },
        new QuizQuestion { 
            Question = "Что такое интегрированная графика?", 
            Options = new[] { 
                "Видеокарта в отдельном слоте", 
                "Графическое ядро, встроенное в процессор или чипсет", 
                "Виртуальная видеокарта в облаке", 
                "Графический редактор в ОС" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },

        // === STORAGE / НАКОПИТЕЛИ (5 вопросов) ===
        new QuizQuestion { 
            Question = "В чём главное преимущество NVMe SSD перед SATA SSD?", 
            Options = new[] { 
                "Ниже цена", 
                "Выше скорость за счёт подключения через PCIe", 
                "Больший объём", 
                "Меньше нагрев" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Что означает TBW в характеристиках SSD?", 
            Options = new[] { 
                "Total Bytes Written — ресурс записи", 
                "Turbo Boost Write", 
                "Time Before Wearout", 
                "Technical Backup Warranty" 
            }, 
            CorrectOptionIndex = 0, 
            Difficulty = "hard" 
        },
        new QuizQuestion { 
            Question = "Что такое RAID 1?", 
            Options = new[] { 
                "Полоса данных для скорости", 
                "Зеркалирование — дублирование данных на двух дисках", 
                "Чётность для восстановления", 
                "Комбинация всех уровней" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Какой интерфейс имеет наибольший теоретический предел скорости?", 
            Options = new[] { "SATA III", "PCIe 3.0 x4", "PCIe 4.0 x4", "PCIe 5.0 x4" }, 
            CorrectOptionIndex = 3, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Что такое S.M.A.R.T. в накопителях?", 
            Options = new[] { 
                "Система мониторинга состояния и предсказания сбоев", 
                "Режим быстрого форматирования", 
                "Технология сжатия данных", 
                "Протокол шифрования" 
            }, 
            CorrectOptionIndex = 0, 
            Difficulty = "medium" 
        },

        // === PSU / БЛОК ПИТАНИЯ (5 вопросов) ===
        new QuizQuestion { 
            Question = "Что означает сертификация 80 PLUS Bronze?", 
            Options = new[] { 
                "Гарантия 3 года", 
                "КПД блока питания не менее 82% при 20-100% нагрузке", 
                "Поддержка только бюджетных сборок", 
                "Низкий уровень шума" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Какой запас по мощности блока питания рекомендуется?", 
            Options = new[] { "0%", "10-20%", "50-100%", "200%+" }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Что такое модульный блок питания?", 
            Options = new[] { 
                "Состоящий из нескольких блоков", 
                "С возможностью отсоединения ненужных кабелей", 
                "С автоматической регулировкой напряжения", 
                "С встроенным ИБП" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Какой разъём питания нужен для мощной видеокарты?", 
            Options = new[] { "SATA", "4-pin CPU", "6/8-pin PCIe", "Molex" }, 
            CorrectOptionIndex = 2, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Почему не стоит экономить на блоке питания?", 
            Options = new[] { 
                "Он не влияет на систему", 
                "Некачественный БП может повредить другие компоненты", 
                "Дешёвые БП всегда тише", 
                "Мощные БП занимают больше места" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },

        // === COOLING / ОХЛАЖДЕНИЕ (3 вопроса) ===
        new QuizQuestion { 
            Question = "Что такое TDP кулера?", 
            Options = new[] { 
                "Максимальная температура, которую он выдерживает", 
                "Максимальная тепловая мощность, которую он может рассеять", 
                "Потребляемая мощность вентиляторов", 
                "Уровень шума в дБ" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Для чего нужна термопаста?", 
            Options = new[] { 
                "Крепление кулера к плате", 
                "Заполнение микронеровностей между процессором и кулером для улучшения теплопередачи", 
                "Защита от статического электричества", 
                "Смазка для вентиляторов" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },
        new QuizQuestion { 
            Question = "Какой размер вентилятора наиболее распространён в корпусах ATX?", 
            Options = new[] { "80 мм", "120 мм", "200 мм", "250 мм" }, 
            CorrectOptionIndex = 1, 
            Difficulty = "easy" 
        },

        // === BIOS / UEFI / ОБЩЕЕ (2 вопроса) ===
        new QuizQuestion { 
            Question = "В чём разница между BIOS и UEFI?", 
            Options = new[] { 
                "Это одно и то же", 
                "UEFI — современный интерфейс с поддержкой мыши, больших дисков и безопасной загрузки", 
                "BIOS быстрее загружается", 
                "UEFI работает только на AMD" 
            }, 
            CorrectOptionIndex = 1, 
            Difficulty = "medium" 
        },
        new QuizQuestion { 
            Question = "Какую клавишу чаще всего используют для входа в BIOS при загрузке?", 
            Options = new[] { "F1 / F2 / Del", "Ctrl+Alt+Delete", "Esc", "Пробел" }, 
            CorrectOptionIndex = 0, 
            Difficulty = "easy" 
        }
    );
    await db.SaveChangesAsync();
    Console.WriteLine("✅ Seeded 40 quiz questions");
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
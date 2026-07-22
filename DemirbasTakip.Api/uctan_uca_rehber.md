# Kurumsal Demirbaş ve Zimmet Takip Sistemi — Sıfırdan Bitirmeye Kadar Uçtan Uca Uygulama Rehberi

> Bu rehber, sizi hiçbir ön bilgi varsaymadan, adım adım komut komut, projeyi baştan sona (backend + frontend) bitirmeye götürür. Her adımı sırayla uygulayın, bir adımı atlamayın. Kod bloklarını olduğu gibi kopyalayıp kullanabilirsiniz; dosya adları ve yollar tam olarak belirtilmiştir.
>
> **Teknoloji:** .NET Core Web API (backend) + SQL Server (veritabanı) + JWT (kimlik doğrulama) + Vue 3 + Vuetify 3 + Pinia + Axios (frontend)
> **Fazlar:** Faz 1 (Login) → Faz 2 (CRUD) → Faz 3 (Yetkilendirme) — hem backend hem frontend için aynı sırayla ilerlenecek.

---

## İçindekiler

- **BÖLÜM A — Hazırlık:** Kurulması gereken tüm araçlar
- **BÖLÜM B — Backend:** Proje oluşturma → Faz 1 → Faz 2 → Faz 3
- **BÖLÜM C — Backend'i Frontend'e Açma:** CORS
- **BÖLÜM D — Frontend:** Proje oluşturma → Faz 1 → Faz 2 → Faz 3
- **BÖLÜM E — Uçtan Uca Çalıştırma ve Test**
- **BÖLÜM F — Kabul Kriterleri Kontrol Listesi**
- **BÖLÜM G — Sorun Giderme**

---

# BÖLÜM A — Hazırlık

Aşağıdaki programları **bu sırayla** kurun. Her kurulumdan sonra verilen doğrulama komutunu çalıştırıp beklenen çıktıyı gördüğünüzden emin olun, sonra bir sonrakine geçin.

### A.1 — .NET SDK

1. https://dotnet.microsoft.com/download adresine gidin.
2. **.NET 10 (LTS)** sürümünü işletim sisteminize göre indirip kurun.
3. Terminal/PowerShell açın, doğrulayın:
   ```bash
   dotnet --version
   ```
   Çıktı `10.x.x` gibi bir şey olmalı.

### A.2 — Node.js (frontend için)

1. https://nodejs.org adresinden **LTS sürümünü** indirip kurun.
2. Doğrulayın:
   ```bash
   node --version
   npm --version
   ```

### A.3 — SQL Server

İki seçenekten birini uygulayın:

**Seçenek 1 — Yerel kurulum (Windows):**
1. https://www.microsoft.com/sql-server/sql-server-downloads adresinden **Developer Edition**'ı indirip kurun.
2. Kurulum sırasında "Basic" kurulum yeterli.
3. **SQL Server Management Studio (SSMS)**'yi de https://aka.ms/ssmsfullsetup adresinden indirip kurun (veritabanını görsel olarak yönetmek için).

**Seçenek 2 — Docker ile (Windows/Mac/Linux, önerilir):**
1. Docker Desktop'ı kurun: https://www.docker.com/products/docker-desktop
2. Terminalde çalıştırın:
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Demirbas!2026" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
   ```
3. Konteyner çalışıyor mu kontrol edin:
   ```bash
   docker ps
   ```
   Listede `sqlserver` görünmeli.

### A.4 — Kod Editörü

1. **Visual Studio 2022/2026 Community** (Windows, ücretsiz) — https://visualstudio.microsoft.com/downloads — kurulumda **"ASP.NET and web development"** ve **"Node.js development"** iş yüklerini (workload) işaretleyin.
   - Alternatif: **VS Code** (https://code.visualstudio.com) + **C# Dev Kit** eklentisi. Her iki teknoloji için de VS Code kullanılabilir, bu rehberdeki komutlar VS Code terminaliyle birebir çalışır.

### A.5 — Postman

https://www.postman.com/downloads adresinden indirip kurun. API'yi tarayıcı olmadan test etmek için kullanacağız.

### A.6 — Git (opsiyonel ama önerilir)

https://git-scm.com/downloads adresinden kurun. Projeyi versiyon kontrolüne almak için:
```bash
git --version
```

---

# BÖLÜM B — Backend: .NET Core Web API

## B.1 — Proje İskeletini Oluşturma

1. Projelerinizi tutacağınız bir klasöre gidin (örnek: `Belgeler/Projeler`) ve terminalde şu klasörü oluşturup içine girin:
   ```bash
   mkdir DemirbasTakipSistemi
   cd DemirbasTakipSistemi
   ```

2. Solution (çözüm) dosyasını oluşturun:
   ```bash
   dotnet new sln -n DemirbasTakip
   ```

3. Web API projesini oluşturun (**`-controllers` bayrağını unutmayın**, bu bize controller tabanlı klasik yapıyı verir):
   ```bash
   dotnet new webapi -n DemirbasTakip.Api -controllers
   ```

4. Projeyi solution'a ekleyin:
   ```bash
   dotnet sln add DemirbasTakip.Api/DemirbasTakip.Api.csproj
   ```

5. Proje klasörüne girin:
   ```bash
   cd DemirbasTakip.Api
   ```

6. Şimdi çalıştığını doğrulayın:
   ```bash
   dotnet run
   ```
   Terminalde `Now listening on: https://localhost:5001` gibi bir satır görmelisiniz. Tarayıcıda `https://localhost:5001/swagger` adresini açın, Swagger sayfasını görüyorsanız her şey doğru. **`Ctrl+C` ile durdurun** ve devam edin.

## B.2 — Gerekli NuGet Paketlerini Kurma

`DemirbasTakip.Api` klasörünün içindeyken sırayla çalıştırın:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
```

EF Core migration komutlarını kullanabilmek için global aracı bir kez kurun:
```bash
dotnet tool install --global dotnet-ef
```
(Zaten kuruluysa `dotnet tool update --global dotnet-ef` çalıştırın.)

## B.3 — Klasör Yapısını Oluşturma

`DemirbasTakip.Api` klasörünün içinde şu klasörleri oluşturun:

```bash
mkdir Entities DTOs Data Services Repositories Auth Common Controllers
```

(`Controllers` klasörü zaten var, `WeatherForecastController.cs` dosyasını silin — artık ihtiyacımız yok.)

```bash
rm Controllers/WeatherForecastController.cs
```

Nihai klasör yapınız şu şekilde olacak:
```
DemirbasTakip.Api/
├── Controllers/
├── Services/
├── Repositories/      (opsiyonel, doğrudan DbContext de kullanabilirsiniz)
├── Entities/
├── DTOs/
├── Auth/
├── Common/
├── Data/
├── Program.cs
├── appsettings.json
└── DemirbasTakip.Api.csproj
```

## B.4 — appsettings.json Ayarları

`appsettings.json` dosyasını açın ve tamamen şu içerikle değiştirin:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DemirbasDb;User Id=sa;Password=Demirbas!2026;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "bu-anahtar-en-az-32-karakter-uzunlugunda-olmali-ve-gizli-tutulmali",
    "Issuer": "DemirbasTakip.Api",
    "Audience": "DemirbasTakip.Client",
    "ExpireMinutes": 120
  }
}
```

> Eğer Docker yerine yerel SQL Server kurdunuzsa ve Windows Authentication kullanacaksanız connection string'i şuna çevirin:
> `"Server=localhost;Database=DemirbasDb;Trusted_Connection=True;TrustServerCertificate=True;"`

## B.5 — FAZ 1: Login Altyapısı (Backend)

### B.5.1 — Entity'leri Oluşturma

`Entities/Role.cs` dosyasını oluşturun:
```csharp
namespace DemirbasTakip.Api.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}
```

`Entities/User.cs` dosyasını oluşturun:
```csharp
namespace DemirbasTakip.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
```

### B.5.2 — DbContext Oluşturma

`Data/AppDbContext.cs` dosyasını oluşturun:
```csharp
using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Başlangıç rollerini otomatik ekle
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );
    }
}
```

### B.5.3 — DTO'ları Oluşturma

`DTOs/LoginDto.cs`:
```csharp
namespace DemirbasTakip.Api.DTOs;

public record LoginDto(string Username, string Password);
```

`DTOs/LoginResponseDto.cs`:
```csharp
namespace DemirbasTakip.Api.DTOs;

public record LoginResponseDto(string Token, string Username, string Role);
```

### B.5.4 — Token Üretici Servis

`Auth/TokenService.cs`:
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Auth;

public class TokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### B.5.5 — Auth Servisi

`Services/IAuthService.cs`:
```csharp
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
}
```

`Services/AuthService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Auth;

namespace DemirbasTakip.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthService(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        var token = _tokenService.CreateToken(user);
        return new LoginResponseDto(token, user.Username, user.Role.Name);
    }
}
```

### B.5.6 — AuthController

`Controllers/AuthController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto.Username, dto.Password);
        if (result is null)
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var username = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return Ok(new { username, role });
    }
}
```

### B.5.7 — Program.cs'i Baştan Yazma

`Program.cs` dosyasının **tüm içeriğini silin** ve yerine şunu yazın:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.Auth;
using DemirbasTakip.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Servis Kayıtları ----
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// Faz 2'de Asset/Personnel/Assignment servislerini buraya ekleyeceğiz

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Örnek: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// CORS — Bölüm C'de detaylandırılacak, şimdiden ekliyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")   // Vite'ın varsayılan portu
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ---- Middleware Pipeline (sıralama önemli, değiştirmeyin) ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### B.5.8 — İlk Migration'ı Oluşturma ve Veritabanını Kurma

`DemirbasTakip.Api` klasöründeyken:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Hata almazsanız SQL Server'da `DemirbasDb` adında bir veritabanı, içinde `Users` ve `Roles` tabloları (Roles içinde Admin/User önceden dolu) oluşmuş olacak.

### B.5.9 — İlk Admin Kullanıcısını Elle Ekleme

Henüz kayıt (register) endpoint'i yazmadık, bu yüzden ilk admin kullanıcıyı elle ekleyeceğiz.

1. Aşağıdaki küçük konsol kodunu geçici olarak `Program.cs`'in **en üstüne**, `var builder = ...` satırından **önce** eklemek yerine, daha kolay bir yöntem kullanalım: SSMS/Azure Data Studio ile `DemirbasDb` veritabanına bağlanın ve şu SQL'i çalıştırın (şifre olarak `Admin123!` kullanacağız, hash'i aşağıda hazır veriyorum):

```sql
INSERT INTO Users (Username, PasswordHash, RoleId)
VALUES ('admin', '$2a$11$K9wZ8t5mZ1p8qYyH9Xz5UuQvJd1yqzT0nJb1zR8vJHxN0Q5cW9r5C', 1);
```

> Bu hash `Admin123!` şifresinin BCrypt karşılığı **değildir** — kendi hash'inizi üretmeniz gerekir çünkü BCrypt her seferinde farklı (salt'lı) hash üretir. Bunu yapmanın en kolay yolu: `Program.cs`'e geçici bir test endpoint'i eklemektir. Aşağıdaki adımı izleyin:

**Geçici hash üretme endpoint'i** — `Program.cs`'de `app.MapControllers();` satırından hemen önce ekleyin, admin kullanıcıyı ekledikten sonra silin:
```csharp
app.MapGet("/gecici-hash-uret/{sifre}", (string sifre) => BCrypt.Net.BCrypt.HashPassword(sifre));
```
`dotnet run` ile uygulamayı çalıştırın, tarayıcıda `https://localhost:5001/gecici-hash-uret/Admin123!` adresine gidin, dönen hash'i kopyalayın ve yukarıdaki SQL `INSERT` komutundaki hash yerine yapıştırıp çalıştırın. **Sonra bu geçici endpoint satırını `Program.cs`'den silmeyi unutmayın** — production'da böyle bir endpoint asla bırakılmaz.

### B.5.10 — Faz 1'i Test Etme

1. `dotnet run` ile uygulamayı başlatın.
2. Postman'de yeni bir istek oluşturun:
   - **POST** `https://localhost:5001/api/auth/login`
   - Body → raw → JSON:
     ```json
     { "username": "admin", "password": "Admin123!" }
     ```
   - Gönderin, `200 OK` ve içinde `token` alanı dönmeli.
3. Dönen `token` değerini kopyalayın.
4. Yeni istek: **GET** `https://localhost:5001/api/auth/me`
   - Headers → `Authorization` → `Bearer <kopyaladığınız token>`
   - Gönderin, `{ "username": "admin", "role": "Admin" }` dönmeli.
5. Token'sız çağırdığınızda (Authorization header'ı olmadan) `401 Unauthorized` dönmeli.

**Faz 1 backend tamamlandı.** Faz 2'ye geçmeden önce isterseniz frontend Faz 1'i de şimdi yapıp (Bölüm D.2) uçtan uca login akışını görebilirsiniz — ama bu rehberde önce tüm backend'i, sonra tüm frontend'i bitirme sırasını izleyeceğiz.

## B.6 — FAZ 2: CRUD Altyapısı (Backend)

### B.6.1 — Kalan Entity'ler

`Entities/Asset.cs`:
```csharp
namespace DemirbasTakip.Api.Entities;

public class Asset
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "Aktif";
    public string SerialNumber { get; set; } = string.Empty;
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}
```

`Entities/Personnel.cs`:
```csharp
namespace DemirbasTakip.Api.Entities;

public class Personnel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}
```

`Entities/AssetAssignment.cs`:
```csharp
namespace DemirbasTakip.Api.Entities;

public class AssetAssignment
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
    public int PersonnelId { get; set; }
    public Personnel Personnel { get; set; } = null!;
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
}
```

### B.6.2 — DbContext'i Güncelleme

`Data/AppDbContext.cs` dosyasını açın, şu satırları `Users`/`Roles` `DbSet`'lerinin altına ekleyin:
```csharp
public DbSet<Asset> Assets => Set<Asset>();
public DbSet<Personnel> Personnel => Set<Personnel>();
public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
```
`OnModelCreating` metodunun içine, `HasData` satırlarından sonra ekleyin:
```csharp
modelBuilder.Entity<AssetAssignment>()
    .HasOne(a => a.Asset)
    .WithMany(a => a.Assignments)
    .HasForeignKey(a => a.AssetId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<AssetAssignment>()
    .HasOne(a => a.Personnel)
    .WithMany(p => p.Assignments)
    .HasForeignKey(a => a.PersonnelId)
    .OnDelete(DeleteBehavior.Restrict);
```

### B.6.3 — DTO'lar

`DTOs/AssetDtos.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace DemirbasTakip.Api.DTOs;

public record AssetResponseDto(int Id, string Code, string Name, string Category, string Status, string SerialNumber);

public record AssetCreateDto(
    [Required, StringLength(50)] string Code,
    [Required, StringLength(200)] string Name,
    [Required] string Category,
    string SerialNumber);
```

`DTOs/PersonnelDtos.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace DemirbasTakip.Api.DTOs;

public record PersonnelResponseDto(int Id, string FullName, string Department);

public record PersonnelCreateDto(
    [Required, StringLength(150)] string FullName,
    [Required, StringLength(100)] string Department);
```

`DTOs/AssignmentDtos.cs`:
```csharp
namespace DemirbasTakip.Api.DTOs;

public record AssignmentResponseDto(int Id, int AssetId, string AssetName, int PersonnelId, string PersonnelName, DateTime AssignedDate, DateTime? ReturnedDate);

public record AssignmentCreateDto(int AssetId, int PersonnelId);

public record AssignmentReturnDto(int AssignmentId);
```

### B.6.4 — Ortak Hata Sınıfı

`Common/BusinessRuleException.cs`:
```csharp
namespace DemirbasTakip.Api.Common;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
```

### B.6.5 — Asset Servisi ve Controller'ı

`Services/IAssetService.cs`:
```csharp
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IAssetService
{
    Task<IEnumerable<AssetResponseDto>> GetAllAsync();
    Task<AssetResponseDto?> GetByIdAsync(int id);
    Task<AssetResponseDto> CreateAsync(AssetCreateDto dto);
    Task<bool> UpdateAsync(int id, AssetCreateDto dto);
    Task<bool> DeleteAsync(int id);
}
```

`Services/AssetService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Services;

public class AssetService : IAssetService
{
    private readonly AppDbContext _context;
    public AssetService(AppDbContext context) => _context = context;

    private static AssetResponseDto ToDto(Asset a) =>
        new(a.Id, a.Code, a.Name, a.Category, a.Status, a.SerialNumber);

    public async Task<IEnumerable<AssetResponseDto>> GetAllAsync()
        => await _context.Assets.Select(a => ToDto(a)).ToListAsync();

    public async Task<AssetResponseDto?> GetByIdAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        return asset is null ? null : ToDto(asset);
    }

    public async Task<AssetResponseDto> CreateAsync(AssetCreateDto dto)
    {
        var asset = new Asset
        {
            Code = dto.Code,
            Name = dto.Name,
            Category = dto.Category,
            SerialNumber = dto.SerialNumber,
            Status = "Aktif"
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return ToDto(asset);
    }

    public async Task<bool> UpdateAsync(int id, AssetCreateDto dto)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset is null) return false;

        asset.Code = dto.Code;
        asset.Name = dto.Name;
        asset.Category = dto.Category;
        asset.SerialNumber = dto.SerialNumber;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset is null) return false;

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

`Controllers/AssetsController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("api/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _service;
    public AssetsController(IAssetService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] AssetCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetCreateDto dto)
        => await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}
```

### B.6.6 — Personnel Servisi ve Controller'ı

`Services/IPersonnelService.cs`:
```csharp
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IPersonnelService
{
    Task<IEnumerable<PersonnelResponseDto>> GetAllAsync();
    Task<PersonnelResponseDto> CreateAsync(PersonnelCreateDto dto);
}
```

`Services/PersonnelService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Services;

public class PersonnelService : IPersonnelService
{
    private readonly AppDbContext _context;
    public PersonnelService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<PersonnelResponseDto>> GetAllAsync()
        => await _context.Personnel
            .Select(p => new PersonnelResponseDto(p.Id, p.FullName, p.Department))
            .ToListAsync();

    public async Task<PersonnelResponseDto> CreateAsync(PersonnelCreateDto dto)
    {
        var personnel = new Personnel { FullName = dto.FullName, Department = dto.Department };
        _context.Personnel.Add(personnel);
        await _context.SaveChangesAsync();
        return new PersonnelResponseDto(personnel.Id, personnel.FullName, personnel.Department);
    }
}
```

`Controllers/PersonnelController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("api/personnel")]
[Authorize]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelService _service;
    public PersonnelController(IPersonnelService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] PersonnelCreateDto dto)
        => Ok(await _service.CreateAsync(dto));
}
```

### B.6.7 — Assignment (Zimmet) Servisi ve Controller'ı

`Services/IAssignmentService.cs`:
```csharp
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IAssignmentService
{
    Task<IEnumerable<AssignmentResponseDto>> GetAllAsync();
    Task<AssignmentResponseDto> AssignAsync(AssignmentCreateDto dto);
    Task<bool> ReturnAsync(AssignmentReturnDto dto);
}
```

`Services/AssignmentService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Entities;
using DemirbasTakip.Api.Common;

namespace DemirbasTakip.Api.Services;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _context;
    public AssignmentService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<AssignmentResponseDto>> GetAllAsync()
        => await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
            .Select(a => new AssignmentResponseDto(a.Id, a.AssetId, a.Asset.Name, a.PersonnelId, a.Personnel.FullName, a.AssignedDate, a.ReturnedDate))
            .ToListAsync();

    public async Task<AssignmentResponseDto> AssignAsync(AssignmentCreateDto dto)
    {
        // İş kuralı: aynı demirbaş aktif zimmetteyse ikinci atamayı engelle
        bool activeAssignmentExists = await _context.AssetAssignments
            .AnyAsync(a => a.AssetId == dto.AssetId && a.ReturnedDate == null);

        if (activeAssignmentExists)
            throw new BusinessRuleException("Bu demirbaş zaten aktif zimmette, önce iade alınmalı.");

        var assignment = new AssetAssignment
        {
            AssetId = dto.AssetId,
            PersonnelId = dto.PersonnelId,
            AssignedDate = DateTime.UtcNow,
            ReturnedDate = null
        };
        _context.AssetAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        var asset = await _context.Assets.FindAsync(dto.AssetId);
        var personnel = await _context.Personnel.FindAsync(dto.PersonnelId);
        return new AssignmentResponseDto(assignment.Id, assignment.AssetId, asset!.Name, assignment.PersonnelId, personnel!.FullName, assignment.AssignedDate, assignment.ReturnedDate);
    }

    public async Task<bool> ReturnAsync(AssignmentReturnDto dto)
    {
        var assignment = await _context.AssetAssignments.FindAsync(dto.AssignmentId);
        if (assignment is null || assignment.ReturnedDate is not null) return false;

        assignment.ReturnedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
```

`Controllers/AssignmentsController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;
using DemirbasTakip.Api.Common;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _service;
    public AssignmentsController(IAssignmentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign([FromBody] AssignmentCreateDto dto)
    {
        try
        {
            var result = await _service.AssignAsync(dto);
            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            return Conflict(new { message = ex.Message });   // 409
        }
    }

    [HttpPost("return")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Return([FromBody] AssignmentReturnDto dto)
        => await _service.ReturnAsync(dto) ? Ok(new { message = "İade alındı." }) : NotFound();
}
```

### B.6.8 — Yeni Servisleri Program.cs'e Kaydetme

`Program.cs`'de `builder.Services.AddScoped<IAuthService, AuthService>();` satırının altına ekleyin:
```csharp
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
```

### B.6.9 — Migration Oluşturma

```bash
dotnet ef migrations add AddAssetsPersonnelAssignments
dotnet ef database update
```

### B.6.10 — Faz 2'yi Test Etme

Postman'de (her istekte admin token'ını `Authorization: Bearer <token>` header'ında kullanarak):

1. **POST** `/api/assets` → body: `{ "code": "DMR-001", "name": "Dell Laptop", "category": "Bilgisayar", "serialNumber": "SN12345" }` → `201 Created` bekleyin.
2. **GET** `/api/assets` → az önce eklediğiniz kaydı listede görün.
3. **POST** `/api/personnel` → body: `{ "fullName": "Ahmet Yılmaz", "department": "Bilgi İşlem" }` → `200 OK`.
4. **POST** `/api/assignments` → body: `{ "assetId": 1, "personnelId": 1 }` → `200 OK`.
5. Aynı isteği **tekrar** gönderin → `409 Conflict` ve "zaten aktif zimmette" mesajı dönmeli — iş kuralı çalışıyor.
6. **POST** `/api/assignments/return` → body: `{ "assignmentId": 1 }` → `200 OK`.
7. İlk assignment'ı tekrar deneyin → artık `200 OK` ile başarılı olmalı (roadmap'inizdeki "iade sonrası tekrar zimmet" senaryosu).

**Faz 2 backend tamamlandı.**

## B.7 — FAZ 3: Yetkilendirme (Backend)

İyi haber: Controller kodlarını B.6'da zaten `[Authorize]` ve `[Authorize(Roles = "Admin")]` ile yazdık, bu yüzden backend tarafında Faz 3'ün büyük kısmı **hazır**. Roadmap'inizin istediği son iki kontrolü ekleyelim:

### B.7.1 — Kural Kontrolü

Aşağıdaki tabloyu kontrol edin, hepsi B.6'da zaten uygulanmış olmalı:

| Kural | Nerede uygulandı |
|---|---|
| Admin: ekleme/güncelleme/silme/zimmet verme/iade alma | `AssetsController` POST/PUT/DELETE, `AssignmentsController` tümü → `[Authorize(Roles = "Admin")]` |
| User: listeleme ve kendi bilgilerini görme | `GetAll`/`GetById` metotları sadece `[Authorize]` (rol şartı yok) |
| Token yoksa 401, rol yetersizse 403 | ASP.NET Core varsayılan davranışı, ekstra kod gerekmez |

### B.7.2 — "Kendi Bilgilerini Görme" için ek endpoint

`AuthController.cs`'deki `Me()` action'ı zaten bunu karşılıyor (token içindeki claim'den kullanıcı bilgisini döner, veritabanı sorgusu gerekmez).

### B.7.3 — Faz 3'ü Test Etme

1. `Users` tablosuna ikinci bir kullanıcı ekleyin (RoleId = 2, yani "User") — B.5.9'daki gibi geçici hash endpoint'ini kullanarak.
2. Bu kullanıcıyla login olun, token alın.
3. Bu token ile **POST** `/api/assets` çağırmayı deneyin → `403 Forbidden` dönmeli.
4. Aynı token ile **GET** `/api/assets` çağırın → `200 OK` dönmeli (listeleme herkese açık).

**Backend tamamen bitti.** Şimdi frontend'e geçiyoruz.

---

# BÖLÜM C — Backend'i Frontend'e Açma: CORS

Frontend (`http://localhost:5173`) ile backend (`https://localhost:5001`) **farklı portlarda** çalıştığı için, tarayıcı güvenlik politikası (CORS) gereği backend'in frontend'in isteklerine izin vermesi gerekir. Bunu B.5.7'de `Program.cs`'e zaten ekledik:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```
ve
```csharp
app.UseCors("FrontendPolicy");
```
Bu satırın **`app.UseAuthentication()`'dan önce, `app.UseHttpsRedirection()`'dan sonra** olduğundan emin olun (yukarıdaki B.5.7'deki sıra zaten doğru). Frontend projesini oluşturduğunuzda Vite varsayılan olarak `5173` portunu kullanır; farklı bir port kullanırsanız bu satırı ona göre güncelleyin.

---

# BÖLÜM D — Frontend: Vue 3 + Vuetify 3

## D.1 — Proje İskeletini Oluşturma

1. `DemirbasTakipSistemi` ana klasörüne dönün (backend klasörünün **yanına**, içine değil):
   ```bash
   cd ..
   ```
   Şu an `DemirbasTakipSistemi/DemirbasTakip.Api` klasöründeydiniz, `DemirbasTakipSistemi` klasörüne çıktınız.

2. Vuetify'ın resmi kurulum aracıyla projeyi oluşturun:
   ```bash
   npm create vuetify@latest
   ```
3. Sorulara şu şekilde cevap verin:
   - **Project name:** `demirbas-takip-web`
   - **Framework:** Vue (varsayılan)
   - **Use TypeScript?** Hayır (`No`) — bu rehberde JavaScript kullanıyoruz, isterseniz Evet de seçebilirsiniz, kod örnekleri küçük değişikliklerle çalışır.
   - **Vuetify Features:** Router, Pinia, ESLint seçeneklerinin **hepsini** işaretleyin (boşluk tuşuyla seçim, enter ile onay).
   - **Package manager:** npm

4. Proje klasörüne girin ve gerekli paketleri kurun:
   ```bash
   cd demirbas-takip-web
   npm install
   npm install axios
   ```

5. Projeyi çalıştırıp doğrulayın:
   ```bash
   npm run dev
   ```
   Terminalde `Local: http://localhost:5173/` gibi bir satır göreceksiniz. Tarayıcıda açın, Vuetify karşılama sayfasını görüyorsanız kurulum başarılı. **`Ctrl+C` ile durdurun.**

## D.2 — Proje Yapısını Hazırlama

`src/` klasörünün altında şu klasörleri oluşturun:
```bash
mkdir src/stores src/services src/views src/components src/router 2>/dev/null
```
(`stores` ve `router` klasörleri kurulum sırasında Pinia/Router seçtiyseniz zaten gelmiş olabilir, üzerine yazmaz, sorun değil.)

## D.3 — Axios Servis Katmanı (Tüm Fazlar İçin Ortak Altyapı)

`src/services/api.js` dosyasını oluşturun:
```javascript
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

const api = axios.create({
  baseURL: 'https://localhost:5001/api',
})

// İstek interceptor'ı — her isteğe otomatik token ekler
api.interceptors.request.use((config) => {
  const authStore = useAuthStore()
  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`
  }
  return config
})

// Yanıt interceptor'ı — 401 gelirse kullanıcıyı çıkışa yönlendirir
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      const authStore = useAuthStore()
      authStore.logout()
      router.push('/login')
    }
    return Promise.reject(error)
  }
)

export default api
```

> `baseURL`'i backend'inizin çalıştığı adrese göre ayarlayın. `dotnet run` çıktısındaki adres farklıysa (örn. `https://localhost:7001`) burayı güncelleyin.

## D.4 — FAZ 1: Login Ekranı ve Oturum Yönetimi (Frontend)

### D.4.1 — Auth Store (Pinia)

`src/stores/auth.js` dosyasını oluşturun (kurulum sırasında bir örnek `counter.js` gelmiş olabilir, onu silebilirsiniz):
```javascript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null)
  const username = ref(localStorage.getItem('username') || null)
  const role = ref(localStorage.getItem('role') || null)

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin = computed(() => role.value === 'Admin')

  function setSession(data) {
    token.value = data.token
    username.value = data.username
    role.value = data.role
    localStorage.setItem('token', data.token)
    localStorage.setItem('username', data.username)
    localStorage.setItem('role', data.role)
  }

  function logout() {
    token.value = null
    username.value = null
    role.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('role')
  }

  return { token, username, role, isLoggedIn, isAdmin, setSession, logout }
})
```

> Not: Token'ı `localStorage`'da saklamak basit projeler için yaygın bir yaklaşımdır. Daha ileri güvenlik gerektiren projelerde `httpOnly` cookie tercih edilir — staj projeniz kapsamında `localStorage` yeterlidir.

### D.4.2 — Login Servis Fonksiyonu

`src/services/authService.js`:
```javascript
import api from './api'

export async function login(username, password) {
  const response = await api.post('/auth/login', { username, password })
  return response.data   // { token, username, role }
}
```

### D.4.3 — Login.vue Ekranı

`src/views/Login.vue`:
```vue
<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="4">
        <v-card elevation="8" class="pa-4">
          <v-card-title class="text-h5 text-center">Demirbaş Takip Sistemi</v-card-title>
          <v-card-subtitle class="text-center mb-4">Giriş Yap</v-card-subtitle>
          <v-card-text>
            <v-form @submit.prevent="handleLogin" ref="formRef">
              <v-text-field
                v-model="username"
                label="Kullanıcı Adı"
                :rules="[v => !!v || 'Kullanıcı adı zorunludur']"
                required
              />
              <v-text-field
                v-model="password"
                label="Şifre"
                type="password"
                :rules="[v => !!v || 'Şifre zorunludur']"
                required
              />
              <v-alert v-if="errorMessage" type="error" density="compact" class="mb-4">
                {{ errorMessage }}
              </v-alert>
              <v-btn type="submit" color="primary" block :loading="loading">Giriş Yap</v-btn>
            </v-form>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { login } from '@/services/authService'

const username = ref('')
const password = ref('')
const errorMessage = ref('')
const loading = ref(false)
const formRef = ref(null)

const router = useRouter()
const authStore = useAuthStore()

async function handleLogin() {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  loading.value = true
  errorMessage.value = ''
  try {
    const data = await login(username.value, password.value)
    authStore.setSession(data)
    router.push('/')
  } catch (err) {
    errorMessage.value = err.response?.data?.message || 'Giriş başarısız.'
  } finally {
    loading.value = false
  }
}
</script>
```

### D.4.4 — Dashboard / Ana Sayfa

`src/views/Dashboard.vue`:
```vue
<template>
  <v-container>
    <h2>Hoş geldiniz, {{ authStore.username }}</h2>
    <p>Rolünüz: {{ authStore.role }}</p>
    <v-row class="mt-4">
      <v-col cols="12" sm="4">
        <v-card to="/assets" hover>
          <v-card-title>Demirbaşlar</v-card-title>
          <v-card-text>Demirbaş listesi ve işlemleri</v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" sm="4">
        <v-card to="/personnel" hover>
          <v-card-title>Personel</v-card-title>
          <v-card-text>Personel listesi</v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" sm="4">
        <v-card to="/assignments" hover>
          <v-card-title>Zimmet İşlemleri</v-card-title>
          <v-card-text>Atama, iade, geçmiş</v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
const authStore = useAuthStore()
</script>
```

### D.4.5 — Üst Menü (App Bar) ile Ana Layout

`src/App.vue` dosyasının içeriğini tamamen şununla değiştirin:
```vue
<template>
  <v-app>
    <v-app-bar v-if="authStore.isLoggedIn" color="primary">
      <v-toolbar-title>Demirbaş Takip</v-toolbar-title>
      <v-spacer />
      <span class="mr-4">{{ authStore.username }} ({{ authStore.role }})</span>
      <v-btn @click="handleLogout">Çıkış</v-btn>
    </v-app-bar>
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>
```

### D.4.6 — Router ve Router Guard

`src/router/index.js` dosyasının içeriğini tamamen şununla değiştirin:
```javascript
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  { path: '/login', name: 'login', component: () => import('@/views/Login.vue') },
  { path: '/', name: 'dashboard', component: () => import('@/views/Dashboard.vue'), meta: { requiresAuth: true } },
  { path: '/assets', name: 'assets', component: () => import('@/views/Assets.vue'), meta: { requiresAuth: true } },
  { path: '/personnel', name: 'personnel', component: () => import('@/views/Personnel.vue'), meta: { requiresAuth: true } },
  { path: '/assignments', name: 'assignments', component: () => import('@/views/Assignments.vue'), meta: { requiresAuth: true } },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    return { name: 'login' }
  }
  return true
})

export default router
```

> **Not:** `Assets.vue`, `Personnel.vue`, `Assignments.vue` dosyalarını henüz oluşturmadık — Faz 2'de oluşturacağız. Şimdilik router'da referans verilmiş olmaları sorun değil, siz D.5'e gelene kadar bu sayfalara girmeyeceksiniz.

### D.4.7 — Faz 1'i Test Etme

1. **Backend'in çalıştığından emin olun** — ayrı bir terminalde `DemirbasTakip.Api` klasöründe `dotnet run`.
2. Frontend'i başlatın — `demirbas-takip-web` klasöründe `npm run dev`.
3. Tarayıcıda `http://localhost:5173` açın → otomatik olarak `/login`'e yönlenmelisiniz (router guard çalışıyor).
4. `admin` / `Admin123!` ile giriş yapın → dashboard'a yönlenmeli, üst menüde kullanıcı adınız ve "Çıkış" butonu görünmeli.
5. Tarayıcı geliştirici araçlarından (F12) **Application/Storage → Local Storage** kısmına bakın, `token`, `username`, `role` değerlerinin kaydedildiğini görün.
6. "Çıkış" butonuna basın → login sayfasına dönmeli, local storage temizlenmeli.

**Frontend Faz 1 tamamlandı.**

## D.5 — FAZ 2: CRUD Ekranları (Frontend)

### D.5.1 — Servis Fonksiyonları

`src/services/assetService.js`:
```javascript
import api from './api'

export const getAssets = () => api.get('/assets').then(r => r.data)
export const createAsset = (asset) => api.post('/assets', asset).then(r => r.data)
export const updateAsset = (id, asset) => api.put(`/assets/${id}`, asset)
export const deleteAsset = (id) => api.delete(`/assets/${id}`)
```

`src/services/personnelService.js`:
```javascript
import api from './api'

export const getPersonnel = () => api.get('/personnel').then(r => r.data)
export const createPersonnel = (personnel) => api.post('/personnel', personnel).then(r => r.data)
```

`src/services/assignmentService.js`:
```javascript
import api from './api'

export const getAssignments = () => api.get('/assignments').then(r => r.data)
export const assignAsset = (data) => api.post('/assignments', data).then(r => r.data)
export const returnAsset = (assignmentId) => api.post('/assignments/return', { assignmentId })
```

### D.5.2 — Demirbaş Ekranı (Liste + Ekleme/Düzenleme Dialog'u)

`src/views/Assets.vue`:
```vue
<template>
  <v-container>
    <div class="d-flex justify-space-between align-center mb-4">
      <h2>Demirbaşlar</h2>
      <v-btn v-if="authStore.isAdmin" color="primary" @click="openCreateDialog">Yeni Demirbaş</v-btn>
    </div>

    <v-text-field v-model="search" label="Ara" prepend-inner-icon="mdi-magnify" class="mb-4" />

    <v-data-table
      :headers="headers"
      :items="assets"
      :search="search"
      :loading="loading"
    >
      <template #item.actions="{ item }">
        <template v-if="authStore.isAdmin">
          <v-btn size="small" variant="text" @click="openEditDialog(item)">Düzenle</v-btn>
          <v-btn size="small" variant="text" color="error" @click="handleDelete(item.id)">Sil</v-btn>
        </template>
      </template>
    </v-data-table>

    <v-dialog v-model="dialogOpen" max-width="500">
      <v-card>
        <v-card-title>{{ editingId ? 'Demirbaş Düzenle' : 'Yeni Demirbaş' }}</v-card-title>
        <v-card-text>
          <v-form ref="formRef">
            <v-text-field v-model="form.code" label="Kod" :rules="[v => !!v || 'Zorunlu']" />
            <v-text-field v-model="form.name" label="Ad" :rules="[v => !!v || 'Zorunlu']" />
            <v-text-field v-model="form.category" label="Kategori" />
            <v-text-field v-model="form.serialNumber" label="Seri No" />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="dialogOpen = false">İptal</v-btn>
          <v-btn color="primary" @click="handleSave">Kaydet</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { getAssets, createAsset, updateAsset, deleteAsset } from '@/services/assetService'

const authStore = useAuthStore()
const assets = ref([])
const loading = ref(false)
const search = ref('')
const dialogOpen = ref(false)
const editingId = ref(null)
const formRef = ref(null)
const form = ref({ code: '', name: '', category: '', serialNumber: '' })

const headers = [
  { title: 'Kod', key: 'code' },
  { title: 'Ad', key: 'name' },
  { title: 'Kategori', key: 'category' },
  { title: 'Durum', key: 'status' },
  { title: 'Seri No', key: 'serialNumber' },
  { title: 'İşlemler', key: 'actions', sortable: false },
]

async function loadAssets() {
  loading.value = true
  assets.value = await getAssets()
  loading.value = false
}

function openCreateDialog() {
  editingId.value = null
  form.value = { code: '', name: '', category: '', serialNumber: '' }
  dialogOpen.value = true
}

function openEditDialog(item) {
  editingId.value = item.id
  form.value = { code: item.code, name: item.name, category: item.category, serialNumber: item.serialNumber }
  dialogOpen.value = true
}

async function handleSave() {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  if (editingId.value) {
    await updateAsset(editingId.value, form.value)
  } else {
    await createAsset(form.value)
  }
  dialogOpen.value = false
  await loadAssets()
}

async function handleDelete(id) {
  if (!confirm('Bu demirbaşı silmek istediğinize emin misiniz?')) return
  await deleteAsset(id)
  await loadAssets()
}

onMounted(loadAssets)
</script>
```

### D.5.3 — Personel Ekranı

`src/views/Personnel.vue`:
```vue
<template>
  <v-container>
    <div class="d-flex justify-space-between align-center mb-4">
      <h2>Personel</h2>
      <v-btn v-if="authStore.isAdmin" color="primary" @click="dialogOpen = true">Yeni Personel</v-btn>
    </div>

    <v-data-table :headers="headers" :items="personnelList" :loading="loading" />

    <v-dialog v-model="dialogOpen" max-width="500">
      <v-card>
        <v-card-title>Yeni Personel</v-card-title>
        <v-card-text>
          <v-form ref="formRef">
            <v-text-field v-model="form.fullName" label="Ad Soyad" :rules="[v => !!v || 'Zorunlu']" />
            <v-text-field v-model="form.department" label="Departman" />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="dialogOpen = false">İptal</v-btn>
          <v-btn color="primary" @click="handleSave">Kaydet</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { getPersonnel, createPersonnel } from '@/services/personnelService'

const authStore = useAuthStore()
const personnelList = ref([])
const loading = ref(false)
const dialogOpen = ref(false)
const formRef = ref(null)
const form = ref({ fullName: '', department: '' })

const headers = [
  { title: 'Ad Soyad', key: 'fullName' },
  { title: 'Departman', key: 'department' },
]

async function loadPersonnel() {
  loading.value = true
  personnelList.value = await getPersonnel()
  loading.value = false
}

async function handleSave() {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  await createPersonnel(form.value)
  dialogOpen.value = false
  form.value = { fullName: '', department: '' }
  await loadPersonnel()
}

onMounted(loadPersonnel)
</script>
```

### D.5.4 — Zimmet İşlemleri Ekranı

`src/views/Assignments.vue`:
```vue
<template>
  <v-container>
    <h2 class="mb-4">Zimmet İşlemleri</h2>

    <v-card v-if="authStore.isAdmin" class="pa-4 mb-6">
      <v-card-title>Yeni Zimmet Ver</v-card-title>
      <v-row>
        <v-col cols="12" sm="4">
          <v-select v-model="selectedAssetId" :items="assetOptions" item-title="name" item-value="id" label="Demirbaş" />
        </v-col>
        <v-col cols="12" sm="4">
          <v-select v-model="selectedPersonnelId" :items="personnelOptions" item-title="fullName" item-value="id" label="Personel" />
        </v-col>
        <v-col cols="12" sm="4" class="d-flex align-center">
          <v-btn color="primary" @click="handleAssign">Zimmet Ver</v-btn>
        </v-col>
      </v-row>
      <v-alert v-if="errorMessage" type="error" density="compact" class="mt-2">{{ errorMessage }}</v-alert>
    </v-card>

    <h3 class="mb-2">Zimmet Geçmişi</h3>
    <v-data-table :headers="headers" :items="assignments" :loading="loading">
      <template #item.status="{ item }">
        <v-chip :color="item.returnedDate ? 'grey' : 'success'" size="small">
          {{ item.returnedDate ? 'İade Edildi' : 'Aktif' }}
        </v-chip>
      </template>
      <template #item.actions="{ item }">
        <v-btn v-if="authStore.isAdmin && !item.returnedDate" size="small" variant="text" @click="handleReturn(item.id)">
          İade Al
        </v-btn>
      </template>
    </v-data-table>
  </v-container>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { getAssignments, assignAsset, returnAsset } from '@/services/assignmentService'
import { getAssets } from '@/services/assetService'
import { getPersonnel } from '@/services/personnelService'

const authStore = useAuthStore()
const assignments = ref([])
const assetOptions = ref([])
const personnelOptions = ref([])
const selectedAssetId = ref(null)
const selectedPersonnelId = ref(null)
const loading = ref(false)
const errorMessage = ref('')

const headers = [
  { title: 'Demirbaş', key: 'assetName' },
  { title: 'Personel', key: 'personnelName' },
  { title: 'Atama Tarihi', key: 'assignedDate' },
  { title: 'Durum', key: 'status' },
  { title: 'İşlemler', key: 'actions', sortable: false },
]

async function loadAll() {
  loading.value = true
  assignments.value = await getAssignments()
  assetOptions.value = await getAssets()
  personnelOptions.value = await getPersonnel()
  loading.value = false
}

async function handleAssign() {
  errorMessage.value = ''
  if (!selectedAssetId.value || !selectedPersonnelId.value) {
    errorMessage.value = 'Demirbaş ve personel seçmelisiniz.'
    return
  }
  try {
    await assignAsset({ assetId: selectedAssetId.value, personnelId: selectedPersonnelId.value })
    selectedAssetId.value = null
    selectedPersonnelId.value = null
    await loadAll()
  } catch (err) {
    errorMessage.value = err.response?.data?.message || 'İşlem başarısız.'
  }
}

async function handleReturn(assignmentId) {
  await returnAsset(assignmentId)
  await loadAll()
}

onMounted(loadAll)
</script>
```

### D.5.5 — Faz 2'yi Test Etme

1. Backend ve frontend'i birlikte çalışır durumda tutun.
2. Login olun, **Demirbaşlar** sayfasına gidin, "Yeni Demirbaş" ile bir kayıt ekleyin → tabloda görünmeli.
3. Düzenle/Sil butonlarını deneyin.
4. **Personel** sayfasında bir personel ekleyin.
5. **Zimmet İşlemleri** sayfasında demirbaşı personele atayın → geçmiş tablosunda "Aktif" durumda görünmeli.
6. Aynı demirbaşı tekrar atamayı deneyin → backend'den dönen 409 hatası ekranda kırmızı uyarı olarak görünmeli.
7. "İade Al" butonuna basın → durum "İade Edildi" olarak güncellenmeli.

**Frontend Faz 2 tamamlandı.**

## D.6 — FAZ 3: Yetkilendirme ve Rol Bazlı Görünürlük (Frontend)

Bu bölümde büyük ölçüde **zaten kodun içine gömülü** olan mantığı gözden geçireceğiz ve tamamlayacağız.

### D.6.1 — Buton görünürlüğü kontrolü

Yukarıdaki `Assets.vue`, `Personnel.vue`, `Assignments.vue` dosyalarında `v-if="authStore.isAdmin"` ile zaten admin olmayan kullanıcılardan ekleme/silme/zimmet verme butonlarını gizledik (D.5.2–D.5.4). Ekstra bir şey yapmanıza gerek yok, sadece bunun çalıştığını test edeceksiniz.

### D.6.2 — Route meta ile admin-özel sayfa ayrımı (opsiyonel ileri seviye)

Eğer tamamen admin'e özel bir sayfa eklemek isterseniz (örneğin bir "Kullanıcı Yönetimi" ekranı), `router/index.js`'e şu deseni uygulayın:
```javascript
{ path: '/admin-only', component: () => import('@/views/AdminOnly.vue'), meta: { requiresAuth: true, requiresAdmin: true } }
```
ve `router.beforeEach` fonksiyonunu güncelleyin:
```javascript
router.beforeEach((to) => {
  const authStore = useAuthStore()
  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    return { name: 'login' }
  }
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    return { name: 'dashboard' }   // admin değilse dashboard'a geri gönder
  }
  return true
})
```
Roadmap'inizin mevcut kapsamında (Assets/Personnel/Assignments sayfaları herkese açık, sadece butonlar gizli) bu adım zorunlu değil, bilgi amaçlı verildi.

### D.6.3 — 403 durumunda kullanıcıya anlaşılır mesaj

Backend'den 403 dönebilecek senaryolar için (örn. bir user token'ı manipüle edip doğrudan API'ye istek atarsa), `src/services/api.js`'deki response interceptor'ını genişletin:
```javascript
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      const authStore = useAuthStore()
      authStore.logout()
      router.push('/login')
    }
    if (error.response && error.response.status === 403) {
      alert('Bu işlem için yetkiniz bulunmuyor.')
    }
    return Promise.reject(error)
  }
)
```

### D.6.4 — Faz 3'ü Test Etme

1. Admin kullanıcıyla login olun → tüm ekleme/düzenleme/silme/zimmet butonlarını görmelisiniz.
2. Çıkış yapıp, User rolündeki kullanıcıyla login olun → aynı sayfalarda bu butonların **görünmediğini** doğrulayın.
3. Listeleme işlevlerinin (Demirbaş/Personel/Zimmet geçmişi görüntüleme) User için de çalıştığını doğrulayın.

**Frontend Faz 3 tamamlandı — proje uçtan uca bitti.**

---

# BÖLÜM E — Uçtan Uca Çalıştırma ve Test

Projeyi her açtığınızda bu sırayı izleyin:

1. **Terminal 1 — Backend:**
   ```bash
   cd DemirbasTakipSistemi/DemirbasTakip.Api
   dotnet run
   ```
2. **Terminal 2 — Frontend:**
   ```bash
   cd DemirbasTakipSistemi/demirbas-takip-web
   npm run dev
   ```
3. Tarayıcıda `http://localhost:5173` açın.
4. SQL Server Docker ile çalışıyorsa, konteynerin ayakta olduğundan emin olun: `docker ps`. Kapalıysa: `docker start sqlserver`.

## Uçtan Uca Kabul Senaryosu (roadmap'inizdeki minimum test senaryolarının frontend üzerinden çalıştırılması)

| # | Senaryo | Beklenen |
|---|---|---|
| 1 | Yanlış şifre ile login | Ekranda kırmızı hata mesajı, giriş yapılmaz |
| 2 | Login olmadan `/assets` adresine elle gitme | Otomatik olarak `/login`'e yönlendirilir |
| 3 | Admin ile demirbaş ekleme | Kayıt tabloya düşer |
| 4 | User ile admin butonlarını arama | Butonlar hiç görünmez |
| 5 | Aktif zimmetteki ürünü tekrar verme | Ekranda "zaten aktif zimmette" hatası |
| 6 | İade sonrası tekrar zimmet verme | Başarılı, tabloda "Aktif" durumuna döner |

---

# BÖLÜM F — Kabul Kriterleri Kontrol Listesi (Görev Dokümanına Göre)

| Madde (görev dokümanından) | Nerede karşılandı |
|---|---|
| Kullanıcı giriş yapabilmeli ve token alabilmeli | B.5 (backend) + D.4 (frontend) |
| Giriş yapmayan kullanıcı korumalı sayfaya erişememeli | B.5.6 `[Authorize]` + D.4.6 router guard |
| Demirbaş CRUD işlemleri çalışmalı | B.6.5 + D.5.2 |
| Personel seçilerek zimmet verilebilmeli | B.6.7 + D.5.4 |
| Zimmet iade işlemi yapılabilmeli | B.6.7 (`/return`) + D.5.4 |
| Admin ve standart kullanıcı yetkileri ayrılmalı | B.7 + D.6 |
| Axios interceptor ile token otomatik eklenmeli | D.3 |
| Router guard ile korumalı sayfa engeli | D.4.6 |
| Logout ile oturum temizlenmeli | D.4.1 `logout()` + D.4.5 |
| v-data-table ile listeleme | D.5.2, D.5.3, D.5.4 |
| Kod tek dosyada toplanmamalı, katmanlı yapı | Backend: Controllers/Services/Entities/DTOs ayrımı (B.1–B.7). Frontend: views/services/stores ayrımı (D.1–D.6) |
| Şifreler hash'li tutulmalı | B.5.5 (BCrypt) |
| Zimmet geçmişi silinmemeli, hareket kaydı tutulmalı | B.6.7 — `ReturnedDate` güncellenir, kayıt silinmez |

---

# BÖLÜM G — Sorun Giderme

| Karşılaştığınız durum | Muhtemel sebep ve çözüm |
|---|---|
| `dotnet ef` komutu bulunamadı | `dotnet tool install --global dotnet-ef` çalıştırın, terminali yeniden başlatın. |
| Migration'da "Cannot open database" hatası | SQL Server servisi/konteyneri kapalı. `docker ps` ile kontrol edin, kapalıysa `docker start sqlserver`. Connection string'deki port/şifreyi appsettings.json ile karşılaştırın. |
| Frontend'den backend'e istek atınca "CORS" hatası konsolda kırmızı çıkıyor | `Program.cs`'deki `WithOrigins` adresi frontend'in gerçek adresiyle (port dahil) birebir aynı mı kontrol edin; `app.UseCors(...)` satırının `UseAuthentication()`'dan önce olduğundan emin olun. |
| Login sonrası her istek 401 dönüyor | Token süresi dolmuş olabilir (appsettings.json'daki `ExpireMinutes`), tekrar login olun. Ayrıca `Authorization` header'ının `Bearer <token>` formatında (araya tek boşluk) gittiğinden emin olun. |
| `[Authorize(Roles="Admin")]` olan endpoint'e admin token'la bile 403 dönüyor | JWT claim'inde rol adı büyük/küçük harf veya farklı yazılmış olabilir (`Admin` vs `admin`) — `Roles` tablosundaki `Name` alanı ile `[Authorize(Roles="...")]` içindeki değerin birebir aynı olduğundan emin olun. |
| `https://localhost:5001` tarayıcıda "Bağlantınız güvenli değil" uyarısı veriyor | Yerel geliştirme sertifikasını güvenilir kılın: `dotnet dev-certs https --trust` komutunu çalıştırın. |
| npm run dev çalışmıyor, "vite command not found" | `npm install` komutunu proje klasöründe tekrar çalıştırın. |
| v-data-table'da veri görünmüyor ama network sekmesinde 200 dönüyor | `headers` dizisindeki `key` değerleri ile backend'den gelen JSON alan adlarının (camelCase) birebir eşleştiğinden emin olun. |
| `AllowAnyOrigin` yerine neden `WithOrigins` kullandık | Kimlik bilgisi (token/cookie) taşıyan isteklerde tarayıcılar güvenlik gereği `AllowAnyOrigin` ile credential'lı istekleri reddeder; bu yüzden spesifik origin belirtmek zorunludur. |

---

### Kapanış

Bu rehberi baştan sona sırasıyla uyguladığınızda, staj görev dokümanınızdaki 3 fazın (Login, CRUD, Yetkilendirme) hem backend hem frontend tarafı tamamlanmış, birbirine bağlı, çalışan bir sistem elde etmiş olursunuz. Bir adımda hata alırsanız, hangi bölüm/adım numarasında olduğunuzu ve tam hata mesajını paylaşın; birlikte çözelim.

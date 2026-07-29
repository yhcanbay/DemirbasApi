// Program.cs: Uygulamanın giriş noktası ve konfigürasyon merkezi.
// Spring Boot'taki @SpringBootApplication + SecurityConfig + application.properties'in toplamı.
// .NET 6+'dan itibaren ayrı Startup.cs yok; her şey burada tek dosyada yapılır.

using Microsoft.AspNetCore.Authentication.JwtBearer;  // JWT kimlik doğrulaması için
using Microsoft.EntityFrameworkCore;                   // UseSqlServer() için
using Microsoft.IdentityModel.Tokens;                  // TokenValidationParameters için
using System.Text;                                     // Encoding.UTF8 için
using DemirbasTakip.Api.Data;                          // AppDbContext için
using DemirbasTakip.Api.Auth;                          // TokenService için
using DemirbasTakip.Api.Services;                      // IAuthService, AuthService için
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authorization;                               // Modern API dokümantasyon arayüzü

// ============================================================
// BÖLÜM 1: Builder — Servisler (DI Container'a kayıt)
// Spring'deki ApplicationContext'e bean tanımlamak gibi.
// ============================================================
var builder = WebApplication.CreateBuilder(args);

// --- Controller'ları kaydet ---
// [ApiController] attribute'lü tüm sınıflar otomatik bulunup endpoint olarak açılır.
// Spring'deki @ComponentScan + @Controller gibi düşün.
builder.Services.AddControllers();

// --- Native .NET 10 OpenAPI desteğini ekle ---
// Swagger yerine .NET 10'un yerleşik OpenAPI şema üreticisini kullanıyoruz.
// /openapi/v1.json adresinde JSON şemasını yayımlar.
builder.Services.AddOpenApi();

// --- Veritabanı bağlantısını kaydet ---
// appsettings.json'daki "DefaultConnection" string'ini kullanarak SQL Server'a bağlanır.
// Spring'deki @EnableJpaRepositories + DataSource bean'ine karşılık gelir.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Uygulama servislerini DI container'a kaydet ---
// AddScoped<T>() = her HTTP isteği için yeni bir örnek oluşturulur (Spring'deki @Scope("request")).
// AddSingleton<T>() olsaydı tüm uygulama boyunca tek örnek olurdu.
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// NOT: Faz 2'de AssetService, PersonnelService, AssignmentService buraya eklenecek.

// --- JWT kimlik doğrulamasını yapılandır ---
// Spring Security'deki JwtAuthenticationFilter + SecurityFilterChain karşılığı.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Gelen her HTTP isteğindeki Authorization: Bearer <token> header'ı bu parametrelerle doğrulanır.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,              // Token doğru API'den mi geldi?
            ValidateAudience = true,            // Token doğru istemciye mi yönlendirilmiş?
            ValidateLifetime = true,            // Token süresi dolmuş mu?
            ValidateIssuerSigningKey = true,    // Token imzası geçerli mi? (sahte token tespiti)
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// --- Yetkilendirme sistemini etkinleştir ---
// [Authorize] ve [Authorize(Roles = "Admin")] attribute'lerinin çalışması için gerekli.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// --- CORS (Cross-Origin Resource Sharing) ---
// Tarayıcı güvenlik politikası: frontend (Vue, localhost:5173) farklı port'tan backend'e istek atabilsin.
// Spring'deki @CrossOrigin veya WebMvcConfigurer.addCorsMappings'e benzer.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // Vite dev server varsayılan portu
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================================
// BÖLÜM 2: App — Middleware Pipeline (sıralama ÖNEMLİ!)
// Spring'deki Filter Chain gibi; her istek bu sırayla işlenir.
// ============================================================
var app = builder.Build();

// Geliştirme ortamında API dokümantasyonunu aç.
if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON şemasını /openapi/v1.json adresinde yayımla.
    app.MapOpenApi();

    // Scalar: modern, kullanıcı dostu API test arayüzü.
    // https://localhost:5001/scalar/v1 adresinden erişilir.
    // Swagger UI'ya çok benzer ama daha modern görünümlü.
    // Authentication: Scalar arayüzünde sağ üstten "Bearer" token girebilirsiniz.
    app.MapScalarApiReference(options =>
    {
        options.Title = "Demirbaş Takip API";
        // JWT ile kimlik doğrulama — Scalar'ın built-in authentication desteği
        options.Authentication = new ScalarAuthenticationOptions
        {
            // Bearer token ile kimlik doğrulama — Scalar arayüzünde sağ üstten token girebilirsiniz.
            PreferredSecuritySchemes = ["Bearer"]
        };
    });
}

// HTTP isteklerini HTTPS'e yönlendir.
// Sadece HTTPS yapılandırıldığında anlamlıdır; "http" profiliyle çalışırken bu satır
// "Failed to determine the https port" uyarısı verir — bu nedenle production için bırakılıyor
// ama geliştirme sırasında "dotnet run --launch-profile https" komutuyla HTTPS'i açabilirsiniz.
if (!app.Environment.IsDevelopment())
{
    // Production'da HTTP→HTTPS yönlendirmesi zorlanır.
    app.UseHttpsRedirection();
}

// CORS: UseAuthentication'dan ÖNCE olmalı
app.UseCors("FrontendPolicy");

// Kimlik doğrulama: JWT token doğrulanır.
// UseAuthentication, UseAuthorization'dan ÖNCE gelmeli — sıra kritik!
app.UseAuthentication();

// Yetkilendirme: [Authorize] attribute'lü endpoint'lere erişim kontrolü yapılır.
app.UseAuthorization();

// Controller sınıflarındaki [HttpGet], [HttpPost] vs. attribute'ler çalışsın.
app.MapControllers();

// ============================================================
// B.5.9 — Geçici Hash Üretme Endpoint'i
// İlk admin kullanıcısını elle ekleyebilmek için BCrypt hash üretir.
// Kullanım: tarayıcıda https://localhost:5001/gecici-hash-uret/Admin123! adresine git.
// ÖNEMLİ: Admin'i veritabanına ekledikten sonra bu satırı SİL!
// Production'da böyle bir endpoint bırakılmaz.
// ============================================================
app.MapGet("/gecici-hash-uret/{sifre}", (string sifre) => BCrypt.Net.BCrypt.HashPassword(sifre));

app.Run();

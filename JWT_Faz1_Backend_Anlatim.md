# Faz 1 Backend: .NET ile JWT Mantigi

Spring bildigin icin en temiz karsilastirma su:

.NET'teki JWT yapisi, Spring Security'deki `SecurityConfig + JwtTokenProvider + AuthService + AuthController` duzeninin neredeyse aynisidir.

Projede JWT akisi su dosyalarda kuruludur:

- `DemirbasTakip.Api/Program.cs`: Security ayarlari, middleware sirasi
- `DemirbasTakip.Api/Auth/TokenService.cs`: JWT uretimi
- `DemirbasTakip.Api/Services/impl/AuthService.cs`: kullanici adi/sifre kontrolu
- `DemirbasTakip.Api/Controllers/AuthController.cs`: `/api/auth/login` ve `/api/auth/me`
- `DemirbasTakip.Api/appsettings.json`: JWT secret key, issuer, audience, sure

## JWT Nedir?

JWT, login basarili olunca backend'in kullaniciya verdigi imzali bir kimlik karti gibi dusunulebilir.

Login olurken:

```http
POST /api/auth/login
```

Kullanici adi ve sifre gider. Backend kontrol eder. Dogruysa soyle bir cevap doner:

```json
{
  "token": "xxxxx.yyyyy.zzzzz",
  "username": "admin",
  "role": "Admin"
}
```

Frontend bu token'i saklar. Sonraki isteklerde sunu gonderir:

```http
Authorization: Bearer xxxxx.yyyyy.zzzzz
```

Backend de protected endpoint'lerde bu token gecerli mi diye kontrol eder.

Spring karsiligi:

```java
Authorization: Bearer token
JwtAuthenticationFilter
SecurityContextHolder
@PreAuthorize
```

.NET karsiligi:

```csharp
Authorization: Bearer token
UseAuthentication()
HttpContext.User
[Authorize]
```

## 1. Paketler

Projede su paketler var:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
<PackageReference Include="BCrypt.Net-Next" />
```

`Microsoft.AspNetCore.Authentication.JwtBearer`, backend'e gelen `Bearer token` header'ini okuyup dogrular.

`BCrypt.Net-Next`, sifre hash dogrulamasi yapar. Spring'deki `BCryptPasswordEncoder.matches()` gibi dusunebilirsin.

## 2. appsettings.json

JWT ayarlari burada tutulur:

```json
"Jwt": {
  "Key": "r0StrcgjP8w6hQRUt9vk4x2ImNa5IJo3Ct0KjMAGlQU",
  "Issuer": "DemirbasTakip.Api",
  "Audience": "DemirbasTakip.Client",
  "ExpireMinutes": 60
}
```

Bunlarin anlami:

- `Key`: Token'i imzalamak icin kullanilan gizli anahtar. Spring'deki `jwt.secret`.
- `Issuer`: Token'i kim uretti? Bu API.
- `Audience`: Token kimin icin uretildi? Frontend client.
- `ExpireMinutes`: Token kac dakika gecerli?

Onemli nokta: JWT sifrelenmez, imzalanir. Yani token icindeki bilgiler decode edilebilir ama degistirilemez. Degistirilirse imza bozulur.

## 3. Program.cs Icinde JWT Ayari

Bu bolum .NET tarafinda Spring Security config gibi calisir:

```csharp
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
```

Bu kod sunu soyler:

"Benim authentication sistemim JWT Bearer olacak. Gelen token'in issuer, audience, sure ve imzasini kontrol et."

Spring benzeri:

```java
http
  .oauth2ResourceServer()
  .jwt();
```

veya custom JWT filter yazdigin yapi.

## 4. Middleware Sirasi

`Program.cs` icinde su sira cok onemlidir:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Anlami:

- `UseAuthentication`: Token'i oku, dogrula, kullaniciyi olustur.
- `UseAuthorization`: Bu kullanici bu endpoint'e girebilir mi kontrol et.
- `MapControllers`: Controller endpoint'lerini calistir.

Spring'deki filter chain gibi dusun. Once authentication, sonra authorization.

## 5. TokenService: Token Ureten Sinif

`TokenService.cs` icinde token'in icine claim'ler eklenir:

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Role, user.Role.Name)
};
```

Claim, token'in icine koydugun kullanici bilgisidir.

Ornek claim'ler:

- Kullanici ID
- Kullanici adi
- Rol

Spring'de JWT icine koydugun `subject`, `roles`, `userId` gibi bilgilerle ayni mantiktir.

Sonra token imzalanir:

```csharp
var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
```

Burada backend gizli key ile token'i imzalar. Daha sonra gelen isteklerde ayni key ile "bu token gercekten bizden mi cikmis?" diye kontrol eder.

## 6. AuthService: Login Is Mantigi

`AuthService.cs` login'in asil yeridir.

Akis su sekildedir:

```csharp
var user = await _context.Users
    .Include(u => u.Role)
    .FirstOrDefaultAsync(u => u.Username == username);
```

Bu, kullaniciyi veritabanindan ceker. `Include(u => u.Role)` kismi role bilgisini de getirir.

Spring karsiligi:

```java
userRepository.findByUsername(username)
```

Eger kullanici yoksa:

```csharp
if (user is null) return null;
```

Sifre kontrolu:

```csharp
if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
```

Spring karsiligi:

```java
passwordEncoder.matches(rawPassword, user.getPasswordHash())
```

Basariliysa:

```csharp
var token = _tokenService.CreateToken(user);
return new LoginResponseDto(token, user.Username, user.Role.Name);
```

Yani login basariliysa token uretilir ve frontend'e doner.

## 7. AuthController: Disari Acilan Endpoint

`AuthController.cs` icinde login endpoint'i vardir:

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
```

Bu endpoint'in yolu:

```http
POST /api/auth/login
```

Body:

```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

Eger bilgiler dogruysa `200 OK`, yanlissa `401 Unauthorized` doner.

Bir de test icin guzel endpoint var:

```csharp
[HttpGet("me")]
[Authorize]
public IActionResult Me()
```

Buradaki `[Authorize]` cok onemlidir.

Spring karsiligi:

```java
@PreAuthorize("isAuthenticated()")
```

veya security config'te authenticated endpoint.

Bu endpoint'e token olmadan gidersen `401` alirsin. Token ile gidersen kullanici bilgisini doner.

## 8. Protected Endpoint Nasil Yazilir?

Mesela ileride demirbaslari sadece giris yapan kullanici gorsun istersen:

```csharp
[HttpGet]
[Authorize]
public IActionResult GetAssets()
{
    return Ok();
}
```

Sadece Admin girsin istersen:

```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public IActionResult CreateAsset()
{
    return Ok();
}
```

Bu role bilgisi token icindeki su claim'den gelir:

```csharp
new Claim(ClaimTypes.Role, user.Role.Name)
```

Yani `Admin` rolu token'a yazilir, `[Authorize(Roles = "Admin")]` da onu kontrol eder.

## Kafada Oturacak Buyuk Resim

Login akisi:

```text
Frontend login formu
        ↓
POST /api/auth/login
        ↓
AuthController
        ↓
AuthService
        ↓
Users tablosundan kullanici cekilir
        ↓
BCrypt ile sifre kontrol edilir
        ↓
TokenService JWT uretir
        ↓
Frontend token'i alir
```

Korumali endpoint akisi:

```text
Frontend Authorization: Bearer token gonderir
        ↓
UseAuthentication token'i dogrular
        ↓
UseAuthorization [Authorize] kontrolunu yapar
        ↓
Controller calisir
```

Dotnet tarafini Spring'e cevirirsek:

```text
Program.cs                   = SecurityConfig + application.properties
TokenService                 = JwtTokenProvider
AuthService                  = UserDetailsService/AuthService
AppDbContext                 = EntityManager/JPA Repository altyapisi
[Authorize]                  = @PreAuthorize / authenticated rule
HttpContext.User             = SecurityContextHolder.getAuthentication()
BCrypt.Verify                = BCryptPasswordEncoder.matches()
```

## Faz 1 Icin Ogrenmen Gereken Oz

Faz 1 backend JWT icin temel mantik sudur:

1. Kullanici adi ve sifreyi al.
2. Kullaniciyi veritabaninda bul.
3. BCrypt ile sifreyi dogrula.
4. Dogruysa JWT uret.
5. Frontend token'i sonraki isteklerde `Authorization: Bearer <token>` olarak gondersin.
6. Backend `[Authorize]` ile endpoint'leri korusun.
7. Rol gerekiyorsa `[Authorize(Roles = "Admin")]` kullan.

Projendeki temel iskelet dogru kurulmus. Bundan sonra Faz 1'i saglamlastirmak icin genelde admin kullanicisini seed etmek, gecici hash endpoint'ini kaldirmak, role bazli endpointleri belirlemek ve login testlerini yapmak gerekir.

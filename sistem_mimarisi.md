# Demirbaş Takip Sistemi — Mimari Özeti

## Proje Tanımı
Şirket içi demirbaş ve zimmet takip sistemi. Vue 3 + .NET 10 Web API + SQL Server + JWT.
Sistem tamamen admin kontrolündedir; self-register yoktur.

---

## Roller ve Erişim Modeli

| Rol | RoleId | Açıklama |
|---|---|---|
| Admin | 1 | Sistemi yöneten kullanıcı. Her endpointe erişir. Personnel kaydı OLMAZ. |
| User | 2 | Sisteme giriş yapan çalışan. Sınırlı erişimi vardır. Personnel kaydı OLMAK ZORUNDA. |

### Yetkilendirme Kuralları (Program.cs)

```
FallbackPolicy = RequireRole("Admin")
  → Hiçbir attribute yazılmayan endpoint'lere sadece Admin erişir.

[Authorize(Policy = "AllowedUser")]
  → Hem Admin hem User erişebilir.

[AllowAnonymous]
  → Token olmadan herkes erişebilir (login, refresh gibi endpoint'ler).
```

---

## User ↔ Personnel İlişkisi (1-to-1)

- **User**: Login hesabı. `Username`, `PasswordHash`, `RoleId` içerir.
- **Personnel**: Çalışan kaydı. `FullName`, `UserId` içerir.
- FK `Personnel.UserId` üzerindedir (Personnel bağımlı taraftır).
- **Admin kullanıcısının Personnel kaydı yoktur** — hiçbir Personnel.UserId, Admin User'a işaret etmez.
- **User rolündeki her kullanıcının bir Personnel kaydı vardır** — admin tarafından oluşturulur.
- Kullanıcı oluşturma akışı: Admin tek bir istek (`CreateUserDto`) ile hem `User` hem `Personnel` kaydını atomik olarak oluşturur.

```
Users tablosu               Personnel tablosu
──────────────              ─────────────────────────────
Id | Username | RoleId      Id | FullName | UserId (FK → Users.Id)
1  | admin    | 1           1  | Ahmet K. | 2
2  | ahmetk   | 2           
```

---

## Katman Mimarisi

```
Controller (IXxxController + impl/XxxController)
    ↓
Service (IXxxService + impl/XxxService)
    ↓
AppDbContext (EF Core — doğrudan DbSet kullanılır, ayrı Repository yok)
    ↓
SQL Server
```

### Klasör Yapısı

```
/Controllers
  IAuthController.cs
  IUserController.cs
  impl/AuthController.cs
  impl/UserController.cs

/Services
  IAuthService.cs
  IUserService.cs
  impl/AuthService.cs
  impl/UserService.cs

/DTOs
  /Request
    LoginDto.cs
    RegisterDto.cs
    /Create
      CreateUserDto.cs      ← Admin'in User+Personnel oluşturması için
      CreateAssetDto.cs
      ...
  /Response
    LoginResponseDto.cs
    UserResponseDto.cs      ← Kullanıcı listesi için
    PersonnelResponseDto.cs
    ...

/Entities
  User.cs          → Users tablosu
  Personnel.cs     → Personnel tablosu (UserId FK)
  Role.cs          → Roles tablosu (seed: Admin=1, User=2)
  RefreshToken.cs  → RefreshTokens tablosu
  Asset.cs         → Assets tablosu
  AssetAssignment.cs → AssetAssignments tablosu
  Department.cs    → Departments tablosu
  PersonnelDepartment.cs → M2N ara tablo

/Data
  AppDbContext.cs   → Tüm ilişki konfigürasyonları ve seed data burada

/Auth
  TokenService.cs  → JWT access token + refresh token üretimi
```

---

## Önemli Konfigürasyonlar (AppDbContext)

### Seed Data
```csharp
Role { Id = 1, Name = "Admin" }
Role { Id = 2, Name = "User" }
```

### İlişki Özeti
| İlişki | Tür | FK Tarafı | OnDelete |
|---|---|---|---|
| Personnel → User | 1:1 | Personnel.UserId | Restrict |
| User → RefreshToken | 1:N | RefreshToken.UserId | Cascade |
| AssetAssignment → Asset | N:1 | AssetAssignment.AssetId | Restrict |
| AssetAssignment → Personnel | N:1 | AssetAssignment.PersonnelId | Restrict |
| PersonnelDepartment → Personnel | N:1 | PersonnelDepartment.PersonnelId | Restrict |
| PersonnelDepartment → Department | N:1 | PersonnelDepartment.DepartmentId | Restrict |

---

## Auth Akışı

```
POST /api/auth/login      [AllowAnonymous] → access token + refresh token döner
POST /api/auth/refresh    [AllowAnonymous] → Rotation: eski token silinir, yeni ikili döner
POST /api/auth/logout     [FallbackPolicy] → kullanıcının tüm refresh token'ları silinir
POST /api/auth/register   [FallbackPolicy] → sadece Admin çağırabilir (yeni User+Personnel oluşturur)
```

---

## Genel Kod Kuralları

- Şifreler **BCrypt** ile hash'lenir, düz metin asla saklanmaz.
- Tüm async metodlar `async Task<T>` döndürür.
- Controller'lar interface implement eder (`IXxxController`).
- Service'ler interface üzerinden DI ile inject edilir.
- DTO'lar `record` sözdizimi ile tanımlanır (immutable).
- Zimmet geçmişi asla silinmez; sadece `ReturnDate` doldurulur.
- EF Core ilişkileri `OnModelCreating` içinde fluent API ile tanımlanır.

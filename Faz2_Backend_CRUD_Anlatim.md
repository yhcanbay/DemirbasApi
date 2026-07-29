# Faz 2 Backend: .NET ile CRUD, EF Core ve Zimmet Mantigi

Faz 1'de login ve JWT tarafini kurmustuk. Faz 2'de artik uygulamanin asil is alanina geciyoruz:

- Demirbas kayitlari
- Personel kayitlari
- Departman/personel iliskileri
- Zimmet atama ve iade islemleri
- CRUD endpointleri

Spring bilen biri icin Faz 2'nin ana karsiligi sudur:

```text
Entity                 = @Entity
DbContext + DbSet      = EntityManager + JpaRepository altyapisi
Service                = @Service
Controller             = @RestController
DTO                    = Request/Response DTO
Migration              = Flyway/Liquibase veya Hibernate schema update mantigi
Include                = JOIN fetch / eager fetch
SaveChangesAsync       = transaction sonunda flush/save
```

Projede Faz 2'nin entity ve migration tarafinda temel parcalar var. Service ve Controller taraflari ise Faz 2'de tamamlanacak asil CRUD katmanidir.

## Faz 2'nin Buyuk Resmi

Bu fazda backend su sorulara cevap vermeye baslar:

1. Sisteme yeni demirbas nasil eklenir?
2. Demirbaslar nasil listelenir, guncellenir, silinir?
3. Personel nasil eklenir ve listelenir?
4. Bir demirbas bir personele nasil zimmetlenir?
5. Zimmetli demirbas tekrar nasil iade edilir?
6. Bir demirbas ayni anda iki kisiye zimmetlenebilir mi?

Ozellikle son soru onemli. Faz 2 sadece CRUD degildir; basit is kurallari da burada baslar.

## 1. Entity Nedir?

Entity, veritabanindaki tabloya karsilik gelen C# sinifidir.

Spring'de:

```java
@Entity
public class Asset {
    @Id
    private Long id;
}
```

.NET / EF Core'da:

```csharp
public class Asset
{
    public int Id { get; set; }
}
```

EF Core, `Id` adli property'yi convention geregi primary key olarak algilar.

Projede Faz 2 icin ana entity'ler sunlar:

- `Asset`: Demirbas
- `Personnel`: Personel
- `Department`: Departman
- `AssetAssignment`: Zimmet kaydi
- `PersonnelDepartment`: Personel-departman gecmisini tutan ara tablo

## 2. Asset Entity: Demirbas

`DemirbasTakip.Api/Entities/Asset.cs` dosyasi bir demirbas kaydini temsil eder.

Ornek alanlar:

```csharp
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

Bunu Spring'deki su entity gibi dusunebilirsin:

```java
@Entity
public class Asset {
    @Id
    private Long id;
    private String code;
    private String name;
    private String category;
    private String status;
    private String serialNumber;

    @OneToMany(mappedBy = "asset")
    private List<AssetAssignment> assignments;
}
```

`Assignments` navigation property'dir. Yani Asset tablosunda direkt boyle bir kolon olmaz; EF Core iliskiyi anlamak icin bunu kullanir.

## 3. Personnel Entity: Personel

`Personnel.cs`, zimmet atanabilecek personeli temsil eder.

```csharp
public class Personnel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public ICollection<PersonnelDepartment> PersonnelDepartments { get; set; } = new List<PersonnelDepartment>();
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}
```

Burada iki iliski var:

- Bir personelin birden fazla zimmet kaydi olabilir.
- Bir personel zaman icinde birden fazla departmanda calismis olabilir.

Spring karsiligi:

```java
@OneToMany(mappedBy = "personnel")
private List<AssetAssignment> assignments;

@OneToMany(mappedBy = "personnel")
private List<PersonnelDepartment> personnelDepartments;
```

## 4. Department ve PersonnelDepartment

Projede departman yapisi su sekilde ayrilmis:

```csharp
public class Department
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public ICollection<PersonnelDepartment> PersonnelDepartments { get; set; } = new List<PersonnelDepartment>();
}
```

`PersonnelDepartment` ise personel ile departman arasindaki ara tablodur:

```csharp
public class PersonnelDepartment
{
    public int Id { get; set; }
    public int PersonnelId { get; set; }
    public int DepartmentId { get; set; }
    public Personnel Personnel { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
```

Bu neden ayri entity?

Cunku sadece "personel hangi departmanda?" bilgisini degil, "ne zaman basladi, ne zaman ayrildi?" bilgisini de tutuyoruz.

Yani bu klasik `ManyToMany` degil, ekstra kolonlari olan explicit junction entity'dir.

Spring'de bu durumda `@ManyToMany` yerine ayri bir entity yazardin:

```java
@Entity
public class PersonnelDepartment {
    @ManyToOne
    private Personnel personnel;

    @ManyToOne
    private Department department;

    private LocalDateTime startDate;
    private LocalDateTime endDate;
}
```

## 5. AssetAssignment: Zimmet Kaydi

Faz 2'nin en onemli entity'si budur.

```csharp
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

Bu tablo sunu tutar:

```text
Hangi demirbas, hangi personele, ne zaman verildi, ne zaman geri alindi?
```

`ReturnedDate == null` ise:

```text
Bu zimmet hala aktif.
```

`ReturnedDate != null` ise:

```text
Bu zimmet kapatilmis/iade edilmis.
```

Bu model cok guzel cunku gecmis kayitlari silmeden saklamani saglar.

## 6. AppDbContext: Veritabani Kapisi

`AppDbContext`, Spring'deki `EntityManager` + repository altyapisina benzer.

Projede su `DbSet`'ler var:

```csharp
public DbSet<User> Users => Set<User>();
public DbSet<Role> Roles => Set<Role>();
public DbSet<Asset> Assets => Set<Asset>();
public DbSet<Personnel> Personnel => Set<Personnel>();
public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
public DbSet<PersonnelDepartment> PersonnelDepartments => Set<PersonnelDepartment>();
public DbSet<Department> Departments => Set<Department>();
```

`DbSet<Asset>` su anlama gelir:

```text
Assets tablosu uzerinde sorgu, ekleme, guncelleme, silme yapabilirim.
```

Spring Data JPA'daki:

```java
public interface AssetRepository extends JpaRepository<Asset, Long> {
}
```

mantigina yakindir.

## 7. Iliski Konfigurasyonu

`AppDbContext.OnModelCreating` icinde iliskiler ayarlaniyor:

```csharp
modelBuilder.Entity<AssetAssignment>()
    .HasOne(zimmet => zimmet.Asset)
    .WithMany(demirbas => demirbas.Assignments)
    .HasForeignKey(zimmet => zimmet.AssetId)
    .OnDelete(DeleteBehavior.Restrict);
```

Bu su demek:

```text
Bir zimmet kaydi bir demirbasa aittir.
Bir demirbasin birden fazla zimmet kaydi olabilir.
AssetId foreign key'dir.
Uzerinde zimmet gecmisi olan demirbas kontrolsuz silinmesin.
```

Diger iliski:

```csharp
modelBuilder.Entity<AssetAssignment>()
    .HasOne(zimmet => zimmet.Personnel)
    .WithMany(personel => personel.Assignments)
    .HasForeignKey(zimmet => zimmet.PersonnelId)
    .OnDelete(DeleteBehavior.Restrict);
```

Bu da:

```text
Bir zimmet kaydi bir personele aittir.
Bir personelin birden fazla zimmet kaydi olabilir.
PersonnelId foreign key'dir.
```

Spring karsiligi:

```java
@ManyToOne
@JoinColumn(name = "asset_id")
private Asset asset;

@ManyToOne
@JoinColumn(name = "personnel_id")
private Personnel personnel;
```

`DeleteBehavior.Restrict`, JPA'da cascade delete vermemeye ve FK kisiti ile silmeyi engellemeye benzer.

## 8. DTO Nedir?

DTO, dis dunyaya acilan veri modelidir.

Entity veritabani modelidir. DTO ise API request/response modelidir.

Neden entity'yi direkt controller'dan dondurmuyoruz?

- Gereksiz alanlari acmamak icin
- Sonsuz navigation loop riskini azaltmak icin
- API contract'ini veritabani modelinden ayirmak icin
- Frontend'e daha temiz JSON vermek icin

Ornek Asset DTO'lari:

```csharp
public record AssetResponseDto(
    int Id,
    string Code,
    string Name,
    string Category,
    string Status,
    string SerialNumber);

public record AssetCreateDto(
    string Code,
    string Name,
    string Category,
    string Status,
    string SerialNumber);
```

Spring'de Lombok record/DTO class gibi dusun:

```java
public record AssetCreateDto(
    String code,
    String name,
    String category,
    String status,
    String serialNumber
) {}
```

## 9. Service Katmani

Service katmani is mantiginin durdugu yerdir.

Controller sadece HTTP ile ilgilenir:

- Request alir
- Service'i cagirir
- HTTP response doner

Service ise asil isi yapar:

- Veritabanina gider
- Entity olusturur
- Is kurali kontrol eder
- DTO'ya cevirir
- Kaydeder

Ornek `IAssetService`:

```csharp
public interface IAssetService
{
    Task<IEnumerable<AssetResponseDto>> GetAllAsync();
    Task<AssetResponseDto?> GetByIdAsync(int id);
    Task<AssetResponseDto> CreateAsync(AssetCreateDto dto);
    Task<bool> UpdateAsync(int id, AssetCreateDto dto);
    Task<bool> DeleteAsync(int id);
}
```

Spring karsiligi:

```java
public interface AssetService {
    List<AssetResponseDto> getAll();
    AssetResponseDto getById(Long id);
    AssetResponseDto create(AssetCreateDto dto);
    boolean update(Long id, AssetCreateDto dto);
    boolean delete(Long id);
}
```

## 10. Asset CRUD Mantigi

Asset icin klasik CRUD akisi:

```text
GET    /api/assets       -> tum demirbaslari listele
GET    /api/assets/{id}  -> tek demirbas getir
POST   /api/assets       -> yeni demirbas ekle
PUT    /api/assets/{id}  -> demirbas guncelle
DELETE /api/assets/{id}  -> demirbas sil
```

Service tarafinda listeleme:

```csharp
public async Task<IEnumerable<AssetResponseDto>> GetAllAsync()
    => await _context.Assets
        .Select(a => new AssetResponseDto(
            a.Id,
            a.Code,
            a.Name,
            a.Category,
            a.Status,
            a.SerialNumber))
        .ToListAsync();
```

Burada `Select`, entity'yi DTO'ya cevirir.

Spring'de:

```java
assetRepository.findAll()
    .stream()
    .map(assetMapper::toDto)
    .toList();
```

Yeni demirbas ekleme:

```csharp
var asset = new Asset
{
    Code = dto.Code,
    Name = dto.Name,
    Category = dto.Category,
    Status = dto.Status,
    SerialNumber = dto.SerialNumber
};

_context.Assets.Add(asset);
await _context.SaveChangesAsync();
```

`SaveChangesAsync`, Spring'deki `repository.save(asset)` veya transaction sonunda flush edilmesi gibi dusunulebilir.

## 11. Personnel CRUD Mantigi

Personel tarafinda minimum Faz 2 endpointleri:

```text
GET  /api/personnel
POST /api/personnel
```

Basit create mantigi:

```csharp
var personnel = new Personnel
{
    FullName = dto.FullName
};

_context.Personnel.Add(personnel);
await _context.SaveChangesAsync();
```

Projede departman yapisi ayri entity'ye alindigi icin personelin departmani string olarak tutulmuyor. Personel-departman atamasi `PersonnelDepartment` uzerinden yapilmali.

## 12. Assignment/Zimmet Servisi

Faz 2'nin en kritik is kurali burada:

```text
Bir demirbas ayni anda sadece bir aktif zimmette olabilir.
```

Aktif zimmet nasil anlasilir?

```csharp
ReturnedDate == null
```

Yeni zimmet verirken once kontrol edilir:

```csharp
bool activeAssignmentExists = await _context.AssetAssignments
    .AnyAsync(a => a.AssetId == dto.AssetId && a.ReturnedDate == null);

if (activeAssignmentExists)
{
    throw new BusinessRuleException("Bu demirbas zaten zimmetli.");
}
```

Sonra zimmet kaydi olusturulur:

```csharp
var assignment = new AssetAssignment
{
    AssetId = dto.AssetId,
    PersonnelId = dto.PersonnelId,
    AssignedDate = DateTime.UtcNow
};

_context.AssetAssignments.Add(assignment);
await _context.SaveChangesAsync();
```

Spring karsiligi:

```java
boolean exists = assignmentRepository
    .existsByAssetIdAndReturnedDateIsNull(assetId);

if (exists) {
    throw new BusinessException("Bu demirbas zaten zimmetli.");
}

assignmentRepository.save(assignment);
```

## 13. Zimmet Iade Mantigi

Iade etmek, zimmet kaydini silmek degildir.

Dogru mantik:

```text
ReturnedDate alanini doldur.
```

Ornek:

```csharp
var assignment = await _context.AssetAssignments
    .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && a.ReturnedDate == null);

if (assignment is null)
{
    return false;
}

assignment.ReturnedDate = DateTime.UtcNow;
await _context.SaveChangesAsync();
return true;
```

Boylece gecmis korunur:

```text
Laptop once Ahmet'teydi, sonra iade edildi, sonra Ayse'ye verildi.
```

Bu raporlama icin cok degerlidir.

## 14. Include Ne Ise Yarar?

EF Core'da navigation property'leri sorguya dahil etmek icin `Include` kullanilir.

Zimmetleri listelerken sadece `AssetId` ve `PersonnelId` yetmez; frontend'e demirbas adi ve personel adi da lazim olur.

```csharp
var assignments = await _context.AssetAssignments
    .Include(a => a.Asset)
    .Include(a => a.Personnel)
    .ToListAsync();
```

Spring karsiligi:

```java
@EntityGraph(attributePaths = {"asset", "personnel"})
List<AssetAssignment> findAll();
```

veya JPQL:

```java
select a from AssetAssignment a
join fetch a.asset
join fetch a.personnel
```

## 15. Controller Katmani

Controller, service'i HTTP endpoint olarak disari acar.

Asset controller ornegi:

```csharp
[ApiController]
[Route("api/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _service;

    public AssetsController(IAssetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssetCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }
}
```

Spring karsiligi:

```java
@RestController
@RequestMapping("/api/assets")
public class AssetController {
    private final AssetService service;

    @GetMapping
    public List<AssetResponseDto> getAll() {
        return service.getAll();
    }

    @PostMapping
    public AssetResponseDto create(@RequestBody AssetCreateDto dto) {
        return service.create(dto);
    }
}
```

`[Authorize]` koyarsan JWT zorunlu olur. Yani Faz 1'de kurdugun login sistemi Faz 2 endpointlerini korumaya baslar.

## 16. Dependency Injection Kaydi

Service yazmak yetmez; .NET DI container'a kaydetmek gerekir.

`Program.cs` icine:

```csharp
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
```

Bu Spring'deki `@Service` anotasyonunun yaptigi ise benzer.

Spring'de class ustune `@Service` koyunca bean olur. .NET'te genelde `Program.cs` icinde elle kaydedersin.

## 17. Migration Mantigi

Entity'leri yazinca veritabani otomatik degismez. EF Core'a migration olusturtman gerekir.

Komut:

```bash
dotnet ef migrations add AddCrudEntities
```

Sonra veritabanina uygula:

```bash
dotnet ef database update
```

Spring karsiligi:

- Hibernate `ddl-auto=update`
- Flyway migration
- Liquibase changelog

EF Core migration dosyasi sunu temsil eder:

```text
C# entity modelindeki degisikligi SQL tablosuna cevirmek icin gereken adimlar.
```

## 18. Faz 2 Endpoint Haritasi

Faz 2 sonunda backend'de kabaca su endpointler beklenir:

```text
Auth:
POST /api/auth/login
GET  /api/auth/me

Assets:
GET    /api/assets
GET    /api/assets/{id}
POST   /api/assets
PUT    /api/assets/{id}
DELETE /api/assets/{id}

Personnel:
GET  /api/personnel
POST /api/personnel

Assignments:
GET  /api/assignments
POST /api/assignments
POST /api/assignments/return
```

Faz 3'te bunlarin role bazli yetkilendirmesi sertlestirilir:

```csharp
[Authorize(Roles = "Admin")]
```

Ama Faz 2 icin minimum olarak `[Authorize]` ile login zorunlu hale getirmek yeterlidir.

## 19. Faz 2'de Sik Dusulen Hatalar

### Entity'yi direkt dondurmek

Yanlis:

```csharp
return Ok(await _context.Assets.ToListAsync());
```

Daha temiz:

```csharp
return Ok(await _context.Assets
    .Select(a => new AssetResponseDto(...))
    .ToListAsync());
```

### SaveChangesAsync unutmak

Yanlis:

```csharp
_context.Assets.Add(asset);
return asset;
```

Dogru:

```csharp
_context.Assets.Add(asset);
await _context.SaveChangesAsync();
return asset;
```

### Aktif zimmet kontrolunu unutmak

Yanlis:

```csharp
_context.AssetAssignments.Add(newAssignment);
```

Dogru:

```csharp
bool exists = await _context.AssetAssignments
    .AnyAsync(a => a.AssetId == dto.AssetId && a.ReturnedDate == null);

if (exists)
{
    throw new BusinessRuleException("Bu demirbas zaten zimmetli.");
}
```

### Iade ederken kaydi silmek

Yanlis:

```csharp
_context.AssetAssignments.Remove(assignment);
```

Dogru:

```csharp
assignment.ReturnedDate = DateTime.UtcNow;
```

## 20. Kafada Oturacak Buyuk Resim

Faz 2 backend akisi:

```text
HTTP Request
        ↓
Controller
        ↓
Service
        ↓
AppDbContext
        ↓
EF Core
        ↓
SQL Server
```

Ornek demirbas ekleme akisi:

```text
POST /api/assets
        ↓
AssetsController.Create
        ↓
AssetService.CreateAsync
        ↓
new Asset olusturulur
        ↓
_context.Assets.Add(asset)
        ↓
SaveChangesAsync
        ↓
AssetResponseDto doner
```

Ornek zimmet atama akisi:

```text
POST /api/assignments
        ↓
AssignmentController.Assign
        ↓
AssignmentService.AssignAsync
        ↓
Aktif zimmet var mi kontrol edilir
        ↓
Yoksa AssetAssignment olusturulur
        ↓
SaveChangesAsync
        ↓
AssignmentResponseDto doner
```

## Spring - .NET Kisa Sozluk

```text
@Entity                         = public class Entity
@Id                             = public int Id { get; set; }
JpaRepository                   = DbSet + DbContext
@Service                        = Service class + Program.cs AddScoped
@RestController                 = [ApiController] + ControllerBase
@RequestMapping("/api/assets")  = [Route("api/assets")]
@GetMapping                     = [HttpGet]
@PostMapping                    = [HttpPost]
@RequestBody                    = [FromBody]
Optional<T>                     = T?
repository.save(entity)         = Add/Update + SaveChangesAsync
findAll()                       = ToListAsync()
findById(id)                    = FindAsync(id)
join fetch                      = Include()
@Transactional                  = EF Core SaveChanges transaction mantigi
```

## Faz 2 Icin Ogrenmen Gereken Oz

Faz 2'nin ozeti sudur:

1. Entity'ler veritabani tablolarini temsil eder.
2. `AppDbContext` bu tablolara ulasmanin kapisidir.
3. DTO'lar API'ye giren/cikan temiz modellerdir.
4. Service katmani is mantigini tasir.
5. Controller katmani HTTP endpointlerini tasir.
6. CRUD islemleri `Add`, `FindAsync`, `Select`, `Remove`, `SaveChangesAsync` ile yapilir.
7. Zimmette en onemli kural: aktif zimmeti olan demirbas tekrar zimmetlenemez.
8. Iade etmek, kaydi silmek degil `ReturnedDate` alanini doldurmaktir.
9. Entity degisikliginden sonra migration ve database update gerekir.
10. Faz 1'deki JWT sistemi, Faz 2 endpointlerini `[Authorize]` ile korur.

Faz 2'yi bitirdiginde artik backend sadece login yapan bir API olmaktan cikar; gercek demirbas, personel ve zimmet operasyonlarini yoneten bir sisteme donusur.

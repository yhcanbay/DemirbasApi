using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Entities;

// "Data" isim uzayına alıyoruz — veritabanı katmanına ait sınıflar buraya girer.
namespace DemirbasTakip.Api.Data;

// AppDbContext = Spring'deki JPA EntityManager'ın karşılığı.
// Tüm veritabanı sorguları bu sınıf üzerinden yapılır.
// DbContext : DbContext — Java'da "extends" yerine C#'ta ":" kullanılır.
public class AppDbContext : DbContext
{
    // Constructor injection: DI container bu options nesnesini doldurur (bkz. Program.cs).
    // "base(options)" = Java'daki "super(options)" gibi üst sınıfa iletiyoruz.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet<T> = Spring Data'daki JpaRepository gibi — EF Core bu tanımlar üzerinden SQL üretir.
    // "=>" sözdizimi expression body: { return Set<User>(); } ile aynı anlama gelir.
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Personnel> Personnel => Set<Personnel>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<PersonnelDepartment> PersonnelDepartments => Set<PersonnelDepartment>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // OnModelCreating = Spring'deki @Entity konfigürasyonunun kod karşılığı.
    // Tablo ilişkilerini, seed datayı ve kısıtlamaları burada tanımlarız.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Başlangıç Verileri (Seed Data) ---
        // Uygulama ilk kurulduğunda Roles tablosuna otomatik Admin ve User rolleri eklenir.
        // Rehber gereği migration sırasında bu kayıtlar veritabanına yazılır.
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );

        // --- AssetAssignment İlişki Konfigürasyonları ---
        // Bir zimmet kaydı bir demirbaşa bağlıdır (N:1).
        // OnDelete(Restrict) = demirbaş silinmek istenirse, üzerinde açık zimmet varken hata verir.
        // Bu, Spring'deki @ManyToOne + cascade = NONE gibi davranır.
        modelBuilder.Entity<AssetAssignment>()
            .HasOne(zimmet => zimmet.Asset)                     // AssetAssignment → Asset (N:1)
            .WithMany(demirbaş => demirbaş.Assignments)         // Asset → AssetAssignment (1:N)
            .HasForeignKey(zimmet => zimmet.AssetId)            // Foreign key sütunu: AssetId
            .OnDelete(DeleteBehavior.Restrict);                 // Sil engellensin

        modelBuilder.Entity<AssetAssignment>()
            .HasOne(zimmet => zimmet.Personnel)                 // AssetAssignment → Personnel (N:1)
            .WithMany(personel => personel.Assignments)         // Personnel → AssetAssignment (1:N)
            .HasForeignKey(zimmet => zimmet.PersonnelId)        // Foreign key sütunu: PersonnelId
            .OnDelete(DeleteBehavior.Restrict);                 // Sil engellensin

        // --- PersonnelDepartment İlişki Konfigürasyonları ---
        // Personnel ↔ Department arasındaki M:N ilişkinin ara tablosu.
        // İki farklı tabloya FK bağlı olduğu için AssetAssignment gibi
        // SQL Server'ın "multiple cascade paths" hatasını önlemek amacıyla
        // OnDelete(Restrict) tanımlanıyor.
        modelBuilder.Entity<PersonnelDepartment>()
            .HasOne(pd => pd.Personnel)                              // PersonnelDepartment → Personnel (N:1)
            .WithMany(p => p.PersonnelDepartments)                   // Personnel → PersonnelDepartment (1:N)
            .HasForeignKey(pd => pd.PersonnelId)                     // Foreign key sütunu: PersonnelId
            .OnDelete(DeleteBehavior.Restrict);                      // Sil engellensin

        modelBuilder.Entity<PersonnelDepartment>()
            .HasOne(pd => pd.Department)                             // PersonnelDepartment → Department (N:1)
            .WithMany(d => d.PersonnelDepartments)                   // Department → PersonnelDepartment (1:N)
            .HasForeignKey(pd => pd.DepartmentId)                    // Foreign key sütunu: DepartmentId
            .OnDelete(DeleteBehavior.Restrict);                      // Sil engellensin

        // --- RefreshToken İlişki Konfigürasyonu ---
        // Bir kullanıcının birden fazla refresh token'ı olabilir (1:N).
        // Kullanıcı silinirse refresh token'ları da otomatik silinsin (Cascade).
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)                    // RefreshToken → User (N:1)
            .WithMany(u => u.RefreshTokens)           // User → RefreshToken (1:N)
            .HasForeignKey(rt => rt.UserId)           // Foreign key sütunu: UserId
            .OnDelete(DeleteBehavior.Cascade);        // Kullanıcı silinince token'lar da silinsin

        // --- Personnel ↔ User 1:1 İlişki Konfigürasyonu ---
        // Personnel "bağımlı" (dependent) taraftır; FK (UserId) Personnel tablosundadır.
        // User tarafında navigation property tanımlamadık — WithOne() boş bırakıldı.
        // OnDelete(Restrict) = User silinmek istenirse önce personel kaydı kaldırılmalıdır.
        modelBuilder.Entity<Personnel>()
            .HasOne(p => p.User)                        // Personnel → User (1:1)
            .WithOne()                                  // User tarafında navigation yok
            .HasForeignKey<Personnel>(p => p.UserId)    // FK sütunu: Personnel.UserId
            .OnDelete(DeleteBehavior.Restrict);         // User silinince personel kalır
    }
}
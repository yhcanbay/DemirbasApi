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
    }
}
// DTO = Data Transfer Object: Controller ile istemci arasında taşınan veri şablonu.
// Entity sınıflarını (veritabanı satırı) doğrudan dışa açmak yerine DTO kullanırız —
// Spring'teki @RequestBody POJO'larla aynı mantık.
namespace DemirbasTakip.Api.DTOs;

// "record" C# 9+ ile gelen immutable (değiştirilemez) bir veri sınıfı türüdür.
// Java'daki record veya Lombok @Value gibi davranır: constructor, equals, toString otomatik üretilir.
// LoginDto(string Username, string Password) yazınca C# otomatik şunu üretir:
//   public string Username { get; init; }
//   public string Password { get; init; }
// Ayrıca JSON deserializasyon (istemciden gelen veriyi okuma) için de kullanılır.
public record LoginDto(string Username, string Password);
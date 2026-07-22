namespace DemirbasTakip.Api.Common;

// Özel istisna sınıfı: iş kuralı ihlallerinde fırlatılır.
// Örnek: "Bu demirbaş zaten zimmette, tekrar zimmetlenemez."
// C#'ta "Exception" = Java'daki "RuntimeException" gibi — checked exception yoktur.
// "Exception" sınıfından kalıtım alarak kendi istisna tipimizi tanımlıyoruz.
public class BusinessRuleException : Exception
{
    // "base(message)" = Java'daki "super(message)" gibi üst sınıfın constructor'ını çağırır.
    // Bu sayede exception.Message ile hata mesajına ulaşılabilir.
    public BusinessRuleException(string message) : base(message) { }
}

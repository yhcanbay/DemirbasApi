namespace DemirbasTakip.Api.Common;

// Tüm API endpoint'lerinin döndürdüğü standart cevap zarfı.
// Success: işlem başarılı mı?
// Message: kullanıcıya gösterilebilecek açıklama mesajı (opsiyonel)
// Data: dönüş verisi (listeleme, tekil kayıt vb. — veri yoksa null)
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }

    // Başarılı cevap: data zorunlu, mesaj opsiyonel
    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    // Hata cevabı: mesaj zorunlu, data yok
    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message };
}

// Veri içermeyen cevaplar için (yalnızca mesaj dönen endpoint'ler).
// Örnek: Create, Update, Delete sonrası "işlem başarılı" mesajı.
public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }

    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message)
        => new() { Success = false, Message = message };
}

# Etkincity Proje Sunum Stratejisi ve Soru-Cevap Rehberi

Bu rehber, projenizi hocanıza en profesyonel ve teknik hakimiyeti yüksek şekilde sunmanız için hazırlanmıştır. Sunuma **veritabanından** başlayarak yukarıya doğru çıkacağız (Bottom-Up Approach).

---

## 1. Giriş: Veritabanı ve Veri Yapısı (Sunum Başlangıcı)
*Hocaya giriş cümlesi: "Hocam, projenin temelini sağlam bir veri yapısı üzerine kurduk. Veritabanı olarak MySQL tercih ettik ve Entity Framework Core kullanarak 'Code-First' yaklaşımıyla ilerledik."*

*   **Code-First Vurgusu:** "Önce C# tarafında sınıflarımızı (Models) tasarladık, ardından Migration'lar aracılığıyla bu yapıları MySQL tablolarına dönüştürdük. Bu bize kod tarafında tam kontrol sağladı."
*   **Tablo İlişkileri:**
    *   **Events:** Etkinliklerin temel bilgilerini tutar.
    *   **Reservations:** Kullanıcıların bilet alımlarını tutar ve `Events` tablosuyla ilişkilidir.
    *   **UserEventViews (Önemli!):** "Bu tablo bizim 'akıllı' sistemimizin kalbi. Kullanıcının hangi etkinliğe ne zaman baktığını kaydediyoruz ki ona özel öneriler sunabilelim."
    *   **Identity Tabloları:** "Güvenlik için ASP.NET Core Identity'nin standart tablolarını kullandık; kullanıcılar, roller ve şifreler burada güvenli bir şekilde saklanıyor."

---

## 2. Mimari Yapı: ASP.NET Core MVC
*"Mimari olarak MVC (Model-View-Controller) desenini kullandık. Bu sayede iş mantığını (Business Logic), veriyi ve arayüzü birbirinden tamamen ayırarak yönetilebilir bir kod yapısı oluşturduk."*

*   **Controller Katmanı:** "Tüm istekler Controller üzerinden geçiyor. Örneğin, ödeme işlemleri ve QR kod üretimi `HomeController` içerisinde asenkron (`async/await`) metodlarla yönetiliyor."
*   **Dependency Injection:** "Veritabanı bağlamını (`ApplicationDbContext`) sınıflara doğrudan değil, .NET'in yerleşik Dependency Injection mekanizmasıyla enjekte ettik. Bu modern yazılım standartlarına uygun bir yaklaşımdır."

---

## 3. Öne Çıkan Teknik Özellikler (Fark Yaratan Kısımlar)
*"Projemizi sıradan bir bilet sitesinden ayıran 3 temel teknik başarıya odaklandık:"*

1.  **Kişiselleştirilmiş Öneri Motoru:** "Kullanıcının geçmişteki izleme verilerini analiz edip, en çok ilgi duyduğu kategoriyi buluyoruz. Ardından MySQL tarafında performanslı bir LINQ sorgusuyla bu kategoriye uygun etkinlikleri ana sayfaya getiriyoruz."
2.  **Dinamik QR Kod Üretimi:** "Bilet satın alındığında, `QRCoder` kütüphanesiyle sunucu tarafında anlık bir QR kod oluşturuluyor. Bu kod bilet bilgilerini içeriyor ve biletin dijital bir kimliği olmasını sağlıyor."
3.  **Hata Toleranslı UI (Image Fallback):** "Web üzerindeki görseller her zaman erişilebilir olmayabilir. Biz hem C# tarafında hem de JavaScript tarafında 'fallback' mekanizması kurduk. Eğer etkinlik resmi kırılırsa, sistem tasarımı bozmadan şık bir yedek görseli otomatik yükler."

---

## 4. Olası Hoca Soruları ve "Bilgili" Cevaplar

**Soru 1: Neden SQL Server değil de MySQL kullandın?**
*   **Cevap:** "Hocam, MySQL açık kaynaklı olması ve hafif yapısıyla web uygulamalarında çok yaygın kullanılıyor. Ayrıca Entity Framework Core ile `Pomelo` kütüphanesi sayesinde tam uyumlu çalışabiliyor. Projenin taşınabilirliği açısından da avantajlı."

**Soru 2: Şifreleri veritabanında nasıl saklıyorsun?**
*   **Cevap:** "Şifreleri asla açık metin (plain-text) olarak tutmuyoruz. ASP.NET Core Identity altyapısı, şifreleri 'Salted Hash' yöntemiyle şifreliyor. Yani veritabanı çalınsa bile şifrelerin çözülmesi imkansıza yakındır."

**Soru 3: EF Core'da 'Lazy Loading' mi 'Eager Loading' mi kullandın?**
*   **Cevap:** "Performans için **Eager Loading** tercih ettim hocam. İlişkili verileri (örneğin Rezervasyon ile birlikte Etkinlik bilgisini) çekerken `.Include()` metodunu kullanarak tek bir SQL sorgusuyla işi bitiriyorum. Bu sayede 'N+1 probleminden' kaçınmış oluyoruz."

**Soru 4: Migration kullanmanın avantajı nedir?**
*   **Cevap:** "Veritabanı şemasındaki değişiklikleri versiyonlamamızı sağlıyor. Takım çalışmasında veya farklı ortamlarda (test/production) veritabanını kodla tamamen aynı seviyeye getirmeyi kolaylaştırıyor."

**Soru 5: Ödeme sayfasında güvenliği nasıl sağladın?**
*   **Cevap:** "Öncelikle `[ValidateAntiForgeryToken]` kullanarak CSRF saldırılarını engelledik. Model bazında validation (doğrulama) yaparak hatalı veya manipüle edilmiş verilerin veritabanına girmesini önledik."

---

## 5. Sunum Kapanışı
*"Sonuç olarak hocam, bu projede sadece bir web sitesi değil; veritabanı yönetimi, asenkron programlama, güvenlik ve kullanıcı deneyimi (UX) prensiplerini bir araya getiren modern bir sistem inşa etmeye çalıştım."*

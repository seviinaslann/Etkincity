# Etkincity - Yeni Nesil Etkinlik ve Bilet Satış Platformu
## Kapsamlı Proje Sunum Raporu (Güncellenmiş)

## 1. Projenin Amacı ve Özeti
**Etkincity**, modern şehir hayatındaki etkinlikleri (konser, tiyatro, festival, spor) dijital bir platformda toplayan, kullanıcıların bu etkinlikleri keşfedip güvenle bilet almalarını sağlayan uçtan uca bir web uygulamasıdır. 

Projenin temel amacı, klasik bilet satış sistemlerini bir adım öteye taşıyarak **kişiselleştirilmiş kullanıcı deneyimi** sunmak ve modern tasarım trendlerini (Glassmorphism) işlevsellik ile birleştirmektir. Proje, hem müşteriler için zengin bir arayüz hem de yöneticiler için güçlü bir kontrol paneli sunmaktadır.

---

## 2. Kullanılan Teknolojiler ve Araçlar

### ⚙️ Backend (Arka Plan)
*   **C# ve .NET 9.0:** Uygulamanın motoru olarak en güncel .NET sürümü tercih edildi.
*   **ASP.NET Core MVC:** Mimari yapı olarak Clean Architecture prensiplerine uygun MVC yapısı kullanıldı.
*   **Entity Framework Core (EF Core):** "Code-First" yaklaşımı ile veritabanı yönetimi sağlandı.
*   **LINQ & Async Programming:** Veritabanı sorgularının performansı için asenkron yapı ve optimize edilmiş LINQ sorguları kullanıldı.

### 🎨 Frontend (Ön Yüz)
*   **Modern CSS & Glassmorphism:** Şeffaf katmanlar ve bulanıklık efektleri ile premium bir görünüm sağlandı.
*   **Bootstrap 5:** Tam mobil uyumluluk (Responsive Design) için kullanıldı.
*   **JavaScript (Vanilla):** Şans çarkı, indirim hesaplama ve dinamik UI güncellemeleri için kullanıldı.
*   **FontAwesome & Google Fonts:** Tipografi ve ikonografi ile görsel zenginlik artırıldı.

### 🗄️ Veritabanı ve Güvenlik
*   **MySQL:** Verilerin güvenli ve ilişkisel bir yapıda tutulması için tercih edildi.
*   **ASP.NET Core Identity:** Kullanıcı kimlik doğrulama, yetkilendirme ve şifreleme altyapısını oluşturur.
*   **Git & GitHub:** Versiyon kontrol sistemi ile kodların güvenliği ve proje takibi sağlandı.

---

## 3. Yeni Eklenen ve Öne Çıkan Özellikler

### 🚀 1. Kişiselleştirilmiş Öneri Sistemi (Recommendation System)
Projenin en dikkat çekici teknik özelliğidir. Kullanıcıların platformdaki davranışları analiz edilir:
*   **Takip Mekanizması:** Kullanıcının incelediği etkinliklerin kategorileri `UserEventView` tablosunda saklanır.
*   **Algoritma:** Kullanıcının en çok ilgilendiği ilk iki kategori belirlenir ve ana sayfada "Sizin İçin Önerilenler" bölümünde bu kategorilere ait güncel etkinlikler öncelikli olarak gösterilir.
*   **Teknik Başarı:** MySQL'in karmaşık LINQ sorgularını çevirme kapasitesine göre optimize edilmiş özel bir sorgu motoru geliştirildi.

### 🖼️ 2. Akıllı Görsel Yönetimi ve Fallback Sistemi
Etkinlik görsellerinin sunumunda "sıfır hata" prensibi uygulandı:
*   **Dinamik Görseller:** Seed verileri 30+ adet yüksek kaliteli, etkinliğin içeriğiyle uyumlu gerçekçi görsellerle güncellendi.
*   **Hata Yakalama (Fallback):** Eğer bir görsel yüklenemezse veya URL hatalıysa, sistem otomatik olarak şık bir placeholder (yedek) görseli devreye sokar. Böylece tasarım asla bozulmaz.

### 🎟️ 3. QR Kodlu Bilet ve Promosyon Sistemi
*   **Benzersiz QR Kod:** Satın alınan her bilet için `QRCoder` kütüphanesi ile anlık olarak kişiye özel QR kod üretilir.
*   **İndirim Motoru:** "Şans Çarkı"ndan kazanılan veya sisteme tanımlı promosyon kodları (ETK-10, ETK-50 vb.) ödeme sayfasında anlık olarak fiyatı günceller.

---

## 4. Proje Mimarisi (MVC Yapısı)

*   **Models:** `Event`, `Reservation`, `UserEventView`, `EventCategory` gibi sınıflarla veritabanı şeması oluşturuldu.
*   **Views:** `Razor` motoru kullanılarak, sunucu taraflı veriler dinamik HTML'e dönüştürüldü. `_Layout.cshtml` ile tüm sayfalarda görsel bütünlük sağlandı.
*   **Controllers:** 
    *   `HomeController`: Tüm müşteri süreçlerini (Arama, Öneri, Rezervasyon, Ödeme) yönetir.
    *   `AdminController`: İstatistik takibi ve etkinlik yönetimini sağlar.

---

## 5. Karşılaşılan Zorluklar ve Çözümler

*   **MySQL Query Translation:** Entity Framework'ün karmaşık koleksiyon sorgularını MySQL'e çevirirken yaşadığı hata, sorgu mantığı "OR" bazlı basit karşılaştırmalara dönüştürülerek çözüldü.
*   **Model Validation:** Rezervasyon sırasında formda gönderilmeyen ancak veritabanında zorunlu olan alanların (ID, Tarih vb.) `ModelState.Remove()` ile validasyondan muaf tutulması sağlanarak form akışı düzeltildi.
*   **Görsel Bütünlük:** Veritabanındaki eksik görsellerin arayüzü bozmaması için JavaScript tabanlı `onerror` event'leri ve C# tabanlı `string.IsNullOrEmpty` kontrolleri birleştirildi.

---

## 6. Sonuç
**Etkincity**, sadece bir bilet satış sitesi değil; veri analizi yapan, kullanıcıyı tanıyan ve ona özel içerik sunan modern bir platformdur. Kullanılan teknolojiler ve çözülen mimari problemler, projenin endüstriyel standartlarda geliştirildiğinin kanıtıdır.

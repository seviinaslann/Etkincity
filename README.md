# Etkincity - Etkinlik Bilet Satış ve Yönetim Sistemi

Etkincity, kullanıcıların çeşitli kategorilerdeki etkinlikleri keşfetmesini, koltuk seçimi yaparak bilet satın almasını ve dijital biletlerini yönetmesini sağlayan modern, yapay zeka destekli bir **ASP.NET Core MVC** web uygulamasıdır.

Bu platform sayesinde kullanıcılar etkinlikleri inceleyebilir, akıllı öneriler alabilir, interaktif koltuk planı üzerinden yerlerini ayırtıp ödeme yapabilir ve QR kodlu PDF biletlerini indirebilirler. Yönetici (Admin) rolündeki kullanıcılar ise tüm etkinlik ve bilet hareketlerini dashboard üzerinden anlık olarak takip edip yönetebilir.

---

## 🚀 Öne Çıkan Özellikler

### 👤 Kullanıcı & Bilet İşlemleri
* **Koltuk Seçimi & VIP Rezervasyon:** Etkinlik detay sayfasında 60 koltuk kapasiteli interaktif yerleşim şeması bulunur. İlk 30 koltuk **VIP** statüsündedir ve standart fiyata %50 ek ücret yansıtır.
* **Gelişmiş Ödeme ve İndirim Sistemi:** Satın alım aşamasında promosyon kodları kullanılabilir:
  * `ETK-10`, `ETK-20`, `ETK-50`: Sırasıyla %10, %20 ve %50 indirim sağlar.
  * `ETK-FREE`: Bileti tamamen ücretsiz hale getirir.
  * `ETK-VIP`: VIP koltuklar için uygulanan %50 ek ücret farkını sıfırlar.
* **QR Kodlu Dijital Bilet:** Başarıyla ödenen biletler için benzersiz bir rezervasyon kodu (`ETK-XXXXXXX`) ve bilet bilgilerini içeren dinamik bir QR kod üretilir.
* **A5 PDF Bilet İndirme:** Kullanıcılar biletlerini **QuestPDF** kütüphanesi ile üretilen, içerisinde QR kod bulunan profesyonel A5 boyutundaki PDF formatında bilgisayarlarına indirebilir.
* **Akıllı Öneri Sistemi:** Kullanıcıların incelediği etkinlik kategorileri veritabanında (`UserEventViews`) izlenir. En çok ilgi duyulan 2 kategori belirlenerek anasayfada kişiye özel yaklaşan etkinlik önerileri listelenir.

### 🤖 Yapay Zeka Desteği
* **Asena (AI Chat Assistant):** Gemini API entegrasyonuna sahip yapay zeka sohbet robotu. Veritabanındaki **güncel etkinlik listesini, mekanları ve fiyatları** dinamik olarak analiz ederek kullanıcılara bilet alımı, etkinlik kuralları ve platform hakkında samimi bir Türkçe ile yardımcı olur.

### 🔑 Yönetici (Admin) Özellikleri
* **Satış & Finans Dashboard'u:** Toplam etkinlik sayısı, satılan bilet sayısı ve elde edilen toplam gelir dashboard üzerinde grafiksel ve sayısal olarak raporlanır.
* **Etkinlik Yönetimi (CRUD):** Etkinlik ekleme, güncelleme, silme işlemleri yapılabilir. Etkinlik resimleri bilgisayardan yüklenebilir veya harici URL olarak belirtilebilir.
* **Görsel İyileştirme Motoru:** Tek tıkla veritabanındaki tüm eksik veya taslak (Picsum) görsellerini, ilgili etkinlik kategorisine (Konser, Spor, Tiyatro, Festival vb.) uygun profesyonel **Unsplash** görselleriyle benzersiz şekilde günceller.

---

## 🛠️ Kullanılan Teknolojiler

* **Framework:** .NET 9.0 (ASP.NET Core MVC)
* **Veritabanı / ORM:** MySQL & Entity Framework Core (Code-First)
* **Kimlik Doğrulama:** ASP.NET Core Identity (Rol ve Yetki Yönetimi)
* **Kütüphaneler:**
  * **QRCoder:** Biletler için QR kod üretimi
  * **QuestPDF:** Dinamik ve şık PDF bilet tasarımı
  * **HttpClient:** Gemini API (2.5-Flash) entegrasyonu

---

## 💻 Kurulum ve Çalıştırma

### 1. Gereksinimler
* .NET 9.0 SDK
* MySQL Server (veya alternatif bir ilişkisel veritabanı)

### 2. Yapılandırma
`appsettings.json` dosyasını açarak veritabanı bağlantı dizenizi ve Gemini API anahtarınızı tanımlayın:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=etkincity_db;User=root;Password=SIFRENIZ;"
  },
  "GeminiApiKey": "BURAYA_GEMINI_API_ANAHTARINIZI_YAPISTIRIN"
}
```

### 3. Veritabanı Migrasyonları
Terminal veya Package Manager Console üzerinden migrasyonları uygulayarak veritabanı tablolarını oluşturun:

```bash
dotnet ef database update
```

### 4. Uygulamayı Başlatma
Uygulamayı yerel ortamda çalıştırmak için aşağıdaki komutu kullanın:

```bash
dotnet run
```

Tarayıcınızdan `http://localhost:5000` (veya console ekranında belirtilen port) adresine giderek uygulamaya erişebilirsiniz.

### 5. Varsayılan Yönetici Girişi
Sistem ilk kez ayağa kalktığında otomatik olarak bir yönetici hesabı oluşturur:
* **E-posta:** `admin@etkincity.com`
* **Şifre:** `Admin123!`

---

## 📁 Proje Klasör Yapısı

* **Controllers/**: İstekleri karşılayan ve iş mantığını yöneten denetleyiciler (`HomeController`, `AdminController`, `ChatController`, `AccountController`).
* **Models/**: Veri yapıları, veritabanı modelleri ve ViewModel'ler (`Event`, `Reservation`, `UserEventView`).
* **Views/**: Kullanıcı arayüzü şablonları (Razor Views).
* **Data/**: Veritabanı bağlam sınıfı (`ApplicationDbContext`) ve migrasyon dosyaları.
* **wwwroot/**: Statik dosyalar (CSS, JS, resimler ve üçüncü parti kütüphaneler).

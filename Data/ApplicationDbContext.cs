using Etkincity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Etkincity.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<UserEventView> UserEventViews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>().Property(e => e.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Reservation>().Property(r => r.TotalPrice).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Event>().HasData(
            new Event { Id = 1, Title = "Akdamar Adası Yaz Konseri", Description = "Tarihi Akdamar Adası'nda muhteşem bir yaz konseri sizleri bekliyor.", Date = new DateTime(2026, 7, 15, 20, 0, 0), Location = "Akdamar Adası", Price = 250, Category = EventCategory.Konser, ImageUrl = "https://picsum.photos/seed/konser1/600/400" },
            new Event { Id = 2, Title = "Vanspor FK Şampiyonluk Maçı", Description = "Vanspor'un kritik şampiyonluk mücadelesinde takımını yalnız bırakma!", Date = new DateTime(2026, 5, 10, 15, 0, 0), Location = "Van Atatürk Stadyumu", Price = 100, Category = EventCategory.Spor, ImageUrl = "https://picsum.photos/seed/spor1/600/400" },
            new Event { Id = 3, Title = "Hamlet - Modern Uyarlama", Description = "Shakespeare'in ölümsüz eseri Hamlet, modern ve farklı bir yorumla Van'da.", Date = new DateTime(2026, 6, 5, 19, 30, 0), Location = "Van Devlet Tiyatrosu", Price = 150, Category = EventCategory.Tiyatro, ImageUrl = "https://picsum.photos/seed/tiyatro1/600/400" },
            new Event { Id = 4, Title = "Van Gölü Geleneksel Gençlik Festivali", Description = "Müzik, eğlence ve yarışmaların olduğu en büyük gençlik festivali.", Date = new DateTime(2026, 8, 20, 12, 0, 0), Location = "Edremit Sahili", Price = 300, Category = EventCategory.Festival, ImageUrl = "https://picsum.photos/seed/festival1/600/400" },
            new Event { Id = 5, Title = "Geleceğin Teknolojileri Söyleşisi", Description = "Yapay zeka ve gelecek hakkında ufuk açıcı bir söyleşi.", Date = new DateTime(2026, 9, 10, 14, 0, 0), Location = "Van Yüzüncü Yıl Üniversitesi", Price = 50, Category = EventCategory.Soylesi, ImageUrl = "https://picsum.photos/seed/soylesi1/600/400" },
            new Event { Id = 6, Title = "Doğu'nun Kahkahası Stand-Up", Description = "Gülme garantili harika bir stand-up gösterisi.", Date = new DateTime(2026, 10, 12, 20, 30, 0), Location = "Van Kültür Merkezi", Price = 120, Category = EventCategory.StandUp, ImageUrl = "https://picsum.photos/seed/standup1/600/400" },
            new Event { Id = 7, Title = "Urartu Tarihi Resim Sergisi", Description = "Tarihin derinliklerinden gelen muazzam eserler sergisi.", Date = new DateTime(2026, 11, 1, 10, 0, 0), Location = "Van Müzesi", Price = 0, Category = EventCategory.Sergi, ImageUrl = "https://picsum.photos/seed/sergi1/600/400" },
            new Event { Id = 8, Title = "Van Kedisi Güzellik Yarışması", Description = "Gözleriyle ünlü Van kedilerinin eğlenceli podyum heyecanı.", Date = new DateTime(2026, 4, 25, 13, 0, 0), Location = "Van Kedisi Araştırma Merkezi", Price = 30, Category = EventCategory.Diger, ImageUrl = "https://picsum.photos/seed/kedi1/600/400" },
            new Event { Id = 9, Title = "Kışa Veda Rock Konseri", Description = "Bölgenin en sevilen rock grupları sahnede.", Date = new DateTime(2026, 5, 2, 21, 0, 0), Location = "Van Kalesi Açık Hava Sahnesi", Price = 180, Category = EventCategory.Konser, ImageUrl = "https://picsum.photos/seed/rock1/600/400" },
            new Event { Id = 10, Title = "Van Gölü Yüzme Maratonu", Description = "Türkiye'nin dört bir yanından gelen sporcularla yüzme maratonu.", Date = new DateTime(2026, 7, 22, 09, 0, 0), Location = "Tuşba Sahili", Price = 0, Category = EventCategory.Spor, ImageUrl = "https://picsum.photos/seed/yuzme1/600/400" },
            new Event { Id = 11, Title = "Bölgesel Lezzetler Festivali", Description = "Van kahvaltısı ve eşsiz lezzetlerin sunulduğu yemek festivali.", Date = new DateTime(2026, 6, 15, 08, 0, 0), Location = "Van Fuar Alanı", Price = 80, Category = EventCategory.Festival, ImageUrl = "https://picsum.photos/seed/yemek1/600/400" },
            new Event { Id = 12, Title = "Klasik Müzik Akşamı", Description = "Senfoni orkestrası eşliğinde unutulmaz klasik müzik ziyafeti.", Date = new DateTime(2026, 9, 25, 20, 0, 0), Location = "Van Kültür Merkezi", Price = 200, Category = EventCategory.Konser, ImageUrl = "https://picsum.photos/seed/klasik1/600/400" },
            new Event { Id = 13, Title = "Yazar Buluşmaları ve İmza Günü", Description = "Sevilen yazarlarla interaktif söyleşi ve imza günü.", Date = new DateTime(2026, 10, 5, 14, 0, 0), Location = "Van İl Halk Kütüphanesi", Price = 0, Category = EventCategory.Soylesi, ImageUrl = "https://picsum.photos/seed/yazar1/600/400" },
            new Event { Id = 14, Title = "Buz Pateni Gösterisi", Description = "Eğlenceli ve heyecanlı artistik buz pateni gösterisi.", Date = new DateTime(2026, 1, 15, 18, 0, 0), Location = "Van AVM Buz Pisti", Price = 100, Category = EventCategory.Tiyatro, ImageUrl = "https://picsum.photos/seed/buz1/600/400" },
            new Event { Id = 15, Title = "Açık Hava Sinema Gecesi", Description = "Yıldızların altında, Van gölü kenarında klasik film gösterimi.", Date = new DateTime(2026, 8, 10, 21, 0, 0), Location = "Edremit Kent Ormanı", Price = 60, Category = EventCategory.Diger, ImageUrl = "https://picsum.photos/seed/sinema1/600/400" }
        );
    }
}

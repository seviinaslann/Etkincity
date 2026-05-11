using Etkincity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    if (await userManager.FindByEmailAsync("admin@etkincity.com") == null)
    {
        var admin = new IdentityUser { UserName = "admin@etkincity.com", Email = "admin@etkincity.com" };
        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddClaimAsync(admin, new System.Security.Claims.Claim("FullName", "Sistem Yöneticisi"));
        }
    }

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seedEvents = new List<Etkincity.Models.Event>
    {
        new Etkincity.Models.Event { Title = "Mor ve Ötesi Senfonik Konseri", Category = Etkincity.Models.EventCategory.Konser, Price = 750, Date = DateTime.Now.AddDays(10), Location = "Harbiye Açıkhava", Description = "Mor ve Ötesi efsane şarkılarını senfoni orkestrası eşliğinde seslendiriyor.", ImageUrl = "https://loremflickr.com/800/600/concert,rock?lock=1" },
        new Etkincity.Models.Event { Title = "Cem Yılmaz - CMXXIV", Category = Etkincity.Models.EventCategory.StandUp, Price = 1500, Date = DateTime.Now.AddDays(15), Location = "Zorlu PSM", Description = "Ünlü komedyen Cem Yılmaz yepyeni gösterisiyle sahnede.", ImageUrl = "https://loremflickr.com/800/600/standup,comedy?lock=2" },
        new Etkincity.Models.Event { Title = "Fenerbahçe - Galatasaray Derbisi", Category = Etkincity.Models.EventCategory.Spor, Price = 2500, Date = DateTime.Now.AddDays(5), Location = "Şükrü Saracoğlu Stadyumu", Description = "Yılın derbisinde nefesler tutuluyor.", ImageUrl = "https://loremflickr.com/800/600/soccer,stadium?lock=3" },
        new Etkincity.Models.Event { Title = "Zengin Mutfağı Tiyatrosu", Category = Etkincity.Models.EventCategory.Tiyatro, Price = 400, Date = DateTime.Now.AddDays(20), Location = "Maximum Uniq", Description = "Şener Şen'in efsaneleştiği tiyatro oyunu.", ImageUrl = "https://loremflickr.com/800/600/theater,stage?lock=4" },
        new Etkincity.Models.Event { Title = "Kahve Festivali 2026", Category = Etkincity.Models.EventCategory.Festival, Price = 300, Date = DateTime.Now.AddDays(8), Location = "KüçükÇiftlik Park", Description = "Dünyanın en iyi kahve çekirdekleri bu festivalde.", ImageUrl = "https://loremflickr.com/800/600/coffee,festival?lock=5" },
        new Etkincity.Models.Event { Title = "Van Gogh Dijital Sergisi", Category = Etkincity.Models.EventCategory.Sergi, Price = 250, Date = DateTime.Now.AddDays(12), Location = "Cermodern", Description = "Sanatçının eserleri dijital dünyada hayat buluyor.", ImageUrl = "https://loremflickr.com/800/600/art,museum?lock=6" },
        new Etkincity.Models.Event { Title = "İlber Ortaylı Tarih Söyleşisi", Category = Etkincity.Models.EventCategory.Soylesi, Price = 150, Date = DateTime.Now.AddDays(25), Location = "Congresium", Description = "Tarihimizin bilinmeyen yönleri İlber hoca ile konuşulacak.", ImageUrl = "https://loremflickr.com/800/600/lecture,speaker?lock=7" },
        new Etkincity.Models.Event { Title = "Duman Açıkhava Konseri", Category = Etkincity.Models.EventCategory.Konser, Price = 600, Date = DateTime.Now.AddDays(18), Location = "Bostancı Gösteri Merkezi", Description = "Duman en sevilen rock parçalarıyla sahnede.", ImageUrl = "https://loremflickr.com/800/600/concert,band?lock=8" },
        new Etkincity.Models.Event { Title = "Anadolu Efes - Real Madrid Basketbol", Category = Etkincity.Models.EventCategory.Spor, Price = 1200, Date = DateTime.Now.AddDays(3), Location = "Sinan Erdem Spor Salonu", Description = "Euroleague heyecanı İstanbul'da.", ImageUrl = "https://loremflickr.com/800/600/basketball?lock=9" },
        new Etkincity.Models.Event { Title = "Kuğulu Göl Balesi", Category = Etkincity.Models.EventCategory.Tiyatro, Price = 800, Date = DateTime.Now.AddDays(22), Location = "AKM", Description = "Klasikleşmiş Kuğulu Göl balesi muhteşem koreografisiyle.", ImageUrl = "https://loremflickr.com/800/600/ballet,dance?lock=10" },
        new Etkincity.Models.Event { Title = "Sokak Lezzetleri Festivali", Category = Etkincity.Models.EventCategory.Festival, Price = 200, Date = DateTime.Now.AddDays(7), Location = "Kuruçeşme Arena", Description = "Yüzlerce farklı sokak lezzeti bir arada.", ImageUrl = "https://loremflickr.com/800/600/streetfood,festival?lock=11" },
        new Etkincity.Models.Event { Title = "Doğu Demirkol Stand Up", Category = Etkincity.Models.EventCategory.StandUp, Price = 500, Date = DateTime.Now.AddDays(14), Location = "BKM", Description = "Gülmekten karnınıza ağrılar girecek.", ImageUrl = "https://loremflickr.com/800/600/standup,mic?lock=12" },
        new Etkincity.Models.Event { Title = "Antik Mısır Sergisi", Category = Etkincity.Models.EventCategory.Sergi, Price = 300, Date = DateTime.Now.AddDays(30), Location = "İstanbul Modern", Description = "Firavunların gizemli dünyasına yolculuk.", ImageUrl = "https://loremflickr.com/800/600/egypt,museum?lock=13" },
        new Etkincity.Models.Event { Title = "Teoman Akustik Gecesi", Category = Etkincity.Models.EventCategory.Konser, Price = 550, Date = DateTime.Now.AddDays(11), Location = "Jolly Joker", Description = "Teoman'dan en güzel akustik parçalar.", ImageUrl = "https://loremflickr.com/800/600/acoustic,guitar?lock=14" },
        new Etkincity.Models.Event { Title = "Yapay Zeka Teknoloji Zirvesi", Category = Etkincity.Models.EventCategory.Soylesi, Price = 450, Date = DateTime.Now.AddDays(6), Location = "Lütfi Kırdar Kongre Merkezi", Description = "Geleceğin teknolojileri uzmanlar tarafından tartışılıyor.", ImageUrl = "https://loremflickr.com/800/600/technology,conference?lock=15" },
        new Etkincity.Models.Event { Title = "Coldplay Stadyum Konseri", Category = Etkincity.Models.EventCategory.Konser, Price = 3500, Date = DateTime.Now.AddDays(45), Location = "Atatürk Olimpiyat Stadyumu", Description = "Dünyaca ünlü Coldplay grubu dev sahne şovuyla.", ImageUrl = "https://loremflickr.com/800/600/concert,stadium?lock=16" },
        new Etkincity.Models.Event { Title = "Müzikal: Sefiller", Category = Etkincity.Models.EventCategory.Tiyatro, Price = 900, Date = DateTime.Now.AddDays(28), Location = "Zorlu PSM", Description = "Victor Hugo'nun ölümsüz eseri Sefiller müzikali.", ImageUrl = "https://loremflickr.com/800/600/musical,theater?lock=17" },
        new Etkincity.Models.Event { Title = "Beşiktaş - Trabzonspor", Category = Etkincity.Models.EventCategory.Spor, Price = 2200, Date = DateTime.Now.AddDays(13), Location = "Vodafone Park", Description = "Süper Lig'in dev maçında büyük heyecan.", ImageUrl = "https://loremflickr.com/800/600/soccer,match?lock=18" },
        new Etkincity.Models.Event { Title = "Gençlik Müzik Festivali", Category = Etkincity.Models.EventCategory.Festival, Price = 400, Date = DateTime.Now.AddDays(40), Location = "Life Park", Description = "Yazın en büyük gençlik festivali.", ImageUrl = "https://loremflickr.com/800/600/music,festival?lock=19" },
        new Etkincity.Models.Event { Title = "Klasik Otomobiller Sergisi", Category = Etkincity.Models.EventCategory.Sergi, Price = 200, Date = DateTime.Now.AddDays(16), Location = "Tüyap", Description = "Geçmişten günümüze en özel klasik araçlar.", ImageUrl = "https://loremflickr.com/800/600/classic,car?lock=20" },
        new Etkincity.Models.Event { Title = "Tolga Çevik - Tolgshow", Category = Etkincity.Models.EventCategory.StandUp, Price = 800, Date = DateTime.Now.AddDays(21), Location = "Bostancı Gösteri Merkezi", Description = "Tolga Çevik yepyeni doğaçlama gösterisiyle.", ImageUrl = "https://loremflickr.com/800/600/comedy,stage?lock=21" },
        new Etkincity.Models.Event { Title = "Prof. Dr. Celal Şengör Söyleşisi", Category = Etkincity.Models.EventCategory.Soylesi, Price = 200, Date = DateTime.Now.AddDays(35), Location = "ODTÜ Kültür Kongre", Description = "Celal Şengör ile yerbilimleri üzerine keyifli bir sohbet.", ImageUrl = "https://loremflickr.com/800/600/professor,lecture?lock=22" },
        new Etkincity.Models.Event { Title = "E-Spor Şampiyonası Finali", Category = Etkincity.Models.EventCategory.Spor, Price = 300, Date = DateTime.Now.AddDays(24), Location = "Volkswagen Arena", Description = "En iyi takımlar şampiyonluk için yarışıyor.", ImageUrl = "https://loremflickr.com/800/600/esports,gaming?lock=23" },
        new Etkincity.Models.Event { Title = "Açık Hava Sinema Gecesi", Category = Etkincity.Models.EventCategory.Diger, Price = 150, Date = DateTime.Now.AddDays(5), Location = "Kalamış Parkı", Description = "Yıldızların altında nostaljik açık hava sineması keyfi.", ImageUrl = "https://loremflickr.com/800/600/outdoor,cinema?lock=24" },
        new Etkincity.Models.Event { Title = "Yeditepe Tiyatro Günleri", Category = Etkincity.Models.EventCategory.Tiyatro, Price = 250, Date = DateTime.Now.AddDays(19), Location = "Yeditepe Üniversitesi", Description = "Genç tiyatroculardan iddialı oyunlar.", ImageUrl = "https://loremflickr.com/800/600/drama,theater?lock=25" },
        new Etkincity.Models.Event { Title = "Sertab Erener Konseri", Category = Etkincity.Models.EventCategory.Konser, Price = 600, Date = DateTime.Now.AddDays(33), Location = "Harbiye Açıkhava", Description = "Sertab Erener muhteşem sesiyle dinleyicileri büyülüyor.", ImageUrl = "https://loremflickr.com/800/600/singer,concert?lock=26" },
        new Etkincity.Models.Event { Title = "Geleneksel Türk El Sanatları", Category = Etkincity.Models.EventCategory.Sergi, Price = 100, Date = DateTime.Now.AddDays(10), Location = "Atatürk Kültür Merkezi", Description = "Ebru, hat ve çini sanatlarının en güzel örnekleri.", ImageUrl = "https://loremflickr.com/800/600/art,craft?lock=27" },
        new Etkincity.Models.Event { Title = "Fazıl Say Piyano Resitali", Category = Etkincity.Models.EventCategory.Konser, Price = 1000, Date = DateTime.Now.AddDays(26), Location = "Lütfi Kırdar", Description = "Dünyaca ünlü piyanistimiz Fazıl Say'dan müzik ziyafeti.", ImageUrl = "https://loremflickr.com/800/600/piano,concert?lock=28" },
        new Etkincity.Models.Event { Title = "Cimri Tiyatro Oyunu", Category = Etkincity.Models.EventCategory.Tiyatro, Price = 350, Date = DateTime.Now.AddDays(9), Location = "Zorlu PSM", Description = "Molière'in ölümsüz eseri Cimri sahnelerde.", ImageUrl = "https://loremflickr.com/800/600/theater,play?lock=29" },
        new Etkincity.Models.Event { Title = "Türkiye - Almanya Voleybol", Category = Etkincity.Models.EventCategory.Spor, Price = 400, Date = DateTime.Now.AddDays(17), Location = "TVF Burhan Felek", Description = "Filenin Sultanları zorlu rakibi karşısında.", ImageUrl = "https://loremflickr.com/800/600/volleyball,match?lock=30" },
        new Etkincity.Models.Event { Title = "Uluslararası Kitap Fuarı", Category = Etkincity.Models.EventCategory.Sergi, Price = 50, Date = DateTime.Now.AddDays(30), Location = "Tüyap", Description = "Binlerce kitap ve yazar bu fuarda buluşuyor.", ImageUrl = "https://loremflickr.com/800/600/books,fair?lock=31" },
        new Etkincity.Models.Event { Title = "Gülse Birsel Söyleşisi", Category = Etkincity.Models.EventCategory.Soylesi, Price = 250, Date = DateTime.Now.AddDays(22), Location = "BKM", Description = "Gülse Birsel ile mizah dolu bir sohbet.", ImageUrl = "https://loremflickr.com/800/600/talkshow,stage?lock=32" }
    };

    foreach (var ev in seedEvents)
    {
        var existingEvent = dbContext.Events.FirstOrDefault(e => e.Title == ev.Title);
        if (existingEvent == null)
        {
            dbContext.Events.Add(ev);
        }
        else
        {
            // Mevcut etkinliklerin görsellerini güncelle
            existingEvent.ImageUrl = ev.ImageUrl;
            dbContext.Events.Update(existingEvent);
        }
    }
    dbContext.SaveChanges();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

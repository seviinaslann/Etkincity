using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Etkincity.Data;
using Microsoft.EntityFrameworkCore;

namespace Etkincity.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatController(IConfiguration configuration, ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public class ChatPart { public string text { get; set; } = string.Empty; }
    public class ChatMessage { 
        public string role { get; set; } = string.Empty; 
        public List<ChatPart> parts { get; set; } = new(); 
    }

    public class ChatRequest
    {
        public List<ChatMessage> History { get; set; } = new();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        string? apiKey = _configuration["GeminiApiKey"];
        
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "BURAYA_API_ANAHTARINIZI_YAPISTIRIN")
        {
            return Ok(new { response = "Sistem yöneticisi henüz API anahtarını yapılandırmadı. Lütfen appsettings.json dosyasına ücretsiz bir Gemini API Anahtarı ekleyin." });
        }

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";

        // Veritabanındaki gerçek etkinlikleri al
        var events = await _context.Events.AsNoTracking().ToListAsync();
        string eventsContext = "Şu anda platformumuzda bulunan GÜNCEL ETKİNLİKLER şunlardır:\n";
        
        if (events.Any())
        {
            foreach(var e in events)
            {
                eventsContext += $"- Etkinlik Adı: {e.Title}, Kategori: {e.Category}, Fiyat: {e.Price} TL, Tarih: {e.Date.ToString("dd MMM yyyy HH:mm")}, Konum: {e.Location}\n";
            }
        }
        else
        {
            eventsContext += "Şu anda sistemde hiç etkinlik bulunmuyor.\n";
        }

        // Asena'nın karakterini tanımlayan System Prompt
        string systemInstructionText = $"Sen Etkincity adlı etkinlik bilet satış platformunun yapay zeka asistanısın. Adın Asena. " +
                            $"Kullanıcıların etkinlikler, bilet alma, fiyatlar ve üyelik hakkındaki sorularına nazikçe, samimi bir Türkçe ile cevap vermelisin. " +
                            $"Kullanıcı etkinlik sorarsa sadece aşağıda verilen GÜNCEL ETKİNLİKLER listesindeki etkinlikleri söyle, asla uydurma. " +
                            $"Doğal, akıcı, kendini tekrar etmeyen, gerçek bir insan/asistan gibi konuş. " +
                            $"Kullanıcıya her defasında aynı cevabı verme, bağlama göre farklı ve yardımcı yanıtlar ver.\n\n" +
                            eventsContext;

        var payload = new
        {
            systemInstruction = new {
                parts = new[] { new { text = systemInstructionText } }
            },
            contents = request.History
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return Ok(new { response = $"Yapay zeka API'sine bağlanılamadı (HTTP {(int)response.StatusCode}). Lütfen API anahtarınızın geçerli olduğundan emin olun." });
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(responseBody);

            // Güvenli erişim: candidates[0].content.parts[0].text
            if (!jsonDocument.RootElement.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                return Ok(new { response = "Yapay zekadan geçerli bir yanıt alınamadı. Lütfen tekrar deneyin." });
            }

            var answerText = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Yanıt alınamadı.";

            return Ok(new { response = answerText });
        }
        catch (TaskCanceledException)
        {
            return Ok(new { response = "Yapay zeka yanıt verme süresi doldu. Lütfen tekrar deneyin." });
        }
        catch (Exception ex)
        {
            return Ok(new { response = $"Üzgünüm, yapay zeka beynime şu an bağlanamıyorum. Lütfen daha sonra tekrar deneyin. (Hata: {ex.Message})" });
        }
    }
}

using Newtonsoft.Json;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Formatting = Newtonsoft.Json.Formatting; // Для кнопок


namespace hello_tg_bot;

public class Program
{

    private static string token = "8619630882:AAGMAveNrnQEkr_K3-pO9anxbIfNFOkpk04";
    private static string weatherApiKey = "6128acbaf7356965c2b3e16fa8161023";
    private static Dictionary<long, int> userState = new(); // 0-Имя, 1-Фамилия, 2-Дата
    private static string dbPath = "users.json";
    static async Task Main(string[] args) // Добавили async Task для корректной работы
    {
        var bot = new TelegramBotClient(token);
        Console.WriteLine("bot ishladi...");
        bot.StartReceiving(HandleUpdate, HandleError);
        Console.ReadLine();
    }
    static async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message is not { } msg) return;
        long chatId = msg.Chat.Id;

        // --- ЛОГИКА РЕГИСТРАЦИИ ---
        if (!userState.ContainsKey(chatId) && msg.Text == "/start")
        {
            userState[chatId] = 0;
            await bot.SendMessage(chatId, "Hush kelibsiz! Ismingizni kiriting:");
            return;
        }

        if (userState.ContainsKey(chatId))
        {
            await RegistrationStep(bot, msg, ct);
            return;
        }

        // --- ГЛАВНОЕ МЕНЮ ---
        switch (msg.Text)
        {
            case "Ob-havo ☀️":
                string weather = await GetWeather("Tashkent"); // Пример для Ташкента
                await SaveHistory(chatId, $"ob-havo haqida soradi: {weather}");
                await bot.SendMessage(chatId, $"Poytaxtimizda hozr: {weather}");
                break;

            case "Soqqa 💵":
                string rate = await GetCurrency();
                await SaveHistory(chatId, $"kursni kordi: {rate}");
                await bot.SendMessage(chatId, $"Kurs USD: {rate} som");
                break;

            case "Tarix 📜":
                var user = GetUser(chatId);
                string hist = string.Join("\n", user.History.TakeLast(5));
                await bot.SendMessage(chatId, $"Oxirgi xarakatlar:\n{hist}");
                break;
        }
    }

    static async Task RegistrationStep(ITelegramBotClient bot, Message msg, CancellationToken ct)
    {
        long id = msg.Chat.Id;
        var user = GetUser(id);

        if (userState[id] == 0) { user.FirstName = msg.Text; userState[id] = 1; await bot.SendMessage(id, "Familiyangizni kiriting:"); }
        else if (userState[id] == 1) { user.LastName = msg.Text; userState[id] = 2; await bot.SendMessage(id, "Tugulgan sanangiz (k.o.y):"); }
        else if (userState[id] == 2)
        {
            user.BirthDate = msg.Text;
            userState.Remove(id);
            SaveUser(user);

            // Отправка голосового приветствия
            await bot.SendMessage(id, "Muvaffaqiyatli!");
            if (System.IO.File.Exists("salom_golos.ogg"))
                await bot.SendVoice(id, InputFile.FromStream(System.IO.File.OpenRead("salom_golos.ogg")));

            await ShowMenu(bot, id);
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    static async Task ShowMenu(ITelegramBotClient bot, long id)
    {
        var menu = new ReplyKeyboardMarkup(new[] {
            new KeyboardButton[] { "Ob-havo ☀️", "Valyuta 💵" },
            new KeyboardButton[] { "Tarix 📜" }
        })
        { ResizeKeyboard = true };
        await bot.SendMessage(id, "Tanlang:", replyMarkup: menu);
    }

    static async Task<string> GetWeather(string city)
    {
        // Твой ключ из скриншота (лучше вынести в appsettings.json)
        string apiKey = "4e784bae26e9d1734236121ab16cba2";

        try
        {
            using var client = new HttpClient();
            // Формируем запрос: город, ключ, метрическая система (цельсии) и язык (ru)
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=ru";

            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return "⚠️ Xato! API hali activ bolmagan 2 soatdan song urinib koring";
            }

            if (!response.IsSuccessStatusCode)
            {
                return $"⚠️ Server xatosi: {response.StatusCode}";
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonResponse);

            // Извлекаем данные безопасно
            double temp = data.main.temp;
            string description = data.weather[0].description;
            string cityName = data.name;

            return $"{cityName}: {temp}°C, {description}";
        }
        catch (Exception ex)
        {
            // Если вообще нет интернета или упал сервер
            Console.WriteLine($"Ob-havoda xatolik: {ex.Message}");
            return "☁️ Ob-havo haqida malumot topilmadi keyinroq urunib koring.";
        }
    }

    static async Task<string> GetCurrency()
    {
        using var client = new HttpClient();
        var res = await client.GetStringAsync("https://www.cbr-xml-daily.ru/daily_json.js");
        dynamic data = JsonConvert.DeserializeObject(res);
        return data.Valute.USD.Value.ToString();
    }

    // --- РАБОТА С JSON ---

    static UserData GetUser(long id)
    {
        var users = System.IO.File.Exists(dbPath) ? JsonConvert.DeserializeObject<List<UserData>>(System.IO.File.ReadAllText(dbPath)) : new List<UserData>();
        return users.FirstOrDefault(u => u.Id == id) ?? new UserData { Id = id };
    }

    static void SaveUser(UserData user)
    {
        var users = System.IO.File.Exists(dbPath) ? JsonConvert.DeserializeObject<List<UserData>>(System.IO.File.ReadAllText(dbPath)) : new List<UserData>();
        users.RemoveAll(u => u.Id == user.Id);
        users.Add(user);
        System.IO.File.WriteAllText(dbPath, JsonConvert.SerializeObject(users, Formatting.Indented));
    }

    static async Task SaveHistory(long id, string action)
    {
        var user = GetUser(id);
        user.History.Add($"{DateTime.Now:HH:mm} - {action}");
        SaveUser(user);
    }

    static Task HandleError(ITelegramBotClient b, Exception e, CancellationToken c) => Task.CompletedTask;
    
}
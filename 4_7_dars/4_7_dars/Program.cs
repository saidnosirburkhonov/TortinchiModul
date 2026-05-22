using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    // Note: Never share your real token publicly! 
    static string token = "8787940439:AAGlFcACukUfB-ZXUz9zreOZsGJQzD5Yn7E";
    static TelegramBotClient bot = new TelegramBotClient(token);

    class RamadanDay
    {
        public string Date { get; set; } = "";
        public string Suhur { get; set; } = "";
        public string Iftar { get; set; } = "";
        public string Dua { get; set; } = "";
    }

    static List<RamadanDay> days = new List<RamadanDay>();

    static async Task Main()
    {
        FillData();

        using var cts = new CancellationTokenSource();

        // Fixed the parameters for StartReceiving
        bot.StartReceiving(
            updateHandler: HandleUpdate,
            pollingErrorHandler: HandleError, // This now matches the signature below
            receiverOptions: new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: cts.Token
        );

        var me = await bot.GetMe();
        Console.WriteLine($"Bot @{me.Username} is running... Press Enter to stop.");
        Console.ReadLine();
        cts.Cancel();
    }

    static void FillData()
    {
        for (int i = 1; i <= 30; i++)
        {
            days.Add(new RamadanDay
            {
                Date = $"{i}-день",
                Suhur = "04:30",
                Iftar = "19:00",
                Dua = "Аллахумма инни ляка сумту..."
            });
        }
    }

    static async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message?.Text == "/start")
        {
            await botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "📅 Выбери день Рамадана:",
                replyMarkup: GetKeyboard(),
                cancellationToken: ct
            );
            return;
        }

        if (update.CallbackQuery != null)
        {
            var query = update.CallbackQuery;
            await botClient.AnswerCallbackQuery(query.Id, cancellationToken: ct);

            if (int.TryParse(query.Data, out int index) && index >= 0 && index < days.Count)
            {
                var day = days[index];
                string text = $"📅 *{day.Date}*\n\n" +
                              $"🌙 *Сухур:* {day.Suhur}\n" +
                              $"🌇 *Ифтар:* {day.Iftar}\n\n" +
                              $"🤲 *Дуа:*\n{day.Dua}";

                await botClient.EditMessageText(
                    chatId: query.Message!.Chat.Id,
                    messageId: query.Message.Id,
                    text: text,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    replyMarkup: GetKeyboard(),
                    cancellationToken: ct
                );
            }
        }
    }

    static InlineKeyboardMarkup GetKeyboard()
    {
        var rows = new List<List<InlineKeyboardButton>>();
        for (int i = 0; i < days.Count; i += 5)
        {
            var row = new List<InlineKeyboardButton>();
            for (int j = i; j < i + 5 && j < days.Count; j++)
            {
                row.Add(InlineKeyboardButton.WithCallbackData(days[j].Date, j.ToString()));
            }
            rows.Add(row);
        }
        return new InlineKeyboardMarkup(rows);
    }

    // FIXED: Added 'PollingErrorSource source' to the parameters
    static Task HandleError(ITelegramBotClient botClient, Exception exception, PollingErrorSource source, CancellationToken ct)
    {
        Console.WriteLine($"Error from {source}: {exception.Message}");
        return Task.CompletedTask;
    }
}
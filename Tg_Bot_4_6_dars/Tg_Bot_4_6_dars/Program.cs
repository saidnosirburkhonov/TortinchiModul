using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;

namespace Tg_Bot_4_6_dars;

class Program
{
    var botClient = new TelegramBotClient("YOUR_BOT_TOKEN_HERE");

        using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token
        );

        var me = await botClient.GetMe();
Console.WriteLine($"Bot ishga tushdi: @{me.Username}");

        Console.ReadLine();
        cts.Cancel();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    if (update.Message is not { } message || message.Text is not { } text)
        return;

    var chatId = message.Chat.Id;
    var msg = text.ToLower();

    string response = msg switch
    {
        "/start" => "Salom! 😊 Men o‘zbekcha gaplashadigan botman. Qalaysan?",
        "xayr" or "hayr" or "bye" => "Xayr! 👋 Yana gaplashamiz!",
        _ when msg.Contains("qalaysan") => "Yaxshiman 😊 Sen-chi?",
        _ when msg.Contains("isming nima") => "Mening ismim UzbekBot 😄",
        _ when msg.Contains("rahmat") => "Arzimaydi! 😊",
        _ when msg.Contains("nima gap") => "Hammasi joyida 😎 Sen nima qilyapsan?",
        _ => "Qiziq 😊 Yana yoz!"
    };

    await bot.SendTextMessageAsync(chatId, response, cancellationToken: ct);
}

static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
{
    Console.WriteLine(exception.Message);
    return Task.CompletedTask;
}
}
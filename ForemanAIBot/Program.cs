using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// Загрузка конфигурации
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var botToken = configuration["BotConfiguration:BotToken"];
var botClient = new TelegramBotClient(botToken);

// Обработчик обновлений
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        if (update.Message is not { } message)
            return;

        if (message.Text == "/start")
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Частые темы", "Реклама/сотрудничество" },
                new KeyboardButton[] { "Нашёл ошибку", "Поддержка" }
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Выберите опцию:",
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при обработке обновления: {ex.Message}");
    }
}

// Обработчик ошибок
Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Ошибка Telegram API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}

// Настройка опций для бота
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // Получать все типы обновлений
};

// Создание обработчика обновлений
var updateHandler = new DefaultUpdateHandler(HandleUpdateAsync, HandlePollingErrorAsync);

// Запуск бота
using var cts = new CancellationTokenSource();

botClient.StartReceiving(
    updateHandler: updateHandler,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

Console.WriteLine("Бот запущен. Нажмите Enter для остановки...");
Console.ReadLine();

cts.Cancel();
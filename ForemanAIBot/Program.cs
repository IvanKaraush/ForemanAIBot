using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// Настройка сервисов
var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.AddConsole();
    })
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// Загрузка конфигурации
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var botToken = configuration["BotConfiguration:BotToken"];
var botClient = new TelegramBotClient(botToken);

// Обработчик обновлений
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        if (update.Message is not { } message)
        {
            return;   
        }

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
        logger.LogError(ex, "Ошибка при обработке обновления");
    }
}

// Обработчик ошибок
Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Ошибка Telegram API:{Environment.NewLine}{apiRequestException.ErrorCode}{Environment.NewLine}{apiRequestException.Message}",
        _ => exception.Message
    };

    logger.LogError(exception, "Ошибка при работе с Telegram API: {ErrorMessage}", errorMessage);
    return Task.CompletedTask;
}

// Настройка опций для бота
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() 
};

// Создание обработчика обновлений
var updateHandler = new DefaultUpdateHandler(HandleUpdateAsync, HandlePollingErrorAsync);

// Запуск бота
var cts = new CancellationTokenSource();
try
{
    botClient.StartReceiving(
        updateHandler: updateHandler,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token
    );

    logger.LogInformation("Бот запущен. Нажмите Enter для остановки...");
    Console.ReadLine();
}
finally
{
    cts.Cancel();
    cts.Dispose();
}
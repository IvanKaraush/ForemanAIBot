using ForemanAIBot.DTOs;
using ForemanAIBot.Interfaces;
using ForemanAIBot.Options;
using ForemanAIBot.Primitives;
using ForemanAIBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// Загрузка конфигурации
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(AppConstants.ConfigFileName, optional: true, reloadOnChange: true)
    .Build();

// Настройка сервисов
var serviceProvider = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole())
    .AddSingleton<IConfiguration>(configuration)
    .Configure<AIConfiguration>(configuration.GetSection(AppConstants.AIConfigSection))
    .Configure<Dictionary<string, string>>(configuration.GetSection(AppConstants.PromptsSection))
    .AddScoped<IAIService, DeepSeekService>()
    .AddHttpClient()
    .AddScoped<ApiClient>()
    .BuildServiceProvider();


var botToken = configuration["BotConfiguration:BotToken"];
var botClient = new TelegramBotClient(botToken);

// Получаем нужный класс для работы с ИИ.
var aiService = serviceProvider.GetRequiredService<IAIService>();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// Словарь для хранения контекста пользователей (специализация)
var userContexts = new Dictionary<long, Specialization>();

// Обработчик обновлений
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        if (update.CallbackQuery is { } callbackQuery)
        {
            await HandleCallbackQueryAsync(botClient, callbackQuery, cancellationToken);
            return;
        }

        if (update.Message is not { } message)
        {
            return;
        }

        var chatId = message.Chat.Id;

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
                chatId: chatId,
                text: "Выберите опцию:",
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken);
        }
        else if (message.Text == "Частые темы")
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Маляр", "painter"),
                    InlineKeyboardButton.WithCallbackData("Гипсокартонщик", "drywaller")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Электрик", "electrician")
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите специализацию:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        else if (userContexts.ContainsKey(chatId))
        {
            var specialization = userContexts[chatId];
            var aiResponse = await aiService.AskAIAsync(
                new AIRequest(specialization, message.Text));

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: aiResponse.Response,
                cancellationToken: cancellationToken);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при обработке обновления");
    }
}

// Обработчик CallbackQuery
async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery,
    CancellationToken cancellationToken)
{
    try
    {
        var chatId = callbackQuery.Message.Chat.Id;
        var data = callbackQuery.Data;

        // Преобразуем строку в enum
        var specialization = data switch
        {
            "painter" => Specialization.Painter,
            "drywaller" => Specialization.Drywaller,
            "electrician" => Specialization.Electrician,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Сохраняем ключ роли в контексте пользователя
        userContexts[chatId] = specialization;

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Теперь вы можете задавать вопросы.",
            cancellationToken: cancellationToken);

        await botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при обработке CallbackQuery");
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
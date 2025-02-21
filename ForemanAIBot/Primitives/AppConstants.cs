namespace ForemanAIBot.Primitives;

public static class AppConstants
{
   /// <summary>
   /// Имя файла конфигурации приложения.
   /// </summary>
   public const string ConfigFileName = "appsettings.json";

   /// <summary>
   /// Название секции конфигурации, содержащей настройки ИИ.
   /// </summary>
   public const string AIConfigSection = "AIConfiguration";

   /// <summary>
   /// Название секции конфигурации, содержащей промпты для различных ролей.
   /// </summary>
   public const string PromptsSection = "Prompts";

   /// <summary>
   /// MIME-тип для передачи JSON-данных в HTTP-запросах.
   /// </summary>
   public const string JsonMediaType = "application/json";
}
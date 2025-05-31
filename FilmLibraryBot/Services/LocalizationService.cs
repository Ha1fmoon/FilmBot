using Domain.Exceptions;
using Newtonsoft.Json.Linq;

namespace FilmLibraryBot.Services;

public class LocalizationService
{
    private readonly IErrorLogger _errorLogger;
    private readonly Dictionary<string, string> _strings;

    public LocalizationService(IErrorLogger errorLogger, string language = "en")
    {
        _errorLogger = errorLogger;
        _strings = LoadStrings(language);
    }

    private Dictionary<string, string> LoadStrings(string language)
    {
        var localizationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localizations");
        var filePath = Path.Combine(localizationFolder, $"{language}.json");

        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(localizationFolder, "en.json");

            if (!File.Exists(filePath)) throw new FileNotFoundException($"Localization file not found: {filePath}");
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var jsonObject = JObject.Parse(json);
            return ConvertJsonToFlatDictionary(jsonObject);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading localization from {filePath}: {ex.Message}", ex);
        }
    }

    private Dictionary<string, string> ConvertJsonToFlatDictionary(JObject jsonObject)
    {
        var result = new Dictionary<string, string>();

        foreach (var category in jsonObject.Properties())
        {
            var categoryName = category.Name;
            var categoryValue = category.Value;

            if (categoryValue is JObject categoryObject)
                foreach (var item in categoryObject.Properties())
                {
                    var key = $"{categoryName}.{item.Name}";
                    var value = item.Value.ToString();
                    result[key] = value;
                }
        }

        return result;
    }

    public string Get(string key, params object?[] args)
    {
        if (!_strings.TryGetValue(key, out var message))
        {
            _errorLogger.LogWarningAsync($"Localization key not found: '{key}'", "LocalizationService.Get");

            return $"[{key}]";
        }

        try
        {
            return args.Length > 0 ? string.Format(message, args) : message;
        }
        catch (FormatException ex)
        {
            _errorLogger.LogErrorAsync(ex, $"LocalizationService.Get - Format error for key: '{key}'");

            return message;
        }
    }
}
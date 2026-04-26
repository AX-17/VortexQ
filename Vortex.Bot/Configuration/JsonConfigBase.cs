using System.Text.Json;
using System.Text.Json.Serialization;
using Vortex.Bot.Events;

namespace Vortex.Bot.Configuration;

public abstract class JsonConfigBase<T> where T : JsonConfigBase<T>, new()
{
    private static T? _instance;
    private static readonly object _lock = new();

    [JsonIgnore]
    public virtual string FileName => typeof(T).Name;

    [JsonIgnore]
    public virtual string Directory => Path.Combine(VortexContext.Path, "Configs");

    [JsonIgnore]
    private string FullPath => Path.Combine(Directory, $"{FileName}.json");

    public virtual void SetDefault()
    {
    }

    public virtual void OnReloaded(ReloadEventArgs args)
    {
    }

    public void Save(string? path = null)
    {
        string filePath = path ?? FullPath;
        string? dir = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        string json = JsonSerializer.Serialize((T)this, options);
        File.WriteAllText(filePath, json);
    }

    private static T LoadFromFile()
    {
        T temp = new T();
        string fullPath = temp.FullPath;

        if (!File.Exists(fullPath))
        {
            temp.SetDefault();
            temp.Save();
            return temp;
        }

        string json = File.ReadAllText(fullPath);
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var config = JsonSerializer.Deserialize<T>(json, options);
        if (config == null)
        {
            temp.SetDefault();
            temp.Save();
            return temp;
        }

        return config;
    }

    private async Task OnReloadAsync(ReloadEventArgs args)
    {
        lock (_lock)
        {
            _instance = LoadFromFile();
        }
        _instance.OnReloaded(args);
    }

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= LoadFromFile();
                }
            }
            return _instance;
        }
    }

    public static string Load()
    {
        ReloadEvents.OnReload += Instance.OnReloadAsync;
        return Instance.FileName;
    }

    public static string Unload()
    {
        ReloadEvents.OnReload -= Instance.OnReloadAsync;
        return Instance.FileName;
    }

    public static void SaveInstance()
    {
        _instance?.Save();
    }

    public static void ReloadInstance()
    {
        lock (_lock)
        {
            _instance = LoadFromFile();
        }
    }
}

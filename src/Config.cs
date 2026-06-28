using System.Text.Json;
using System.Text.Json.Serialization;

namespace SecretKeeper;

internal class Config
{
	private static readonly int CurrentVersion = 1;
	public int Version { get; set; } = 1;

	public string Theme { get; set; } = "System";
	public List<CfgVault> Vaults { get; set; } = [];

	[JsonExtensionData]  // to keep unknown json fields in file (if older version was used again)
	public Dictionary<string, JsonElement>? ExtensionData { get; set; }

	private Config() { }
	public static Config V => field ??= Load();

	private static readonly string FolderPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"MixelTe",
		"SecretKeeper"
	);

	private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");

	public void Save()
	{
		Directory.CreateDirectory(FolderPath);
		Version = CurrentVersion;
		string json = JsonSerializer.Serialize(this, ConfigSerializationContext.Default.Config);
		File.WriteAllText(FilePath, json);
	}

	private static Config Load()
	{
		if (!File.Exists(FilePath)) return new Config();

		try
		{
			string json = File.ReadAllText(FilePath);
			Config config = JsonSerializer.Deserialize(json, ConfigSerializationContext.Default.Config) ?? new Config();

			if (config.Version < CurrentVersion)
			{
				config = Migrate(config);
				config.Save();
			}

			return config;
		}
		catch (JsonException)
		{
			return new Config();
		}
	}

	private static Config Migrate(Config oldConfig)
	{
		// if (oldConfig.Version == 1) { /* Transform V1 data to V2 */ oldConfig.Version = 2; }
		// if (oldConfig.Version == 2) { /* Transform V2 data to V3 */ oldConfig.Version = 3; }
		return oldConfig;
	}
}

[JsonSerializable(typeof(Config))]
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ConfigSerializationContext : JsonSerializerContext
{
}

public class CfgVault
{
	public string Path { get; set; } = string.Empty;
	public string Alias { get; set; } = string.Empty;

	public CfgVault() { }

	public CfgVault(string path, string alias)
	{
		Path = path;
		Alias = alias;
	}
}

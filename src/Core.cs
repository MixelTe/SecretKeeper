namespace SecretKeeper;

static internal class Core
{
	public static Func<string> RequestMasterPassword = () =>
		throw new InvalidOperationException("Master password provider is not configured.");

	public static string? Get(string path, string? key, string? section, string? action)
		=> Get(VaultFile.FromPath(path), key, section, action);
	public static string? Get(VaultFile file, string? key, string? section, string? action)
	{
		if (action != null && action != "no" && action != "otp")
			throw new ArgumentException("Invalid action value.", nameof(action));
		key = Utils.NormalizeName(key);
		section = Utils.NormalizeName(section);
		if (key == null || key == "password" || key == "пароль") key = "pwd";
		if (action == null && key == "otp") action = "otp";
		if (section == null) section = "";

		var data = file.Read(RequestMasterPassword);
		var curSection = "";
		foreach (var line in data.Split('\n'))
		{
			if (line.StartsWith(' ') || line.StartsWith('\t')) continue;
			if (line.StartsWith("---"))
			{
				curSection = Utils.NormalizeName(line);
				continue;
			}
			if (curSection != section) continue;
			var colonI = line.IndexOf(':');
			if (colonI < 0) continue;
			var vKey = Utils.NormalizeName(line[..colonI]);
			if (vKey != key && !(key == "pwd" && (vKey == "password" || vKey == "пароль"))) continue;
			var value = line[(colonI + 1)..].Trim();
			if (action == "otp")
				return Utils.ComputeTotp(value);
			return value;
		}
		return null;
	}

	//public static void VaultAdd(string path, string? alias, bool primary)
	//{

	//}

	//public static void VaultRemove(string path)
	//{

	//}

	//public static void VaultList()
	//{

	//}

	//public static void Add(string name, string value, string? key, string? section, bool overwrite)
	//{

	//}

	//public static void Remove(string name, string? key, string? section)
	//{

	//}

	//public static void List(bool all)
	//{

	//}
}

public class VisibleException(string message) : Exception(message) { }

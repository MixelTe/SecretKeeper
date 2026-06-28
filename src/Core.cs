namespace SecretKeeper;

static internal class Core
{
	public static Func<string> RequestMasterPassword = () =>
		throw new InvalidOperationException("Master password provider is not configured.");

	public static void Get(string name, string? key, string? section, string? action)
	{

	}
	public static void Get(string path, string? file, string? key, string? section, string? action)
	{
		var data = Utils.ReadFile(path);
	}

	public static void VaultAdd(string path, string? alias, bool primary)
	{

	}

	public static void VaultRemove(string path)
	{

	}

	public static void VaultList()
	{

	}

	public static void Add(string name, string value, string? key, string? section, bool overwrite)
	{

	}

	public static void Remove(string name, string? key, string? section)
	{

	}

	public static void List(bool all)
	{

	}
}

public class VisibleException(string message) : Exception(message) { }
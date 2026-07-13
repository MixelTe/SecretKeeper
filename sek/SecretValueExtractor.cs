namespace sek;

internal static class SecretValueExtractor
{
	public static string Extract(string path, string? key, string? section)
	{
		key = Utils.NormalizeName(key);
		section = Utils.NormalizeName(section);
		if (key == null || key == "password" || key == "пароль") key = "pwd";
		if (section == null) section = "";

		var file = SecretValueFile.FromPath(path);
		using var reader = file.OpenText(RequestMasterPassword);

		var curSection = "";
		string? line;
		while ((line = reader.ReadLine()) != null)
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
			if (key == "totp")
				return Utils.ComputeTotp(value);
			return value;
		}
		throw new Exception($"Key \"{key}\" not found in section \"{section}\" of file at {file.VPath}");
	}

	private static string RequestMasterPassword()
	{
		if (Utils.IsInteractive) Console.Write("Enter master password: ");
		return Console.ReadLine() ?? "";
	}

}

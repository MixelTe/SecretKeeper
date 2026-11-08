using System.Diagnostics.CodeAnalysis;

namespace sek;

public static class ExtString
{
	extension([NotNullWhen(false)] string? s)
	{
		public bool IsEmpty() => string.IsNullOrEmpty(s);
	}
	extension([NotNullWhen(true)] string? s)
	{
		public bool IsNotEmpty() => !string.IsNullOrEmpty(s);
	}

	extension(string? s)
	{
		public bool EqualsCI(string? s2) => string.Equals(s, s2, StringComparison.InvariantCultureIgnoreCase);
		public string PathCombine(string? path2)
		{
			var path2Safe = path2 ?? "";
			return s.IsEmpty() ? path2Safe : Path.Combine(s, path2Safe);
		}
		public string PathGetFullPath() => s.IsEmpty() ? Directory.GetCurrentDirectory() : Path.GetFullPath(s);
		[return: NotNullIfNotNull(nameof(s))]
		public string? PathGetFileNameWithoutExtension() => Path.GetFileNameWithoutExtension(s);
	}
}
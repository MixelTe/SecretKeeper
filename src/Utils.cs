using ICSharpCode.SharpZipLib.Zip;
using OtpNet;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SecretKeeper;

internal static partial class Utils
{
	[return: NotNullIfNotNull(nameof(name))]
	public static string? NormalizeName(string? name)
	{
		if (name == null) return null;
		var lower = name.ToLowerInvariant();
		var noContSpaces = ReSpaces().Replace(lower, " ");
		var repSpaceWithDash = noContSpaces.Replace(' ', '-');
		var noContDashes = ReDashes().Replace(repSpaceWithDash, "-");
		return noContDashes.Trim('-');
	}

	[GeneratedRegex(@"\s+")]
	private static partial Regex ReSpaces();

	[GeneratedRegex(@"-+")]
	private static partial Regex ReDashes();

	public static string ReadFile(string path)
	{
		return Try(() =>
		{
			if (!File.Exists(path))
				throw new VisibleException($"Error: The file at '{path}' could not be found.");

			return File.ReadAllText(path);
		});
	}

	public static List<string> GetZipFilesList(string path)
	{
		return Try(() =>
		{
			var fileNames = new List<string>();

			using var zipFile = new ZipFile(path);
			foreach (ZipEntry entry in zipFile)
			{
				if (entry.IsFile)
					fileNames.Add(entry.Name);
			}

			return fileNames;
		});
	}

	public static bool IsZipEncrypted(string zipFilePath)
	{
		return Try(() =>
		{
			using var zipFile = new ZipFile(zipFilePath);
			foreach (ZipEntry entry in zipFile)
			{
				if (entry.IsFile && entry.IsCrypted)
					return true;
			}
			return false;
		});
	}

	public static string ReadFileInZip(string path, string fileName, string? password)
	{
		return Try(() =>
		{
			using var zipFile = new ZipFile(path);
			if (password != null)
				zipFile.Password = password;

			var entry = zipFile.GetEntry(fileName)
				?? throw new FileNotFoundException($"The file '{fileName}' was not found inside ZIP {path}.");

			using var zipStream = zipFile.GetInputStream(entry);
			using var reader = new StreamReader(zipStream);
			return reader.ReadToEnd();
		});
	}

	public static string ComputeTotp(string secretKey)
	{
		return Try(() =>
		{
			var secretBytes = Base32Encoding.ToBytes(secretKey);
			var totp = new Totp(secretBytes, 30, OtpHashMode.Sha1, 6);
			return totp.ComputeTotp();
		});
	}

	private static T Try<T>(Func<T> f)
	{
		try
		{
			return f();
		}
		catch (ZipException ex)
		{
			throw new VisibleException($"Zip corruption or bad password: {ex.Message}");
		}
		catch (UnauthorizedAccessException ex)
		{
			throw new VisibleException($"Permission denied accessing paths: {ex.Message}");
		}
		catch (IOException ex)
		{
			throw new VisibleException($"Error: An I/O error occurred while reading the file. Details: {ex.Message}");
		}
		catch (Exception ex)
		{
			throw new VisibleException($"An unexpected error occurred: {ex.Message}");
		}
	}
}

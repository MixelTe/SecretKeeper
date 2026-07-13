namespace sek;

internal class SecretValueFile
{
	private readonly string _path;
	private readonly bool _isZip;
	private readonly string _fileName;

	/// <param name="path">The absolute physical path to the file or the containing ZIP archive on disk.</param>
	/// <param name="isZip">True if the target file is nested within a ZIP archive; otherwise, false.</param>
	/// <param name="fileName">The relative filename inside the ZIP archive if <paramref name="isZip"/> is true; otherwise, an empty string.</param>
	public SecretValueFile(string path, bool isZip, string fileName)
	{
		_path = path;
		_isZip = isZip;
		_fileName = fileName;
	}

	public string VPath => _isZip ? Path.Combine(_path, _fileName) : _path;

	/// <summary>
	/// Resolves a raw file path, validates its existence on disk, and creates a configured <see cref="SecretValueFile"/> instance.
	/// </summary>
	/// <param name="rawPath">The user-provided absolute or relative path to resolve.</param>
	/// <returns>A validated <see cref="SecretValueFile"/> instance ready for read or write operations.</returns>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="ArgumentException"></exception>
	public static SecretValueFile FromPath(string rawPath)
	{
		var path = ResolvePath(rawPath, out var isZip, out string? zipFileName);
		if (!File.Exists(path))
			throw new FileNotFoundException($"The required file is missing at {path}", path);
		if (!isZip) return new SecretValueFile(path, false, "");
		var files = Utils.GetZipFilesList(path);
		if (zipFileName == null)
		{
			var txtFiles = files.Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).ToList();

			if (txtFiles.Count != 1)
			{
				zipFileName = Path.GetFileNameWithoutExtension(path) + ".txt";
				if (txtFiles.Contains(zipFileName))
					return new SecretValueFile(path, true, zipFileName);

				throw new ArgumentException($"ZIP archive must contain exactly one '.txt' file or a file named '{zipFileName}'. Found {txtFiles.Count} '.txt' files.");
			}
			return new SecretValueFile(path, true, txtFiles[0]);
		}
		if (!files.Contains(zipFileName))
			throw new FileNotFoundException($"File {zipFileName} not found inside ZIP archive at {path}.", path);
		return new SecretValueFile(path, true, zipFileName);
	}

	/// <summary>
	/// Resolves implicit paths and separates the physical container path from the internal ZIP file targeting logic.
	/// </summary>
	/// <remarks>
	/// This method handles implicit extensions (e.g., mapping <c>dir/a</c> to <c>dir/a.txt</c> or <c>dir/a.zip</c>) 
	/// as well as inline ZIP path routing (e.g., treating <c>dir/z.zip/t.txt</c> or <c>dir/z/t</c> where <c>dir/z.zip</c> exists 
	/// as a file entry inside an archive).
	/// </remarks>
	/// <param name="path">The initial input path to analyze and normalize.</param>
	/// <param name="isZip">Outputs <see langword="true"/> if the target path resolves to a ZIP container; otherwise, <see langword="false"/>.</param>
	/// <param name="zipFileName">Outputs the targeted file name inside the ZIP if applicable; otherwise, <see langword="null"/>.</param>
	/// <returns>The fully qualified absolute path to the physical file or ZIP container on disk.</returns>
	private static string ResolvePath(string path, out bool isZip, out string? zipFileName)
	{
		// dir/a -> dir/a.txt, dir/a.zip/*.txt
		// dir/t.txt
		// dir/z.zip
		// dir/z/t
		// dir/z/t.txt
		// dir/z.zip/t
		// dir/z.zip/t.txt
		isZip = false;
		zipFileName = null;
		path = Path.GetFullPath(path);

		var directory = Path.GetDirectoryName(path);
		if (directory != null)
		{
			var dirIsZip = directory.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
			if (!dirIsZip && File.Exists(directory + ".zip"))
			{
				dirIsZip = true;
				directory += ".zip";
			}
			if (dirIsZip)
			{
				isZip = true;
				zipFileName = Path.GetFileName(path);
				if (!Path.HasExtension(zipFileName))
					zipFileName += ".txt";
				return directory;
			}
		}

		var extension = Path.GetExtension(path);
		if (string.IsNullOrEmpty(extension))
		{
			if (File.Exists(path + ".zip"))
			{
				isZip = true;
				return path + ".zip";
			}
			return path + ".txt";
		}
		isZip = extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);
		return path;
	}

	public StreamReader OpenText(Func<string> requestPassword)
	{
		if (_isZip)
		{
			string? password = null;
			if (Utils.IsZipEncrypted(_path))
				password = requestPassword();
			return Utils.OpenFileInZip(_path, _fileName, password);
		}
		return File.OpenText(_path);
	}
}

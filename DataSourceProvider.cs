using ICSharpCode.SharpZipLib.Zip;

namespace sek;

class DataSourceProvider(CommandArgs cmdArgs, Func<string> requestMasterPassword)
{
	public DataSource OpenDataSource()
	{
		if (cmdArgs.ArchiveName.IsEmpty())
		{
			if (cmdArgs.FileName.IsEmpty()) throw new Exception($"File name is not provided");

			var fileFullName = cmdArgs.BasePath.PathCombine(cmdArgs.FileName).PathGetFullPath();
			var stream = File.OpenRead(fileFullName);
			return new DataSource
			{
				stream = stream,
				dataSourceDescription = $"file '{fileFullName}'",
			};
		}
		else
		{
			var archiveFullName = cmdArgs.BasePath.PathCombine(cmdArgs.ArchiveName).PathGetFullPath();

			var entryName = cmdArgs.FileName.IsEmpty() ? archiveFullName.PathGetFileNameWithoutExtension() + ".txt" : cmdArgs.FileName;

			var zipFile = new ZipFile(archiveFullName);

			var entry = zipFile.GetEntry(entryName) ?? throw new Exception($"The file '{entryName}' was not found inside ZIP {archiveFullName}.");
			if (entry.IsCrypted) zipFile.Password = requestMasterPassword();

			var zipStream = zipFile.GetInputStream(entry);
			return new DataSource
			{
				stream = zipStream,
				dataSourceDescription = $"'{entryName}' inside archive '{archiveFullName}'",
				container = zipFile,
			};
		}
	}
}

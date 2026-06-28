namespace SecretKeeper;

internal static class Utils
{
	public static string ReadFile(string path)
	{
		try
		{
			if (!File.Exists(path))
				throw new VisibleException($"Error: The file at '{path}' could not be found.");

			return File.ReadAllText(path);
		}
		catch (UnauthorizedAccessException)
		{
			throw new VisibleException("Error: You do not have permission to access this file.");
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

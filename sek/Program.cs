using System.Diagnostics;
using OtpNet;
using sek;

class Program
{
	public static int Main(string[] args)
	{
		if (args.Length == 0)
		{
			showHelp();
			return 1;
		}

		try
		{
			var _args = new CmdArgs(args);

			var cmdArgs = new CommandArgs
			{
				BasePath = _args.GetOptional("folder", "d") ?? "",
				ArchiveName = _args.GetOptional("archive", "a"),
				FileName = _args.GetOptional("file", "f"),
				ParamName = _args.GetOptional("key", "k"),
				SectionName = _args.GetOptional("section", "s") ?? "",
			};

			_args.CheckUnknownArgs();

			var dataSource = new DataSourceProvider(cmdArgs, RequestMasterPassword).OpenDataSource();

			var paramName = cmdArgs.ParamName ?? "pwd";
			string[] paramNames = paramName.Equals("pwd", StringComparison.InvariantCultureIgnoreCase) ? ["pwd", "password", "пароль"] : [paramName];

			var svx = new SecretValueExtractor(dataSource, cmdArgs.SectionName, paramNames);

			Debug.WriteLine($"datasource : {dataSource.dataSourceDescription}");
			Debug.WriteLine($"section    : {cmdArgs.SectionName}");
			Debug.WriteLine($"paramNames : {string.Join(", ", paramNames)}");

			var paramValue = svx.ExtractValue() ?? throw new Exception(BuldErrMsg(dataSource.dataSourceDescription, cmdArgs, paramNames));

			string result;

			if (cmdArgs.ParamName.EqualsCI("totp"))
			{
				result = ComputeTotp(paramValue);
			}
			else
			{
				result = paramValue;
			}

			Debug.WriteLine($"result     : {result}");

			Console.WriteLine(result);

			return 0;
		}
		catch (Exception x)
		{
			Debug.WriteLine(x.Message);

			Console.Error.WriteLine(x.Message);
			return 1;
		}
	}

	private static void showHelp()
	{
		Console.Error.WriteLine("""
		SecretKeeper — Console utility for extracting passwords and generating TOTP codes.

		Usage:
		  sek <path> [/key:<key-name>] [/section:<section-name>]
		  sek /folder:<folder-path> /archive:archive-name /file:<file-name> [/key:<key-name>] [/section:<section-name>]

		Options:
		  /folder, /d   The directory containing the target file (ignored if a single path is provided).
		  /file, /f     The data file name (.txt or password-protected .zip).
		  /key, /k      The parameter to search for. Defaults to "pwd".
		                Supported values:
		                  - password, пароль, pwd -> Extracts raw password
		                  - totp                  -> Computes and returns a TOTP code
		  /section, /s  The section name to restrict the search to. If omitted,
		                only the global section (before the first "---") is searched.

		Path Resolution Shortcuts:
		  You can pass a single path argument with implicit extensions or zip routing:
		  - sek dir/secrets         -> Looks for dir/secrets.txt or dir/secrets.zip
		  - sek dir/archive/secrets -> If dir/archive.zip exists, looks for secrets.txt inside it
		  - sek dir/archive.zip     -> Opens the ZIP and reads its text file. If multiple text files
		                               exist, it automatically targets "archive.txt".

		I/O Communication:
		  - Passwords/TOTP secrets are written directly to stdout.
		  - Errors are written to stderr.
		  - Master passwords for encrypted ZIPs can be piped via stdin or entered interactively.
		""");
	}
	private static string ComputeTotp(string secretKey)
	{
		var secretBytes = Base32Encoding.ToBytes(secretKey);
		var totp = new Totp(secretBytes, 30, OtpHashMode.Sha1, 6);
		return totp.ComputeTotp();
	}
	private static string RequestMasterPassword()
	{
		if (!IsConsoleRedirected()) Console.Write("Enter master password:");

		return Console.ReadLine() ?? "";
	}
	private static bool IsConsoleRedirected()
	{
		return Console.IsInputRedirected || Console.IsOutputRedirected;
	}
	private static string BuldErrMsg(string dataSourceName, CommandArgs cmdArgs, IReadOnlyList<string> paramNames)
	{
		if (paramNames.Count == 0) return "Keys are not defined";

		var sectionName = cmdArgs.SectionName.IsEmpty() ? $"main part" : $"section \"{cmdArgs.SectionName}\"";

		string errMsgKey;

		if (paramNames.Count > 1)
		{
			var keyNames = string.Join(", ", paramNames);
			errMsgKey = $"None of keys [{keyNames}] found";
		}
		else
		{
			errMsgKey = $"Key '{paramNames[0]}' not found";
		}

		return $"{errMsgKey} in {sectionName} of {dataSourceName}";
	}
}
namespace SecretKeeper;

static internal class CLI
{
	public static int Run(string[] args)
	{
		if (args.Length == 0)
		{
			Console.Error.WriteLine("Empty args");
			return 1;
		}
		try
		{
			Core.RequestMasterPassword = RequestMasterPassword;
			Run(new CmdArgs(args));
		}
		catch (CmdArgException x)
		{
			Console.Error.WriteLine(x.Message);
			return 1;
		}
		catch (VisibleException x)
		{
			Console.Error.WriteLine(x.Message);
			return 1;
		}
		return 0;
	}

	private static void Run(CmdArgs args)
	{
		if (args.ContainsKey("help", "h"))
		{
			ShowHelp(args.Get(1));
			return;
		}
		switch (args[0])
		{
			case "help":
				ShowHelp(args.Get(1));
				break;

			case "get":
				Console.WriteLine(Core.Get(
					args[1, "name"],
					args.Get("key", "k"),
					args.Get("section", "s"),
					args.Get("action", "a")) ?? throw new VisibleException("No value found"));
				break;
			//case "vault":
			//	switch (args[1])
			//	{
			//		case "add":
			//			Core.VaultAdd(
			//				args[2, "path"],
			//				args.Get("alias", "a"),
			//				args.Get("primary", "p") != null);
			//			break;
			//		case "remove":
			//			Core.VaultRemove(args[1, "path"]);
			//			break;
			//		case "list":
			//			Core.VaultList();
			//			break;
			//		default:
			//			throw new CmdArgException($"Unknown command: vault {args[1]}");
			//	}
			//	break;
			//case "add":
			//	Core.Add(
			//		args[1, "name"],
			//		args[2, "value"],
			//		args.Get("key", "k"),
			//		args.Get("section", "s"),
			//		args.Get("overwrite", "o") != null);
			//	break;
			//case "remove":
			//	Core.Remove(
			//		args[1, "name"],
			//		args.Get("key", "k"),
			//		args.Get("section", "s"));
			//	break;
			//case "list":
			//	Core.List(args.Get("all", "a") != null);
			//	break;

			default:
				var isPath = args[0].ContainsAny('/', '\\');
				var isAbsPath = isPath && Path.IsPathFullyQualified(args[0]);
				if (!isAbsPath) throw new CmdArgException($"Unknown command: {args[0]}");
				Console.WriteLine(Core.Get(
					args[0],
					args.Get("key", "k"),
					args.Get("section", "s"),
					args.Get("action", "a")) ?? throw new VisibleException("No value found"));
				break;
		}
		args.AssertUnknownArgs();
	}

	private static string RequestMasterPassword()
	{
		Console.WriteLine("Enter master password: ");
		return Console.ReadLine() ?? "";
	}

	private static void ShowHelp(string? specificCommand = null)
	{
		if (!string.IsNullOrEmpty(specificCommand))
		{
			switch (specificCommand.ToLowerInvariant())
			{
				case "get":
					ShowGetHelp();
					return;
				case "vault":
					Console.WriteLine("Usage: sek vault <subcommand> [arguments]");
					Console.WriteLine();
					Console.WriteLine("Subcommands:");
					Console.WriteLine("  add <path>     Adds a vault path. Options: -a (alias), -p (primary flag)");
					Console.WriteLine("  remove <path>  Removes a vault path.");
					Console.WriteLine("  list           Lists all managed vaults.");
					return;
			}
		}

		Console.WriteLine("Usage: sek <command> [arguments]");
		Console.WriteLine();
		Console.WriteLine("Commands:");
		Console.WriteLine("  get <name>      Retrieves a value. Options: -k (key), -s (section), -a (action)");
		Console.WriteLine("  help <command>  Shows detailed help for a specific command.");
		Console.WriteLine();
		Console.WriteLine("Alternative Usage:");
		Console.WriteLine("  program <absolute-path>  Implicitly executes 'get' using the path as the name parameter.");
	}

	private static void ShowGetHelp()
	{
		Console.WriteLine("Usage: sek get <path> [options]");
		Console.WriteLine();
		Console.WriteLine("Arguments:");
		Console.WriteLine("  <path>              The path to the vault file. Supports advanced resolution mechanics:");
		Console.WriteLine("                      - Implicit extensions: 'dir/vault' resolves to 'dir/vault.txt' or 'dir/vault.zip'");
		Console.WriteLine("                      - Archive nesting:   'dir/archive/secret' extracts 'secret.txt' from 'dir/archive.zip'");
		Console.WriteLine("                      - Auto-discovery:    Providing just a .zip targets its single internal .txt file.");
		Console.WriteLine();
		Console.WriteLine("Options:");
		Console.WriteLine("  -k, --key <key>     The key to look for (colon-separated, e.g., 'key: value').");
		Console.WriteLine("                      [Default: 'pwd']. Also matches aliases: 'password', 'пароль'.");
		Console.WriteLine("                      * Note: Input is normalized (lowercase, spaces/dashes unified).");
		Console.WriteLine();
		Console.WriteLine("  -s, --section <sec> Filter search within a specific section initialized by '--- section_name'.");
		Console.WriteLine("                      [Default: root section (lines before any '---')]");
		Console.WriteLine("                      * Note: Input is normalized (lowercase, spaces/dashes unified).");
		Console.WriteLine();
		Console.WriteLine("  -a, --action <act>  Post-processing action to perform on the retrieved value.");
		Console.WriteLine("                      Allowed values: 'no', 'otp'");
		Console.WriteLine("                      - 'otp': Computes and returns a dynamic TOTP token instead of the raw seed.");
		Console.WriteLine("                      * Note: If --key is 'otp', the 'otp' action is triggered implicitly.");
		Console.WriteLine();
		Console.WriteLine("Name Normalization Rules (-k and -s):");
		Console.WriteLine("  Inputs are extremely forgiving. Text is converted to lowercase, spaces/dashes are collapsed");
		Console.WriteLine("  into a single dash, and leading/trailing dashes are stripped.");
		Console.WriteLine("  - A file header like '--- DB Settings ---' normalizes to 'db-settings'.");
		Console.WriteLine("  - Querying '-s \"DB Settings\"' or '-s db-settings' will both match it successfully.");
		Console.WriteLine("  - Querying '-k \"API Key\"' will successfully match a file line like 'Api  Key: value'.");
		Console.WriteLine();
		Console.WriteLine("Examples:");
		Console.WriteLine("  sek get production/db                   Retrieves 'pwd' from production/db.txt");
		Console.WriteLine("  sek get secrets/personal -k api_key     Retrieves 'api_key' from secrets/personal.txt");
		Console.WriteLine("  sek get archive/secure/banking -k otp   Generates a 2FA TOTP token from 'banking.txt' inside 'archive/secure.zip'");
		Console.WriteLine("  sek get secrets -s finance -k pin       Retrieves 'pin' under the '--- finance' section of secrets.txt");
	}
}

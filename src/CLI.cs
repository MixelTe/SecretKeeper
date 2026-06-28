namespace SecretKeeper;

static internal class CLI
{
	public static void Run(string[] args)
	{
		if (args.Length == 0)
		{
			Console.Error.WriteLine("Empty args");
			return;
		}
		try
		{
			Core.RequestMasterPassword = RequestMasterPassword;
			Run(new CmdArgs(args));
		}
		catch (CmdArgException x)
		{
			Console.Error.WriteLine(x.Message);
		}
	}

	private static void Run(CmdArgs args)
	{
		switch (args[0])
		{
			case "get":
				Core.Get(
					args[1, "name"],
					args.Get("key", "k"),
					args.Get("section", "s"),
					args.Get("action", "a"));
				break;
			case "vault":
				switch (args[1])
				{
					case "add":
						Core.VaultAdd(
							args[2, "path"],
							args.Get("alias", "a"),
							args.Get("primary", "p") != null);
						break;
					case "remove":
						Core.VaultRemove(args[1, "path"]);
						break;
					case "list":
						Core.VaultList();
						break;
					default:
						throw new CmdArgException($"Unknown command: vault {args[1]}");
				}
				break;
			case "add":
				Core.Add(
					args[1, "name"],
					args[2, "value"],
					args.Get("key", "k"),
					args.Get("section", "s"),
					args.Get("overwrite", "o") != null);
				break;
			case "remove":
				Core.Remove(
					args[1, "name"],
					args.Get("key", "k"),
					args.Get("section", "s"));
				break;
			case "list":
				Core.List(args.Get("all", "a") != null);
				break;

			default:
				var isPath = args[0].ContainsAny('/', '\\');
				var isAbsPath = isPath && Path.IsPathFullyQualified(args[0]);
				if (!isAbsPath) throw new CmdArgException($"Unknown command: {args[0]}");
				Core.Get(
					args[0],
					args.Get("file", "f"),
					args.Get("key", "k"),
					args.Get("section", "s"),
					args.Get("action", "a"));
				break;
		}
		args.AssertUnknownArgs();
	}

	private static string RequestMasterPassword()
	{
		Console.WriteLine("Enter master password: ");
		return Console.ReadLine() ?? "";
	}
}

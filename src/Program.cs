using SecretKeeper;

if (args.Length > 0)
	return CLI.Run(args);
else
	TUI.Run();

return 0;
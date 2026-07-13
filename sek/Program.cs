using sek;

if (args.Length == 0)
{
	Console.Error.WriteLine("""
		SecretKeeper — Console utility for extracting passwords and generating TOTP codes.

		Usage:
		  sek <path> [/key:<key-name>] [/section:<section-name>]
		  sek /folder:<folder-path> /file:<file-name> [/key:<key-name>] [/section:<section-name>]

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
	return 1;
}

try
{
	var _args = new CmdArgs(args);
	string path;
	if (_args.Length == 1)
	{
		path = _args[0];
	}
	else
	{
		var folder = _args["folder", "d"];
		var file = _args["file", "f"];
		path = Path.Combine(folder, file);
	}
	var key = _args.Get("key", "k");
	var section = _args.Get("section", "s");
	_args.AssertUnknownArgs();
	var value = SecretValueExtractor.Extract(path, key, section);
	Console.WriteLine(value);
}
catch (Exception x)
{
	Console.Error.WriteLine(x.Message);
	return 1;
}

return 0;
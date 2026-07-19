using System.CommandLine;
using sek;

var folderOption = new Option<string>("--folder", "-d") { Description = "Путь к папке, в которой находится файл данных" };
var fileOption = new Option<string>("--file", "-f") { Description = "Имя файла данных" };
var archiveOption = new Option<string>("--archive", "-a") { Description = "Имя файла архива, в котором может лежать файл данных" };
var paramOption = new Option<string>("--param", "-p") { Description = "Имя искомого параметра", DefaultValueFactory = _ => "pwd" };
var sectionOption = new Option<string>("--section", "-s") { Description = "Имя раздела, в котором нужно искать параметр" };

const string RootCommandDescription = """
SecretKeeper:Exractor — Console utility for extracting passwords and generating TOTP codes.
			
I/O Communication:
	- Passwords/TOTP secrets are written directly to stdout.
	- Errors are written to stderr.
	- Master passwords for encrypted ZIPs can be piped via stdin or entered interactively.
""";

var rootCommand = new RootCommand(RootCommandDescription)
{
	folderOption,
	fileOption,
	archiveOption,
	paramOption,
	sectionOption,
};

if (args.Length == 0)
{
	rootCommand.Parse("--help").Invoke();
	return 1;
}

rootCommand.SetAction(parseResult =>
{
	var cmdArgs = new CommandArgs
	{
		BasePath = parseResult.GetValue(folderOption),
		FileName = parseResult.GetValue(fileOption),
		ArchiveName = parseResult.GetValue(archiveOption),
		ParamName = parseResult.GetValue(paramOption) ?? "",
		SectionName = parseResult.GetValue(sectionOption) ?? "",
	};

	return ValueExtractor.ExtractOrGenerateValue(cmdArgs);
});

return rootCommand.Parse(args).Invoke();

using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace SecretKeeper;

static internal class TUI
{
	public static void Run()
	{
		using IApplication app = Application.Create();
		app.Init();

		var win = new Window()
		{
			Title = "sek - Help System",
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill()
		};
		var scheme = win.GetScheme();

		var categories = new List<string> { "General Usage", "Command: get" };
		var observableCategories = new ObservableCollection<string>(categories);

		var menu = new ListView
		{
			X = 0,
			Y = 1,
			Width = 20,
			Height = Dim.Fill() - 2,
			Source = new ListWrapper<string>(observableCategories)
		};

		var menuTitle = new Label()
		{
			Text = "Commands",
			X = 0,
			Y = 0,
			Width = 20,
		};

		win.Add(menuTitle, menu);

		var helpTextPane = new TextView()
		{
			X = 22,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill() - 2,
			ReadOnly = true,
			WordWrap = true
		};
		helpTextPane.SetScheme(new Terminal.Gui.Drawing.Scheme()
		{
			ReadOnly = scheme.Normal,
		});
		win.Add(helpTextPane);

		helpTextPane.Text = generalHelp;

		menu.ValueChanged += (sender, args) =>
		{
			if (menu.SelectedItem == 0)
			{
				helpTextPane.Text = generalHelp;
			}
			else if (menu.SelectedItem == 1)
			{
				helpTextPane.Text = getHelp;
			}
		};

		var footer = new StatusBar([
				new(Key.Q.WithCtrl, "Quit Application", () => app.RequestStop()) {BindKeyToApplication = true}
			]);
		win.Add(footer);

		app.Run(win);

	}

	static string generalHelp = @"Usage: sek <command> [arguments]

Commands:
  get <name>      Retrieves a value. Options: -k (key), -s (section), -a (action)
  help <command>  Shows detailed help for a specific command.

Alternative Usage:
  program <absolute-path>  Implicitly executes 'get' using the path as the name parameter.";

	static string getHelp = @"Usage: sek get <path> [options]

Arguments:
  <path>              The path to the vault file. Supports advanced resolution mechanics:
                      - Implicit extensions: 'dir/vault' resolves to 'dir/vault.txt' or 'dir/vault.zip'
                      - Archive nesting:   'dir/archive/secret' extracts 'secret.txt' from 'dir/archive.zip'
                      - Auto-discovery:    Providing just a .zip targets its single internal .txt file.

Options:
  -k, --key <key>     The key to look for (colon-separated, e.g., 'key: value').
                      [Default: 'pwd']. Also matches aliases: 'password', 'пароль'.
                      * Note: Input is normalized (lowercase, spaces/dashes unified).

  -s, --section <sec> Filter search within a specific section initialized by '--- section_name'.
                      [Default: root section (lines before any '---')]
                      * Note: Input is normalized (lowercase, spaces/dashes unified).

  -a, --action <act>  Post-processing action to perform on the retrieved value.
                      Allowed values: 'no', 'otp'
                      - 'otp': Computes and returns a dynamic TOTP token instead of the raw seed.
                      * Note: If --key is 'otp', the 'otp' action is triggered implicitly.

Name Normalization Rules (-k and -s):
  Inputs are extremely forgiving. Text is converted to lowercase, spaces/dashes are collapsed
  into a single dash, and leading/trailing dashes are stripped.
  - A file header like '--- DB Settings ---' normalizes to 'db-settings'.
  - Querying '-s \""DB Settings\""' or '-s db-settings' will both match it successfully.
  - Querying '-k \""API Key\""' will successfully match a file line like 'Api  Key: value'.

Examples:
  sek get production/db                   Retrieves 'pwd' from production/db.txt
  sek get secrets/personal -k api_key     Retrieves 'api_key' from secrets/personal.txt
  sek get archive/secure/banking -k otp   Generates a 2FA TOTP token from 'banking.txt' inside 'archive/secure.zip'
  sek get secrets -s finance -k pin       Retrieves 'pin' under the '--- finance' section of secrets.txt";
}

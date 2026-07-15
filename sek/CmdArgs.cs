namespace sek;

internal class CmdArgs
{
	private readonly List<string> _unnamedArgs = [];
	private readonly Dictionary<string, string> _namedArgs = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
	private readonly Dictionary<int, string> _unknownUnnamedArgs = [];
	private readonly Dictionary<string, string> _unknownNamedArgs = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

	public CmdArgs(string[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			var arg = args[i];

			var isNamed = false;
			if (arg.StartsWith("--"))
			{
				isNamed = true;
				arg = arg[2..];
			}
			else if (arg.StartsWith('/') || arg.StartsWith('-'))
			{
				if (arg.Count('/') <= 1)
				{
					isNamed = true;
					arg = arg[1..];
				}
			}

			if (!isNamed)
			{
				_unnamedArgs.Add(arg);
			}			
			else if (arg.IndexOfAny('=', ':') is var j1 && j1 >= 0)
			{
				AddNamedArg(arg[..j1], arg[(j1 + 1)..]);
			}
			else if (args.Length <= i + 1 || args[i + 1].StartsWith('-') || (args[i + 1].StartsWith('/') && args[i + 1].Count('/') == 1))
			{
				AddNamedArg(arg, "");
			}
			else
			{
				AddNamedArg(arg, args[++i]);
			}
		}
		_unknownUnnamedArgs = _unnamedArgs.Select((value, index) => new { value, index }).ToDictionary(x => x.index, x => x.value);
		_unknownNamedArgs = new Dictionary<string, string>(_namedArgs, StringComparer.InvariantCultureIgnoreCase);
	}

	public int Length => _unnamedArgs.Count;

	public string Get(int index, string name) => getParameterByIndex(index, name, throwOnMissing: true) ?? "";
	public string Get(params string[] keys) => getParameterByName(keys, throwOnMissing: true) ?? "";
	public string? GetOptional(int index, string name) => getParameterByIndex(index, name, throwOnMissing: false);
	public string? GetOptional(params string[] keys) => getParameterByName(keys, throwOnMissing: false);

	private string? getParameterByIndex(int index, string name, bool throwOnMissing)
	{
		if (index >= _unnamedArgs.Count)
		{
			if (throwOnMissing) throw new MissingsArgException("Missing required positional argument" + (name == "" ? "" : $": {name}"));
			return null;
		}

		_unknownUnnamedArgs.Remove(index);

		return _unnamedArgs[index];
	}
	private string? getParameterByName(string[] keys, bool throwOnMissing)
	{
		var existingKeys = keys.Where(_namedArgs.ContainsKey).ToList();
		if (existingKeys == null || existingKeys.Count == 0)
		{
			if (throwOnMissing) throw new MissingsArgException($"Missing required argument: {string.Join(" | ", keys)}");
			return null;
		}

		if (existingKeys.Count > 1) throw new CmdArgException($"{keys[0]} was passed multiple times");

		var key = existingKeys[0];
		_unknownNamedArgs.Remove(key);

		return _namedArgs[key];
	}

	public bool Contains(string item) => _unnamedArgs.Contains(item);
	public bool ContainsKey(params string[] keys) => keys.Any(_namedArgs.ContainsKey);

	public void CheckUnknownArgs()
	{
		if (_unknownUnnamedArgs.Count > 0)
			throw new CmdArgException($"Unknown extra positional arguments: {string.Join(", ", _unknownUnnamedArgs.ToList().Select(p => p.Value))}");

		if (_unknownNamedArgs.Count > 0)
			throw new CmdArgException($"Unknown named arguments: {string.Join(", ", _unknownNamedArgs.ToList().Select(p => p.Key))}");
	}

	private void AddNamedArg(string key, string value)
	{
		if (!_namedArgs.TryAdd(key, value)) throw new CmdArgException($"{key} was passed multiple times");
	}
}


public class CmdArgException(string message) : Exception(message) { }
public class MissingsArgException(string message) : CmdArgException(message) { }
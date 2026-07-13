namespace sek;

internal class CmdArgs
{
	readonly List<string> args = [];
	readonly Dictionary<string, string> kargs = [];
	readonly Dictionary<int, string> argsUnused = [];
	readonly Dictionary<string, string> kargsUnused = [];

	public CmdArgs(string[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			var arg = args[i];
			var key = false;
			if (arg.StartsWith("--"))
			{
				key = true;
				arg = arg[2..];
			}
			if (arg.StartsWith('/') || arg.StartsWith('-'))
			{
				if (arg.Count('/') <= 1)
				{
					key = true;
					arg = arg[1..];
				}
			}
			if (!key)
			{
				this.args.Add(arg);
				continue;
			}
			if (arg.IndexOf('=') is var j1 && j1 >= 0)
				AddKArg(arg[..j1], arg[(j1 + 1)..]);
			else if (arg.IndexOf(':') is var j2 && j2 >= 0)
				AddKArg(arg[..j2], arg[(j2 + 1)..]);
			else if (args.Length <= i + 1 || args[i + 1].StartsWith('-') || (args[i + 1].StartsWith('/') && args[i + 1].Count('/') == 1))
				AddKArg(arg, "");
			else
				AddKArg(arg, args[++i]);
		}
		argsUnused = this.args.Select((value, index) => new { value, index }).ToDictionary(x => x.index, x => x.value);
		kargsUnused = new Dictionary<string, string>(kargs);
	}

	private void AddKArg(string key, string value)
	{
		if (kargs.ContainsKey(key))
			throw new CmdArgException($"{key} was passed multiple times");
		kargs[key] = value;
	}


	public string this[int index, string name = ""]
	{
		get
		{
			if (index >= args.Count)
				throw new MissingsArgException("Missing required positional argument" + (name == "" ? "" : $": {name}"));
			argsUnused.Remove(index);
			return args[index];
		}
	}

	public string this[params string[] keys]
	{
		get
		{
			var existingKeys = keys.Where(kargs.ContainsKey).ToList();
			if (existingKeys == null || existingKeys.Count == 0) throw new MissingsArgException($"Missing required argument: {string.Join(" | ", keys)}");
			if (existingKeys.Count > 1) throw new CmdArgException($"{keys[0]} was passed multiple times");
			var key = existingKeys[0];
			kargsUnused.Remove(key);
			return kargs[key];
		}
	}
	public int Length => args.Count;

	public string? Get(int index)
	{
		try { return this[index]; }
		catch (MissingsArgException) { return null; }
	}
	public string? Get(params string[] keys)
	{
		try { return this[keys]; }
		catch (MissingsArgException) { return null; }
	}

	public bool Contains(string item) => args.Contains(item);
	public bool ContainsKey(params string[] keys) => keys.Any(kargs.ContainsKey);

	public void AssertUnknownArgs()
	{
		if (argsUnused.Count > 0)
			throw new CmdArgException($"Unknown extra positional arguments: {string.Join(", ", argsUnused.ToList().Select(p => p.Value))}");
		if (kargsUnused.Count > 0)
			throw new CmdArgException($"Unknown named arguments: {string.Join(", ", kargsUnused.ToList().Select(p => p.Key))}");
	}
}


public class CmdArgException(string message) : Exception(message) { }
public class MissingsArgException(string message) : CmdArgException(message) { }
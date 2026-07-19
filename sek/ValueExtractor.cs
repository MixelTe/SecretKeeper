using System.Diagnostics;
using OtpNet;

namespace sek;

static class ValueExtractor
{
	internal static int ExtractOrGenerateValue(CommandArgs cmdArgs)
	{
		try
		{
			var dataSource = new DataSourceProvider(cmdArgs, RequestMasterPassword).OpenDataSource();

			var paramNames = GetParamNames(cmdArgs);
			var svx = new SecretValueExtractor(dataSource, cmdArgs.SectionName, paramNames);

			Debug.WriteLine($"datasource : {dataSource.dataSourceDescription}");
			Debug.WriteLine($"section    : {cmdArgs.SectionName}");
			Debug.WriteLine($"paramNames : {string.Join(", ", paramNames)}");

			var paramValue = svx.ExtractValue() ?? throw new Exception(BuldErrMsg(dataSource.dataSourceDescription, cmdArgs, paramNames));
			var result = MakeResult(paramValue, cmdArgs.ParamName);

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
	private static string[] GetParamNames(CommandArgs cmdArgs)
	{
		var isParamPwd = cmdArgs.ParamName.Equals("pwd", StringComparison.InvariantCultureIgnoreCase);

		return isParamPwd ? ["pwd", "password", "пароль"] : [cmdArgs.ParamName];
	}
	private static string MakeResult(string paramValue, string paramName)
	{
		if (paramName.EqualsCI("totp"))
		{
			return ComputeTotp(paramValue);
		}
		else
		{
			return paramValue;
		}
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
	private static string BuldErrMsg(string dataSourceName, CommandArgs cmdArgs, string[] paramNames)
	{
		if (paramNames.Length == 0) return "Keys are not defined";

		var sectionName = cmdArgs.SectionName.IsEmpty() ? $"main part" : $"section \"{cmdArgs.SectionName}\"";

		string errMsgKey;

		if (paramNames.Length > 1)
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

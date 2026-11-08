using System.Diagnostics;
using OtpNet;

namespace SecretKeeper;

static class ValueExtractor
{
	internal static string ExtractOrGenerateValue(CommandArgs cmdArgs)
	{
		var dataSourceProvider = new DataSourceProvider(cmdArgs, RequestMasterPassword);
		var dataSource = dataSourceProvider.OpenDataSource();

		using (dataSource.container)
		{
			var paramNames = GetParamNames(cmdArgs);

			Debug.WriteLine($"datasource : {dataSource.dataSourceDescription}");
			Debug.WriteLine($"section    : {cmdArgs.SectionName}");
			Debug.WriteLine($"paramNames : {string.Join(", ", paramNames)}");

			var svx = new SecretValueExtractor(dataSource, cmdArgs.SectionName, paramNames);
			var paramValue = svx.ExtractValue() ?? throw new Exception(BuldErrMsg(dataSource.dataSourceDescription, cmdArgs, paramNames));
			return MakeResult(paramValue, cmdArgs.ParamName);
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

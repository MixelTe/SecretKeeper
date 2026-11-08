using System.Text.RegularExpressions;

namespace sek;

class SecretValueExtractor(DataSource dataSource, string sectionName, string[] paramNames)
{
	private readonly HashSet<string> _ParamNames = new HashSet<string>(paramNames, StringComparer.InvariantCultureIgnoreCase);

	public string? ExtractValue()
	{
		using (dataSource.container)
		using (var reader = new StreamReader(dataSource.stream, detectEncodingFromByteOrderMarks: true))
		{
			var reSection = new Regex(@"^-{3,}\s*(?'name'.*?)\s*-*\s*$");
			var reParameter = new Regex(@"^(?'name'\S.*?)\s*:\s*(?'value'.*?)\s*$");

			var curSectionName = "";

			// ищем параметры в разделах без имени, только если раздел не задан
			// фактически, это будет только часть файла до первого разделителя разделов

			while (reader.ReadLine() is string line)
			{
				if (reSection.Match(line) is var mSection && mSection.Success)
				{
					// согласно ТЗ: если раздел не указан, ищем только в начальной части файла, ДО первого раздела
					if (sectionName.IsEmpty()) break;

					curSectionName = mSection.Groups["name"].Value;
				}
				else if (curSectionName.EqualsCI(sectionName))
				{
					if (reParameter.Match(line) is var mParam && mParam.Success)
					{
						var paramName = mParam.Groups["name"].Value;
						var paramValue = mParam.Groups["value"].Value;

						if (_ParamNames.Contains(paramName)) return paramValue;
					}
				}
			}

			return null;
		}
	}
}

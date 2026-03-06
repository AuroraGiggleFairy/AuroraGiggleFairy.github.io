using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Interactions;

internal sealed class JsonLocalizationManager : ILocalizationManager
{
	private const string NameIdentifier = "name";

	private const string DescriptionIdentifier = "description";

	private const string SpaceToken = "~";

	private readonly string _basePath;

	private readonly string _fileName;

	private readonly Regex _localeParserRegex = new Regex("\\w+.(?<locale>\\w{2}(?:-\\w{2})?).json", RegexOptions.Compiled | RegexOptions.Singleline);

	public JsonLocalizationManager(string basePath, string fileName)
	{
		_basePath = basePath;
		_fileName = fileName;
	}

	public IDictionary<string, string> GetAllDescriptions(IList<string> key, LocalizationTarget destinationType)
	{
		return GetValues(key, "description");
	}

	public IDictionary<string, string> GetAllNames(IList<string> key, LocalizationTarget destinationType)
	{
		return GetValues(key, "name");
	}

	private string[] GetAllFiles()
	{
		return Directory.GetFiles(_basePath, _fileName + ".*.json", SearchOption.TopDirectoryOnly);
	}

	private IDictionary<string, string> GetValues(IList<string> key, string identifier)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] allFiles = GetAllFiles();
		foreach (string path in allFiles)
		{
			Match match = _localeParserRegex.Match(Path.GetFileName(path));
			if (!match.Success)
			{
				continue;
			}
			string value = match.Groups["locale"].Value;
			using StreamReader reader = new StreamReader(path);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			JObject jObject = JObject.Load(reader2);
			string path2 = string.Join(".", key.Select((string x) => "['" + x + "']")) + "." + identifier;
			string text = (string)jObject.SelectToken(path2);
			if (text != null)
			{
				dictionary[value] = text;
			}
		}
		return dictionary;
	}
}

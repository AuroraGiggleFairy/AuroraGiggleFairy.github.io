using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Discord.Interactions;

internal sealed class ResxLocalizationManager : ILocalizationManager
{
	private const string NameIdentifier = "name";

	private const string DescriptionIdentifier = "description";

	private readonly ResourceManager _resourceManager;

	private readonly IEnumerable<CultureInfo> _supportedLocales;

	public ResxLocalizationManager(string baseResource, Assembly assembly, params CultureInfo[] supportedLocales)
	{
		_supportedLocales = supportedLocales;
		_resourceManager = new ResourceManager(baseResource, assembly);
	}

	public IDictionary<string, string> GetAllDescriptions(IList<string> key, LocalizationTarget destinationType)
	{
		return GetValues(key, "description");
	}

	public IDictionary<string, string> GetAllNames(IList<string> key, LocalizationTarget destinationType)
	{
		return GetValues(key, "name");
	}

	private IDictionary<string, string> GetValues(IList<string> key, string identifier)
	{
		string name = string.Join(".", key) + "." + identifier;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (CultureInfo supportedLocale in _supportedLocales)
		{
			string text = _resourceManager.GetString(name, supportedLocale);
			if (text != null)
			{
				dictionary[supportedLocale.Name] = text;
			}
		}
		return dictionary;
	}
}

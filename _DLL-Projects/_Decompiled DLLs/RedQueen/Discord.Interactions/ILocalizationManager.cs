using System.Collections.Generic;

namespace Discord.Interactions;

internal interface ILocalizationManager
{
	IDictionary<string, string> GetAllNames(IList<string> key, LocalizationTarget destinationType);

	IDictionary<string, string> GetAllDescriptions(IList<string> key, LocalizationTarget destinationType);
}

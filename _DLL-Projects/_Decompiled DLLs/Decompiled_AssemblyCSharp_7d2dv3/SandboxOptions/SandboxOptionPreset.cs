using System.Collections.Generic;
using System.Text;

namespace SandboxOptions;

public class SandboxOptionPreset
{
	public string Name;

	public string LocalizedName;

	public string Description = "";

	public string DescriptionKey = "";

	public string Icon = "";

	public string Group = "";

	public List<SandboxOptions> AlwaysShow;

	public bool IsDefault;

	public bool IsUserPreset;

	public bool IsModded;

	public bool IsCustomPreset;

	public short DifficultyRating;

	public readonly Dictionary<SandboxOptions, int> PresetValues = new Dictionary<SandboxOptions, int>();

	public string DisplayName
	{
		get
		{
			if (string.IsNullOrEmpty(LocalizedName))
			{
				return Name;
			}
			return Localization.Get(LocalizedName);
		}
	}

	public string DisplayDescription
	{
		get
		{
			if (string.IsNullOrEmpty(DescriptionKey))
			{
				return Description;
			}
			return Localization.Get(DescriptionKey);
		}
	}

	public string SandboxCode => saveOptionsToCode();

	public bool ForceShowOption(SandboxOptions option)
	{
		if (AlwaysShow != null)
		{
			return AlwaysShow.Contains(option);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveOptionsToCode()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(SandboxOptionManager.currentVersion);
		foreach (SandboxOptions key in PresetValues.Keys)
		{
			stringBuilder.Append($"{SandboxOptionManager.IndexToAlpha2((int)key)}{SandboxOptionManager.IndexToAlpha(PresetValues[key])}");
		}
		return stringBuilder.ToString();
	}
}

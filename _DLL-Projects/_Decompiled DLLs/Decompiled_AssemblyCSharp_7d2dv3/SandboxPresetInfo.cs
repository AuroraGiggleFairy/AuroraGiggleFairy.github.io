using System;
using SandboxOptions;

public struct SandboxPresetInfo(SandboxOptionPreset _preset) : IEquatable<SandboxPresetInfo>
{
	public readonly string InternalName = _preset.Name;

	public readonly string FormattedName = _preset.DisplayName;

	public readonly string Description = _preset.DisplayDescription;

	public readonly string Icon = _preset.Icon;

	public readonly int Difficulty = _preset.DifficultyRating;

	public readonly bool IsUserPreset = _preset.IsUserPreset;

	public readonly bool IsCustomPreset = _preset.IsCustomPreset;

	public string SandboxCode = _preset.SandboxCode;

	public override string ToString()
	{
		return FormattedName;
	}

	public bool Equals(SandboxPresetInfo _other)
	{
		return InternalName == _other.InternalName;
	}

	public override bool Equals(object _obj)
	{
		if (_obj is SandboxPresetInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (InternalName == null)
		{
			return 0;
		}
		return InternalName.GetHashCode();
	}
}

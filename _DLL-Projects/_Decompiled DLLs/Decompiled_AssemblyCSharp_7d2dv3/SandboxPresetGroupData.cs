using System;

public struct SandboxPresetGroupData(string _internalName, string _formattedName) : IEquatable<SandboxPresetGroupData>
{
	public readonly string InternalName = _internalName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string formattedName = _formattedName;

	public override string ToString()
	{
		return formattedName;
	}

	public bool Equals(SandboxPresetGroupData _other)
	{
		return InternalName == _other.InternalName;
	}

	public override bool Equals(object _obj)
	{
		if (_obj is SandboxPresetGroupData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((InternalName != null) ? InternalName.GetHashCode() : 0) * 397;
	}
}

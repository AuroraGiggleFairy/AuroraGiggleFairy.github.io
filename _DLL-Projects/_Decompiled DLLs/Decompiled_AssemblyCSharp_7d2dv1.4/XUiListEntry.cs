using System;

public abstract class XUiListEntry : IComparable
{
	public abstract int CompareTo(object _otherEntry);

	public abstract bool GetBindingValue(ref string _value, string _bindingName);

	public abstract bool MatchesSearch(string _searchString);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiListEntry()
	{
	}
}

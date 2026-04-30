using System;

public abstract class XUiListEntry<T> : IComparable<T> where T : XUiListEntry<T>
{
	public abstract int CompareTo(T _otherEntry);

	public abstract bool GetBindingValue(ref string _value, string _bindingName);

	public abstract bool MatchesSearch(string _searchString);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiListEntry()
	{
	}
}

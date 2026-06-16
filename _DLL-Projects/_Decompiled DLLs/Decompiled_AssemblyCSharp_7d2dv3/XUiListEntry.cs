using System;

public abstract class XUiListEntry<T> : IComparable<T> where T : XUiListEntry<T>
{
	public virtual bool UiDirty { get; set; }

	public abstract int CompareTo(T _otherEntry);

	public virtual bool MatchesSearch(string _searchString)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiListEntry()
	{
	}
}

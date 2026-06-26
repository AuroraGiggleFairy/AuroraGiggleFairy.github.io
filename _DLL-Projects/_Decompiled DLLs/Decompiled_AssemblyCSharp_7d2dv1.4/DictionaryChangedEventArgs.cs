using System;

public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
{
	public TKey Key { get; }

	public TValue Value { get; }

	public string Action { get; }

	public DictionaryChangedEventArgs(TKey key, TValue value, string action)
	{
		Key = key;
		Value = value;
		Action = action;
	}
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal static class AllocFreeConcurrentStack<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>
{
	private const int MaxSize = 35;

	private static readonly Type s_typeOfT = typeof(T);

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1 })]
	private static Stack<RefAsValueType<T>> ThreadLocalStack
	{
		get
		{
			Dictionary<Type, object> dictionary = AllocFreeConcurrentStack.t_stacks;
			if (dictionary == null)
			{
				dictionary = (AllocFreeConcurrentStack.t_stacks = new Dictionary<Type, object>());
			}
			if (!dictionary.TryGetValue(s_typeOfT, out var value))
			{
				value = new Stack<RefAsValueType<T>>(35);
				dictionary.Add(s_typeOfT, value);
			}
			return (Stack<RefAsValueType<T>>)value;
		}
	}

	public static void TryAdd(T item)
	{
		Stack<RefAsValueType<T>> threadLocalStack = ThreadLocalStack;
		if (threadLocalStack.Count < 35)
		{
			threadLocalStack.Push(new RefAsValueType<T>(item));
		}
	}

	public static bool TryTake([_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out T item)
	{
		Stack<RefAsValueType<T>> threadLocalStack = ThreadLocalStack;
		if (threadLocalStack != null && threadLocalStack.Count > 0)
		{
			item = threadLocalStack.Pop().Value;
			return true;
		}
		item = default(T);
		return false;
	}
}
internal static class AllocFreeConcurrentStack
{
	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1, 1 })]
	[ThreadStatic]
	internal static Dictionary<Type, object> t_stacks;
}

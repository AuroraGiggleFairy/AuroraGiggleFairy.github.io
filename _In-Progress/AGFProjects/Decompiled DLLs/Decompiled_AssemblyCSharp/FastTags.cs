using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public readonly struct FastTags<TTagGroup> where TTagGroup : TagGroup.TagsGroupAbs, new()
{
	public static readonly FastTags<TTagGroup> none;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TTagGroup> allInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNumBitsPerField = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFieldShift = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly CaseInsensitiveStringDictionary<int> tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<int, string> bitTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int next;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int singleBit;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ulong[] bits;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<ulong> maskList;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] tagSeparator;

	public static FastTags<TTagGroup> all => allInternal;

	public bool IsEmpty
	{
		get
		{
			if (singleBit > 0)
			{
				return false;
			}
			if (bits == null)
			{
				return true;
			}
			ulong[] array = bits;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != 0L)
				{
					return false;
				}
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags(ulong[] _bits)
	{
		singleBit = 0;
		bits = _bits;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags(int _singleBit)
	{
		singleBit = _singleBit;
		bits = null;
	}

	public FastTags(FastTags<TTagGroup> _ft)
	{
		if (_ft.bits != null)
		{
			bits = new ulong[_ft.bits.Length];
			_ft.bits.CopyTo(bits, 0);
		}
		else
		{
			bits = null;
		}
		singleBit = _ft.singleBit;
	}

	public static FastTags<TTagGroup> Parse(string _str)
	{
		if (_str.IndexOf(',') < 0)
		{
			return GetTag(_str);
		}
		lock (maskList)
		{
			string[] array = _str.Split(tagSeparator);
			for (int i = 0; i < array.Length; i++)
			{
				int bit = GetBit(array[i]);
				int num = bit >> 6;
				while (maskList.Count <= num)
				{
					maskList.Add(0uL);
				}
				maskList[num] |= (ulong)(1L << bit);
			}
			ulong[] array2 = ((maskList.Count > 0) ? maskList.ToArray() : null);
			FastTags<TTagGroup> result = new FastTags<TTagGroup>(array2);
			maskList.Clear();
			return result;
		}
	}

	public static int GetBit(string _tag)
	{
		_tag = _tag.Trim();
		if (tags.TryGetValue(_tag, out var value))
		{
			return value;
		}
		value = Interlocked.Increment(ref next);
		tags.Add(_tag, value);
		bitTags.Add(value, _tag);
		int num = (value >> 6) + 1;
		if (num > allInternal.bits.Length)
		{
			ulong[] array = new ulong[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = ulong.MaxValue;
			}
			allInternal = new FastTags<TTagGroup>(array);
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetBit(int _bit, ulong[] _extended)
	{
		int num = _bit >> 6;
		_extended[num] |= (ulong)(1L << _bit);
	}

	public static FastTags<TTagGroup> GetTag(string _tag)
	{
		return new FastTags<TTagGroup>(GetBit(_tag));
	}

	public static FastTags<TTagGroup> CombineTags(FastTags<TTagGroup> _tags1, FastTags<TTagGroup> _tags2)
	{
		return _tags1 | _tags2;
	}

	public static FastTags<TTagGroup> CombineTags(FastTags<TTagGroup> _tags1, FastTags<TTagGroup> _tags2, FastTags<TTagGroup> _tags3)
	{
		ulong[] array = _tags1.bits;
		int num = ((array != null) ? array.Length : 0);
		ulong[] array2 = _tags2.bits;
		int num2 = ((array2 != null) ? array2.Length : 0);
		ulong[] array3 = _tags3.bits;
		int num3 = ((array3 != null) ? array3.Length : 0);
		int num4 = ((_tags1.singleBit > 0) ? (_tags1.singleBit >> 6) : (-1));
		int num5 = ((_tags2.singleBit > 0) ? (_tags2.singleBit >> 6) : (-1));
		int num6 = ((_tags3.singleBit > 0) ? (_tags3.singleBit >> 6) : (-1));
		int a = MathUtils.Max(num, num2, num3);
		a = MathUtils.Max(a, num4 + 1, num5 + 1, num6 + 1);
		ulong[] array4 = ((a > 0) ? new ulong[a] : null);
		if (num4 >= 0)
		{
			array4[num4] |= (ulong)(1L << _tags1.singleBit);
		}
		if (num5 >= 0)
		{
			array4[num5] |= (ulong)(1L << _tags2.singleBit);
		}
		if (num6 >= 0)
		{
			array4[num6] |= (ulong)(1L << _tags3.singleBit);
		}
		for (int i = 0; i < a; i++)
		{
			if (i < num)
			{
				array4[i] |= _tags1.bits[i];
			}
			if (i < num2)
			{
				array4[i] |= _tags2.bits[i];
			}
			if (i < num3)
			{
				array4[i] |= _tags3.bits[i];
			}
		}
		return new FastTags<TTagGroup>(array4);
	}

	public static FastTags<TTagGroup> CombineTags(FastTags<TTagGroup> _tags1, FastTags<TTagGroup> _tags2, FastTags<TTagGroup> _tags3, FastTags<TTagGroup> _tags4)
	{
		ulong[] array = _tags1.bits;
		int num = ((array != null) ? array.Length : 0);
		ulong[] array2 = _tags2.bits;
		int num2 = ((array2 != null) ? array2.Length : 0);
		ulong[] array3 = _tags3.bits;
		int num3 = ((array3 != null) ? array3.Length : 0);
		ulong[] array4 = _tags4.bits;
		int num4 = ((array4 != null) ? array4.Length : 0);
		int num5 = ((_tags1.singleBit > 0) ? (_tags1.singleBit >> 6) : (-1));
		int num6 = ((_tags2.singleBit > 0) ? (_tags2.singleBit >> 6) : (-1));
		int num7 = ((_tags3.singleBit > 0) ? (_tags3.singleBit >> 6) : (-1));
		int num8 = ((_tags4.singleBit > 0) ? (_tags4.singleBit >> 6) : (-1));
		int a = MathUtils.Max(num, num2, num3, num4);
		a = MathUtils.Max(a, num5 + 1, num6 + 1, num7 + 1);
		a = MathUtils.Max(a, num8 + 1);
		ulong[] array5 = ((a > 0) ? new ulong[a] : null);
		if (num5 >= 0)
		{
			array5[num5] |= (ulong)(1L << _tags1.singleBit);
		}
		if (num6 >= 0)
		{
			array5[num6] |= (ulong)(1L << _tags2.singleBit);
		}
		if (num7 >= 0)
		{
			array5[num7] |= (ulong)(1L << _tags3.singleBit);
		}
		if (num8 >= 0)
		{
			array5[num8] |= (ulong)(1L << _tags4.singleBit);
		}
		for (int i = 0; i < a; i++)
		{
			if (i < num)
			{
				array5[i] |= _tags1.bits[i];
			}
			if (i < num2)
			{
				array5[i] |= _tags2.bits[i];
			}
			if (i < num3)
			{
				array5[i] |= _tags3.bits[i];
			}
			if (i < num4)
			{
				array5[i] |= _tags4.bits[i];
			}
		}
		return new FastTags<TTagGroup>(array5);
	}

	public static void CombineTags(FastTags<TTagGroup> _tags1, FastTags<TTagGroup> _tags2, FastTags<TTagGroup> _tags3, FastTags<TTagGroup> _tags4, ref FastTags<TTagGroup> _outTag)
	{
		ulong[] array = _tags1.bits;
		int num = ((array != null) ? array.Length : 0);
		ulong[] array2 = _tags2.bits;
		int num2 = ((array2 != null) ? array2.Length : 0);
		ulong[] array3 = _tags3.bits;
		int num3 = ((array3 != null) ? array3.Length : 0);
		ulong[] array4 = _tags4.bits;
		int num4 = ((array4 != null) ? array4.Length : 0);
		int num5 = ((_tags1.singleBit > 0) ? (_tags1.singleBit >> 6) : (-1));
		int num6 = ((_tags2.singleBit > 0) ? (_tags2.singleBit >> 6) : (-1));
		int num7 = ((_tags3.singleBit > 0) ? (_tags3.singleBit >> 6) : (-1));
		int num8 = ((_tags4.singleBit > 0) ? (_tags4.singleBit >> 6) : (-1));
		int a = MathUtils.Max(num, num2, num3, num4);
		a = MathUtils.Max(a, num5 + 1, num6 + 1, num7 + 1);
		a = MathUtils.Max(a, num8 + 1);
		ulong[] array5;
		if (_outTag.bits != null && _outTag.bits.Length == a)
		{
			array5 = _outTag.bits;
			for (int i = 0; i < a; i++)
			{
				array5[i] = 0uL;
			}
		}
		else
		{
			array5 = ((a > 0) ? new ulong[a] : null);
		}
		if (num5 >= 0)
		{
			array5[num5] |= (ulong)(1L << _tags1.singleBit);
		}
		if (num6 >= 0)
		{
			array5[num6] |= (ulong)(1L << _tags2.singleBit);
		}
		if (num7 >= 0)
		{
			array5[num7] |= (ulong)(1L << _tags3.singleBit);
		}
		if (num8 >= 0)
		{
			array5[num8] |= (ulong)(1L << _tags4.singleBit);
		}
		for (int j = 0; j < a; j++)
		{
			if (j < num)
			{
				array5[j] |= _tags1.bits[j];
			}
			if (j < num2)
			{
				array5[j] |= _tags2.bits[j];
			}
			if (j < num3)
			{
				array5[j] |= _tags3.bits[j];
			}
			if (j < num4)
			{
				array5[j] |= _tags4.bits[j];
			}
		}
		_outTag = new FastTags<TTagGroup>(array5);
	}

	public List<string> GetTagNames()
	{
		List<string> list = new List<string>();
		if (singleBit > 0)
		{
			if (bitTags.TryGetValue(singleBit, out var value))
			{
				list.Add(value);
			}
			return list;
		}
		if (bits == null)
		{
			return list;
		}
		int num = 0;
		ulong[] array = bits;
		foreach (ulong num2 in array)
		{
			for (int j = 0; j < 64; j++)
			{
				if ((num2 & (ulong)(1L << j)) != 0L && bitTags.TryGetValue(num + j, out var value2))
				{
					list.Add(value2);
				}
			}
			num += 64;
		}
		return list;
	}

	public bool Test_AnySet(FastTags<TTagGroup> _other)
	{
		if (_other.IsEmpty)
		{
			return IsEmpty;
		}
		if (_other.singleBit > 0)
		{
			return Test_Bit(_other.singleBit);
		}
		if (singleBit > 0)
		{
			return _other.Test_Bit(singleBit);
		}
		ulong[] array = bits;
		int a = ((array != null) ? array.Length : 0);
		ulong[] array2 = _other.bits;
		int num = Mathf.Min(a, (array2 != null) ? array2.Length : 0);
		for (int i = 0; i < num; i++)
		{
			if ((bits[i] & _other.bits[i]) != 0L)
			{
				return true;
			}
		}
		return false;
	}

	public bool Test_AllSet(FastTags<TTagGroup> _other)
	{
		if (_other.singleBit > 0)
		{
			if (singleBit > 0)
			{
				return singleBit == _other.singleBit;
			}
			return Test_Bit(_other.singleBit);
		}
		if (singleBit > 0)
		{
			return _other.Test_IsOnlyBit(singleBit);
		}
		ulong[] array = bits;
		int num = ((array != null) ? array.Length : 0);
		ulong[] array2 = _other.bits;
		int num2 = ((array2 != null) ? array2.Length : 0);
		int num3 = Mathf.Min(num, num2);
		for (int i = 0; i < num3; i++)
		{
			ulong num4 = _other.bits[i];
			if ((bits[i] & num4) != num4)
			{
				return false;
			}
		}
		if (num2 > num)
		{
			for (int j = num; j < num2; j++)
			{
				if (_other.bits[j] != 0L)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool Test_Bit(int _bitNum)
	{
		if (IsEmpty)
		{
			return false;
		}
		if (singleBit > 0)
		{
			return _bitNum == singleBit;
		}
		if (bits == null)
		{
			return false;
		}
		int num = _bitNum >> 6;
		if (num >= bits.Length)
		{
			return false;
		}
		return (bits[num] & (ulong)(1L << _bitNum)) != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Test_IsOnlyBit(int _bitNum)
	{
		if (IsEmpty)
		{
			return false;
		}
		if (singleBit > 0)
		{
			return singleBit == _bitNum;
		}
		if (bits == null)
		{
			return false;
		}
		int num = _bitNum >> 6;
		ulong num2 = (ulong)(1L << _bitNum);
		if (num >= bits.Length)
		{
			return false;
		}
		if (bits[num] != num2)
		{
			return false;
		}
		for (int i = 0; i < bits.Length; i++)
		{
			if (i != num && bits[i] != 0L)
			{
				return false;
			}
		}
		return true;
	}

	public bool Equals(FastTags<TTagGroup> _other)
	{
		if (singleBit > 0 && _other.singleBit > 0)
		{
			return singleBit == _other.singleBit;
		}
		if (singleBit > 0)
		{
			return _other.Equals(this);
		}
		if (_other.singleBit > 0)
		{
			return Test_IsOnlyBit(_other.singleBit);
		}
		ulong[] array = bits;
		int num = ((array != null) ? array.Length : 0);
		ulong[] array2 = _other.bits;
		int num2 = ((array2 != null) ? array2.Length : 0);
		int num3 = Mathf.Min(num, num2);
		for (int i = 0; i < num3; i++)
		{
			if (bits[i] != _other.bits[i])
			{
				return false;
			}
		}
		if (num > num3)
		{
			for (int j = num3; j < num; j++)
			{
				if (bits[j] != 0L)
				{
					return false;
				}
			}
		}
		else if (num2 > num3)
		{
			for (int k = num3; k < num2; k++)
			{
				if (_other.bits[k] != 0L)
				{
					return false;
				}
			}
		}
		return true;
	}

	public FastTags<TTagGroup> Remove(FastTags<TTagGroup> _tagsToRemove)
	{
		if (_tagsToRemove.singleBit > 0)
		{
			if (!Test_Bit(_tagsToRemove.singleBit))
			{
				return this;
			}
			if (Test_IsOnlyBit(_tagsToRemove.singleBit))
			{
				return none;
			}
		}
		if (singleBit > 0)
		{
			if (_tagsToRemove.Test_Bit(singleBit))
			{
				return none;
			}
			return this;
		}
		ulong[] array = bits;
		int num = ((array != null) ? array.Length : 0);
		ulong[] array2 = null;
		if (num > 0)
		{
			array2 = new ulong[num];
			ulong[] array3 = _tagsToRemove.bits;
			int num2 = ((array3 != null) ? array3.Length : 0);
			int num3 = ((_tagsToRemove.singleBit > 0) ? (_tagsToRemove.singleBit >> 6) : (-1));
			ulong num4 = (ulong)((_tagsToRemove.singleBit > 0) ? (1L << _tagsToRemove.singleBit) : 0);
			for (int i = 0; i < num; i++)
			{
				array2[i] = ((i < num2) ? (bits[i] & ~_tagsToRemove.bits[i]) : bits[i]);
				if (num3 >= 0 && i == num3)
				{
					array2[i] &= ~num4;
				}
			}
		}
		return new FastTags<TTagGroup>(array2);
	}

	public static FastTags<TTagGroup> operator |(FastTags<TTagGroup> _a, FastTags<TTagGroup> _b)
	{
		if (_b.singleBit > 0)
		{
			if (_a.Test_Bit(_b.singleBit))
			{
				return _a;
			}
			int b = (_b.singleBit >> 6) + 1;
			int a;
			if (_a.singleBit > 0)
			{
				a = (_a.singleBit >> 6) + 1;
			}
			else
			{
				ulong[] array = _a.bits;
				a = ((array != null) ? array.Length : 0);
			}
			int num = Mathf.Max(a, b);
			ulong[] array2 = ((num > 0) ? new ulong[num] : null);
			if (_a.singleBit > 0)
			{
				SetBit(_a.singleBit, array2);
			}
			else if (_a.bits != null)
			{
				for (int i = 0; i < _a.bits.Length; i++)
				{
					array2[i] = _a.bits[i];
				}
			}
			SetBit(_b.singleBit, array2);
			return new FastTags<TTagGroup>(array2);
		}
		if (_a.singleBit > 0)
		{
			return _b | _a;
		}
		ulong[] array3 = _a.bits;
		int num2 = ((array3 != null) ? array3.Length : 0);
		ulong[] array4 = _b.bits;
		int num3 = ((array4 != null) ? array4.Length : 0);
		int num4 = Mathf.Min(num2, num3);
		int num5 = Mathf.Max(num2, num3);
		ulong[] array5 = ((num5 > 0) ? new ulong[num5] : null);
		for (int j = 0; j < num4; j++)
		{
			array5[j] = _a.bits[j] | _b.bits[j];
		}
		if (num2 > num4)
		{
			for (int k = num4; k < num2; k++)
			{
				array5[k] = _a.bits[k];
			}
		}
		else if (num3 > num4)
		{
			for (int l = num4; l < num3; l++)
			{
				array5[l] = _b.bits[l];
			}
		}
		return new FastTags<TTagGroup>(array5);
	}

	public static FastTags<TTagGroup> operator &(FastTags<TTagGroup> _a, FastTags<TTagGroup> _b)
	{
		if (_b.singleBit > 0)
		{
			if (_a.Test_Bit(_b.singleBit))
			{
				return _b;
			}
			return none;
		}
		if (_a.singleBit > 0)
		{
			return _b & _a;
		}
		ulong[] array = _a.bits;
		int a = ((array != null) ? array.Length : 0);
		ulong[] array2 = _b.bits;
		int b = ((array2 != null) ? array2.Length : 0);
		int num = Mathf.Min(a, b);
		ulong[] array3 = null;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			ulong num3 = _a.bits[num2] & _b.bits[num2];
			if (num3 != 0L)
			{
				if (array3 == null)
				{
					array3 = new ulong[num2 + 1];
				}
				array3[num2] = num3;
			}
		}
		return new FastTags<TTagGroup>(array3);
	}

	public override string ToString()
	{
		string text = string.Empty;
		List<string> tagNames = GetTagNames();
		for (int i = 0; i < tagNames.Count; i++)
		{
			text += tagNames[i];
			if (i < tagNames.Count - 1)
			{
				text += ", ";
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static FastTags()
	{
		none = new FastTags<TTagGroup>(null);
		allInternal = new FastTags<TTagGroup>(new ulong[6] { 18446744073709551615uL, 18446744073709551615uL, 18446744073709551615uL, 18446744073709551615uL, 18446744073709551615uL, 18446744073709551615uL });
		tags = new CaseInsensitiveStringDictionary<int>();
		bitTags = new Dictionary<int, string>();
		maskList = new List<ulong>();
		tagSeparator = new char[1] { ',' };
	}
}

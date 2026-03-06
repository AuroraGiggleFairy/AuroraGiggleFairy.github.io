using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class CollectionWrapper<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T> : ICollection<T>, IEnumerable<T>, IEnumerable, IWrappedCollection, IList, ICollection
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private readonly IList _list;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private readonly ICollection<T> _genericCollection;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private object _syncRoot;

	public virtual int Count
	{
		get
		{
			if (_genericCollection != null)
			{
				return _genericCollection.Count;
			}
			return _list.Count;
		}
	}

	public virtual bool IsReadOnly
	{
		get
		{
			if (_genericCollection != null)
			{
				return _genericCollection.IsReadOnly;
			}
			return _list.IsReadOnly;
		}
	}

	bool IList.IsFixedSize
	{
		get
		{
			if (_genericCollection != null)
			{
				return _genericCollection.IsReadOnly;
			}
			return _list.IsFixedSize;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	object IList.this[int index]
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			if (_genericCollection != null)
			{
				throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
			}
			return _list[index];
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			if (_genericCollection != null)
			{
				throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
			}
			VerifyValueType(value);
			_list[index] = (T)value;
		}
	}

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange(ref _syncRoot, new object(), null);
			}
			return _syncRoot;
		}
	}

	public object UnderlyingCollection => ((object)_genericCollection) ?? ((object)_list);

	public CollectionWrapper(IList list)
	{
		ValidationUtils.ArgumentNotNull(list, "list");
		if (list is ICollection<T> genericCollection)
		{
			_genericCollection = genericCollection;
		}
		else
		{
			_list = list;
		}
	}

	public CollectionWrapper(ICollection<T> list)
	{
		ValidationUtils.ArgumentNotNull(list, "list");
		_genericCollection = list;
	}

	public virtual void Add(T item)
	{
		if (_genericCollection != null)
		{
			_genericCollection.Add(item);
		}
		else
		{
			_list.Add(item);
		}
	}

	public virtual void Clear()
	{
		if (_genericCollection != null)
		{
			_genericCollection.Clear();
		}
		else
		{
			_list.Clear();
		}
	}

	public virtual bool Contains(T item)
	{
		if (_genericCollection != null)
		{
			return _genericCollection.Contains(item);
		}
		return _list.Contains(item);
	}

	public virtual void CopyTo(T[] array, int arrayIndex)
	{
		if (_genericCollection != null)
		{
			_genericCollection.CopyTo(array, arrayIndex);
		}
		else
		{
			_list.CopyTo(array, arrayIndex);
		}
	}

	public virtual bool Remove(T item)
	{
		if (_genericCollection != null)
		{
			return _genericCollection.Remove(item);
		}
		bool num = _list.Contains(item);
		if (num)
		{
			_list.Remove(item);
		}
		return num;
	}

	public virtual IEnumerator<T> GetEnumerator()
	{
		IEnumerable<T> genericCollection = _genericCollection;
		return (genericCollection ?? _list.Cast<T>()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		IEnumerable genericCollection = _genericCollection;
		return (genericCollection ?? _list).GetEnumerator();
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	int IList.Add(object value)
	{
		VerifyValueType(value);
		Add((T)value);
		return Count - 1;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	bool IList.Contains(object value)
	{
		if (IsCompatibleObject(value))
		{
			return Contains((T)value);
		}
		return false;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	int IList.IndexOf(object value)
	{
		if (_genericCollection != null)
		{
			throw new InvalidOperationException("Wrapped ICollection<T> does not support IndexOf.");
		}
		if (IsCompatibleObject(value))
		{
			return _list.IndexOf((T)value);
		}
		return -1;
	}

	void IList.RemoveAt(int index)
	{
		if (_genericCollection != null)
		{
			throw new InvalidOperationException("Wrapped ICollection<T> does not support RemoveAt.");
		}
		_list.RemoveAt(index);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	void IList.Insert(int index, object value)
	{
		if (_genericCollection != null)
		{
			throw new InvalidOperationException("Wrapped ICollection<T> does not support Insert.");
		}
		VerifyValueType(value);
		_list.Insert(index, (T)value);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	void IList.Remove(object value)
	{
		if (IsCompatibleObject(value))
		{
			Remove((T)value);
		}
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		CopyTo((T[])array, arrayIndex);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	private static void VerifyValueType(object value)
	{
		if (!IsCompatibleObject(value))
		{
			throw new ArgumentException("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.".FormatWith(CultureInfo.InvariantCulture, value, typeof(T)), "value");
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	private static bool IsCompatibleObject(object value)
	{
		if (!(value is T) && (value != null || (typeof(T).IsValueType() && !ReflectionUtils.IsNullableType(typeof(T)))))
		{
			return false;
		}
		return true;
	}
}

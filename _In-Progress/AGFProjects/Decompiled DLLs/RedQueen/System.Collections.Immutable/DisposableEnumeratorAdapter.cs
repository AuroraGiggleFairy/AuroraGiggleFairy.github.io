using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

internal struct DisposableEnumeratorAdapter<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T, TEnumerator> : IDisposable where TEnumerator : struct, IEnumerator<T>
{
	private readonly IEnumerator<T> _enumeratorObject;

	private TEnumerator _enumeratorStruct;

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)]
	public T Current
	{
		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
		get
		{
			if (_enumeratorObject == null)
			{
				return _enumeratorStruct.Current;
			}
			return _enumeratorObject.Current;
		}
	}

	internal DisposableEnumeratorAdapter(TEnumerator enumerator)
	{
		_enumeratorStruct = enumerator;
		_enumeratorObject = null;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
	internal DisposableEnumeratorAdapter(IEnumerator<T> enumerator)
	{
		_enumeratorStruct = default(TEnumerator);
		_enumeratorObject = enumerator;
	}

	public bool MoveNext()
	{
		if (_enumeratorObject == null)
		{
			return _enumeratorStruct.MoveNext();
		}
		return _enumeratorObject.MoveNext();
	}

	public void Dispose()
	{
		if (_enumeratorObject != null)
		{
			_enumeratorObject.Dispose();
		}
		else
		{
			_enumeratorStruct.Dispose();
		}
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1, 0 })]
	public DisposableEnumeratorAdapter<T, TEnumerator> GetEnumerator()
	{
		return this;
	}
}

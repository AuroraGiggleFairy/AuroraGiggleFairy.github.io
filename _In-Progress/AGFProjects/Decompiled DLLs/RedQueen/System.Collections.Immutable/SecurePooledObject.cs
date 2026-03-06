using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

internal class SecurePooledObject<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>
{
	private readonly T _value;

	private int _owner;

	internal int Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
	internal SecurePooledObject(T newValue)
	{
		Requires.NotNullAllowStructs(newValue, "newValue");
		_value = newValue;
	}

	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)]
	internal T Use<TCaller>(ref TCaller caller) where TCaller : struct, ISecurePooledObjectUser
	{
		if (!IsOwned(ref caller))
		{
			Requires.FailObjectDisposed(caller);
		}
		return _value;
	}

	internal bool TryUse<TCaller>(ref TCaller caller, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)][_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] out T value) where TCaller : struct, ISecurePooledObjectUser
	{
		if (IsOwned(ref caller))
		{
			value = _value;
			return true;
		}
		value = default(T);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsOwned<TCaller>(ref TCaller caller) where TCaller : struct, ISecurePooledObjectUser
	{
		return caller.PoolUserId == _owner;
	}
}

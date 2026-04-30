using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Immutable;

internal class SecureObjectPool
{
	private static int s_poolUserIdCounter;

	internal const int UnassignedId = -1;

	internal static int NewId()
	{
		int num;
		do
		{
			num = Interlocked.Increment(ref s_poolUserIdCounter);
		}
		while (num == -1);
		return num;
	}
}
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
internal class SecureObjectPool<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)] TCaller> where TCaller : ISecurePooledObjectUser
{
	public void TryAdd(TCaller caller, SecurePooledObject<T> item)
	{
		if (caller.PoolUserId == item.Owner)
		{
			item.Owner = -1;
			AllocFreeConcurrentStack<SecurePooledObject<T>>.TryAdd(item);
		}
	}

	public bool TryTake(TCaller caller, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })] out SecurePooledObject<T> item)
	{
		if (caller.PoolUserId != -1 && AllocFreeConcurrentStack<SecurePooledObject<T>>.TryTake(out item))
		{
			item.Owner = caller.PoolUserId;
			return true;
		}
		item = null;
		return false;
	}

	public SecurePooledObject<T> PrepNew(TCaller caller, T newValue)
	{
		Requires.NotNullAllowStructs(newValue, "newValue");
		SecurePooledObject<T> securePooledObject = new SecurePooledObject<T>(newValue);
		securePooledObject.Owner = caller.PoolUserId;
		return securePooledObject;
	}
}

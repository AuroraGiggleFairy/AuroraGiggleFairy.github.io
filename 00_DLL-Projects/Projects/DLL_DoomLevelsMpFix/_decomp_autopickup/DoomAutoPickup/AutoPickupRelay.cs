using UnityEngine;

namespace DoomAutoPickup;

public class AutoPickupRelay : MonoBehaviour
{
	public AutoPickup Owner;

	private void OnTriggerEnter(Collider _other)
	{
		if ((Object)(object)Owner != (Object)null)
		{
			Owner.Touched(_other);
		}
	}

	private void OnTriggerStay(Collider _other)
	{
		if ((Object)(object)Owner != (Object)null)
		{
			Owner.Touched(_other);
		}
	}
}

using UnityEngine;

public class TripWireController : MonoBehaviour
{
	public TileEntityPoweredTrigger TileEntityParent;

	public TileEntityPoweredTrigger TileEntityChild;

	public IWireNode WireNode;

	public void Init(DynamicProperties _properties)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		checkIfTriggered(other);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerStay(Collider other)
	{
		checkIfTriggered(other);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkIfTriggered(Collider other)
	{
		if (TileEntityParent != null && WireNode != null)
		{
			EntityAlive entityAlive = other.transform.GetComponent<EntityAlive>();
			if (entityAlive == null)
			{
				entityAlive = other.transform.GetComponentInParent<EntityAlive>();
			}
			if (entityAlive == null)
			{
				entityAlive = other.transform.parent.GetComponentInChildren<EntityAlive>();
			}
			if (entityAlive == null)
			{
				entityAlive = other.transform.GetComponentInChildren<EntityAlive>();
			}
			if ((!(entityAlive != null) || !(entityAlive as EntityVehicle != null) || (entityAlive as EntityVehicle).HasDriver) && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && TileEntityParent.IsPowered)
			{
				TileEntityChild.IsTriggered = true;
			}
		}
	}
}

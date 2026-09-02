using System;
using System.Collections.Generic;
using UnityEngine;

public class TraderDoorController : MonoBehaviour
{
	public TEFeatureDoor OwnerDoor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3, TraderDoorController> ControllerDictionary = new Dictionary<Vector3, TraderDoorController>();

	public static TraderDoorController AddTraderDoorController(Vector3 pos, GameObject go)
	{
		if (!ControllerDictionary.ContainsKey(pos))
		{
			TraderDoorController traderDoorController = go.AddComponent<TraderDoorController>();
			ControllerDictionary.Add(pos, traderDoorController);
			return traderDoorController;
		}
		return ControllerDictionary[pos];
	}

	public static TraderDoorController GetTraderDoorController(Vector3 pos)
	{
		if (ControllerDictionary.ContainsKey(pos))
		{
			return ControllerDictionary[pos];
		}
		return null;
	}

	public static void RemoveTraderDoorController(Vector3 pos)
	{
		if (!ControllerDictionary.ContainsKey(pos))
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World?.GetPrimaryPlayer();
		if (entityPlayerLocal != null && entityPlayerLocal.AttachedToEntity is EntityVehicle entityVehicle)
		{
			TraderDoorController traderDoorController = ControllerDictionary[pos];
			if (entityVehicle.HornActivation == traderDoorController)
			{
				entityVehicle.HornActivation = null;
			}
		}
		ControllerDictionary.Remove(pos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		EntityVehicle vehicleFromCollider = GetVehicleFromCollider(other);
		if (vehicleFromCollider != null)
		{
			vehicleFromCollider.HornActivation = this;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerExit(Collider other)
	{
		EntityVehicle vehicleFromCollider = GetVehicleFromCollider(other);
		if (vehicleFromCollider != null)
		{
			vehicleFromCollider.HornActivation = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle GetVehicleFromCollider(Collider collider)
	{
		if (collider == null)
		{
			return null;
		}
		Transform transform = collider.transform;
		if (transform != null)
		{
			EntityVehicle entityVehicle = transform.GetComponent<EntityVehicle>();
			if (entityVehicle == null)
			{
				entityVehicle = transform.GetComponentInParent<EntityVehicle>();
			}
			if (entityVehicle == null && transform.parent != null)
			{
				entityVehicle = transform.parent.GetComponentInChildren<EntityVehicle>();
			}
			if (entityVehicle == null)
			{
				entityVehicle = transform.GetComponentInChildren<EntityVehicle>();
			}
			if (entityVehicle != null && entityVehicle.IsAlive())
			{
				return entityVehicle;
			}
		}
		return null;
	}

	public void Activate()
	{
		TEFeatureLockable _typedTe = null;
		OwnerDoor.TryGetSelfOrFeature<TEFeatureLockable>(out _typedTe);
		if (_typedTe == null || !_typedTe.IsLocked())
		{
			OwnerDoor.SetOpen(!OwnerDoor.IsOpen(), _animate: true);
		}
	}
}

using UnityEngine;

public class CollisionCallForward : MonoBehaviour
{
	public Entity Entity;

	public void OnCollisionEnter(Collision collision)
	{
		if (Entity != null)
		{
			Entity.OnCollisionForward(base.transform, collision, isStay: false);
		}
	}

	public void OnCollisionStay(Collision collision)
	{
		if (Entity != null)
		{
			Entity.OnCollisionForward(base.transform, collision, isStay: true);
		}
	}

	public static Entity FindEntity(Transform _t)
	{
		Entity component = _t.GetComponent<Entity>();
		if ((bool)component)
		{
			return component;
		}
		CollisionCallForward componentInParent = _t.GetComponentInParent<CollisionCallForward>();
		if ((bool)componentInParent)
		{
			return componentInParent.Entity;
		}
		return null;
	}
}

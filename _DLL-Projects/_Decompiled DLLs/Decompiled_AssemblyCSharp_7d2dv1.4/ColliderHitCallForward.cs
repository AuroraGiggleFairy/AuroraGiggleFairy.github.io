using UnityEngine;

public class ColliderHitCallForward : MonoBehaviour
{
	public Entity Entity;

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (Entity != null)
		{
			Entity.OnControllerColliderHit(hit);
		}
	}
}

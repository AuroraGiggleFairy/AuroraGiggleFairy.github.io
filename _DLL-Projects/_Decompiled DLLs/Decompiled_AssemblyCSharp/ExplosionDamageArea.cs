using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamageArea : MonoBehaviour
{
	public List<string> BuffActions;

	public int InitiatorEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			base.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity getEntityFromCollider(Collider col)
	{
		Transform transform = col.transform;
		if (!transform.tag.StartsWith("E_") && !transform.CompareTag("Item"))
		{
			return null;
		}
		if (transform.CompareTag("Item"))
		{
			return null;
		}
		Transform transform2 = null;
		if (transform.tag.StartsWith("E_BP_"))
		{
			transform2 = GameUtils.GetHitRootTransform(transform.tag, transform);
		}
		EntityAlive entityAlive = ((transform2 != null) ? transform2.GetComponent<EntityAlive>() : null);
		if (entityAlive == null || entityAlive.IsDead())
		{
			return null;
		}
		return entityAlive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		EntityAlive entityAlive = getEntityFromCollider(other) as EntityAlive;
		if (!(entityAlive == null) && BuffActions != null)
		{
			for (int i = 0; i < BuffActions.Count; i++)
			{
				entityAlive.Buffs.AddBuff(BuffActions[i], InitiatorEntityId, entityAlive.isEntityRemote);
			}
		}
	}
}

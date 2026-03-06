using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRemoveEntities : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string targetGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetGroup = "target_group";

	public override ActionCompleteStates OnPerformAction()
	{
		if (targetGroup != "")
		{
			List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
			if (entityGroup != null)
			{
				GameEventManager current = GameEventManager.Current;
				for (int i = 0; i < entityGroup.Count; i++)
				{
					HandleRemoveData(current, entityGroup[i]);
					GameManager.Instance.StartCoroutine(removeLater(entityGroup[i]));
				}
			}
			return ActionCompleteStates.Complete;
		}
		if (base.Owner.Target != null)
		{
			HandleRemoveData(GameEventManager.Current, base.Owner.Target);
			GameManager.Instance.StartCoroutine(removeLater(base.Owner.Target));
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleRemoveData(GameEventManager gm, Entity ent)
	{
		gm.RemoveSpawnedEntry(ent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator removeLater(Entity e)
	{
		yield return new WaitForSeconds(0.25f);
		if (e is EntityVehicle entityVehicle)
		{
			entityVehicle.Kill();
		}
		if (e != null)
		{
			GameManager.Instance.World.RemoveEntity(e.entityId, EnumRemoveEntityReason.Killed);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTargetGroup, ref targetGroup);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRemoveEntities
		{
			targetGroup = targetGroup
		};
	}
}

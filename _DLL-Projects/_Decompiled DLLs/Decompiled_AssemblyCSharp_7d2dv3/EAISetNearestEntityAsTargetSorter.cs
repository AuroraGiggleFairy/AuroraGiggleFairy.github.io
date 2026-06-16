using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAISetNearestEntityAsTargetSorter : IComparer<Entity>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Entity theEntity;

	public EAISetNearestEntityAsTargetSorter(Entity _entity)
	{
		theEntity = _entity;
	}

	public int Compare(Entity _e, Entity _e2)
	{
		float distanceSq = theEntity.GetDistanceSq(_e);
		float distanceSq2 = theEntity.GetDistanceSq(_e2);
		if (distanceSq < distanceSq2)
		{
			return -1;
		}
		if (!(distanceSq <= distanceSq2))
		{
			return 1;
		}
		return 0;
	}
}

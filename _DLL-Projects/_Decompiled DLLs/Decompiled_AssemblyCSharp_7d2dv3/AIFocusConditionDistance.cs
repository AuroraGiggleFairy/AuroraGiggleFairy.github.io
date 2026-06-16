using UnityEngine;

public readonly struct AIFocusConditionDistance
{
	public readonly float ConditionalDistanceSq;

	public readonly EntityAlive EntityTransform;

	public readonly Transform DistanceTransform;

	public readonly Vector3 DistancePosition;

	public readonly bool HasDistancePosition;

	public AIFocusConditionDistance(float distance, EntityAlive entityTransform)
	{
		ConditionalDistanceSq = distance * distance;
		EntityTransform = entityTransform;
		DistanceTransform = null;
		DistancePosition = Vector3.zero;
		HasDistancePosition = false;
	}

	public AIFocusConditionDistance(float distance, Transform distanceTransform)
	{
		ConditionalDistanceSq = distance * distance;
		EntityTransform = null;
		DistanceTransform = distanceTransform;
		DistancePosition = Vector3.zero;
		HasDistancePosition = false;
	}

	public AIFocusConditionDistance(float distance, Vector3 distancePosition)
	{
		ConditionalDistanceSq = distance * distance;
		EntityTransform = null;
		DistanceTransform = null;
		DistancePosition = distancePosition;
		HasDistancePosition = true;
	}

	public bool IsFocusDisabled(Entity theEntity)
	{
		if (ConditionalDistanceSq > 0f)
		{
			Vector3 zero = Vector3.zero;
			if (HasDistancePosition)
			{
				zero = DistancePosition;
			}
			else if ((bool)EntityTransform)
			{
				zero = EntityTransform.position;
			}
			else
			{
				if (!DistanceTransform)
				{
					return false;
				}
				zero = DistanceTransform.position;
			}
			Vector3 position = theEntity.position;
			if (Vector3.SqrMagnitude(zero - position) > ConditionalDistanceSq)
			{
				return true;
			}
		}
		return false;
	}
}

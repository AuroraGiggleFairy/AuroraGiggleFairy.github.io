public struct AIFocusBody : IFocusTarget
{
	public readonly EntityAlive TargetYawEntity;

	public readonly float TargetYaw;

	public readonly bool HasTargetYaw;

	public AIFocusConditionDistance ConditionDistance;

	public AIFocusBody(float target)
	{
		TargetYaw = target;
		TargetYawEntity = null;
		HasTargetYaw = true;
		ConditionDistance = default(AIFocusConditionDistance);
	}

	public AIFocusBody(EntityAlive target)
	{
		TargetYaw = 0f;
		TargetYawEntity = target;
		HasTargetYaw = false;
		ConditionDistance = default(AIFocusConditionDistance);
	}

	public bool TryGetValue(EntityAlive theEntity, out float value)
	{
		if (HasTargetYaw)
		{
			value = TargetYaw;
			return true;
		}
		if (TargetYawEntity != null)
		{
			value = theEntity.YawForTarget(TargetYawEntity);
			return true;
		}
		value = 0f;
		return false;
	}

	public static bool GetActiveFocusForPriority(EntityAlive theEntity, FocusPriority priority, AIFocus<AIFocusBody> focus, out float focusForPriority)
	{
		return focus.FocusTargets[(int)priority].TryGetValue(theEntity, out focusForPriority);
	}

	public static bool GetActiveFocus(EntityAlive theEntity, AIFocus<AIFocusBody> focus, out float activeFocus)
	{
		_ = theEntity.position;
		for (int i = 0; i < focus.FocusTargets.Length; i++)
		{
			ref AIFocusBody reference = ref focus.FocusTargets[i];
			if (reference.TryGetValue(theEntity, out var value) && !reference.ConditionDistance.IsFocusDisabled(theEntity))
			{
				activeFocus = value;
				return true;
			}
		}
		activeFocus = 0f;
		return false;
	}
}

using UnityEngine;

public readonly struct AIFocusAim(Entity target, AIAimFocusOffset targetOffset) : IFocusTarget
{
	public readonly Entity Target = target;

	public readonly AIAimFocusOffset TargetOffset = targetOffset;

	public readonly AIFocusConditionDistance ConditionDistance = default(AIFocusConditionDistance);

	public static bool GetActiveFocus(Entity theEntity, AIFocus<AIFocusAim> focus, out Vector3 activeFocus)
	{
		_ = theEntity.position;
		for (int i = 0; i < focus.FocusTargets.Length; i++)
		{
			Entity target = focus.FocusTargets[i].Target;
			if ((bool)target && !focus.FocusTargets[i].ConditionDistance.IsFocusDisabled(theEntity))
			{
				Vector3 position = target.position;
				activeFocus = focus.FocusTargets[i].TargetOffset switch
				{
					AIAimFocusOffset.Belly => target.getBellyPosition(), 
					AIAimFocusOffset.Chest => target.getChestPosition(), 
					AIAimFocusOffset.Head => target.getHeadPosition(), 
					_ => focus.FocusTargets[i].Target.position, 
				};
				return true;
			}
		}
		activeFocus = default(Vector3);
		return false;
	}
}

public readonly struct AIFocus<T>(bool thisIsDumb) where T : struct, IFocusTarget
{
	public readonly T[] FocusTargets = new T[4];

	public void SetFocus(FocusPriority priority, T newTarget)
	{
		FocusTargets[(int)priority] = newTarget;
	}

	public void ClearFocus(FocusPriority priority)
	{
		FocusTargets[(int)priority] = new T();
	}
}

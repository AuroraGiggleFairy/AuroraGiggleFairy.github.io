using UnityEngine;

public class NGuiButtonOnClickHandler : MonoBehaviour
{
	public INGuiButtonOnClick OnClickDelegate;

	public INGuiButtonOnDoubleClick OnDoubleClickDelegate;

	public INGuiButtonOnHover OnHoverDelegate;

	public INGuiButtonOnIsHeld OnIsHeldDelegate;

	public virtual void OnClick()
	{
		if (OnClickDelegate != null)
		{
			OnClickDelegate.NGuiButtonOnClick(base.transform);
		}
	}

	public virtual void OnDoubleClick()
	{
		if (OnDoubleClickDelegate != null)
		{
			OnDoubleClickDelegate.NGuiButtonOnDoubleClick(base.transform);
		}
	}

	public void OnHover(bool _isOver)
	{
		if (OnHoverDelegate != null)
		{
			OnHoverDelegate.NGuiButtonOnHover(base.transform, _isOver);
		}
	}

	public void OnIsHeld()
	{
		if (OnIsHeldDelegate != null)
		{
			OnIsHeldDelegate.NGuiButtonOnIsHeld(base.transform);
		}
	}
}

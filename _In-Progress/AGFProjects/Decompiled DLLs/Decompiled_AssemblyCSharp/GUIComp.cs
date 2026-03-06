using UnityEngine;

public abstract class GUIComp
{
	public Rect rect;

	public abstract void OnGUI();

	public virtual void OnGUILayout()
	{
	}

	public void SetPosition(int _x, int _y)
	{
		rect.x = _x;
		rect.y = _y;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIComp()
	{
	}
}

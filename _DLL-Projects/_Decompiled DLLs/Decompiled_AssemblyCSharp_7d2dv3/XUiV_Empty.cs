using UnityEngine;

public class XUiV_Empty : XUiView
{
	public static readonly Vector3[] WorldCornersEmpty = new Vector3[4];

	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	public override Vector3[] WorldCorners => WorldCornersEmpty;

	public XUiV_Empty(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}
}

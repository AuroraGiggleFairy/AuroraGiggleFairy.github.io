using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_FullScreenCollider : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SortedSet<XUiC_FullScreenCollider> visibleInstances = new SortedSet<XUiC_FullScreenCollider>(XUiControllerComparerDepth.Instance);

	public override void Cleanup()
	{
		base.Cleanup();
		visibleInstances.Remove(this);
	}

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void visibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			visibleInstances.Add(this);
		}
		else
		{
			visibleInstances.Remove(this);
		}
	}

	public static bool IsBlocked(XUiView _view)
	{
		if (visibleInstances.Count == 0)
		{
			return false;
		}
		return visibleInstances.Max.ViewComponent.Depth > _view.Depth + 5;
	}
}

using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapWaypoint : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MapWaypointList waypointList;

	public override void Init()
	{
		base.Init();
		waypointList = (XUiC_MapWaypointList)base.Parent.GetChildById("waypointList");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWaypointSetPressed(XUiController _sender, EventArgs _e)
	{
	}
}

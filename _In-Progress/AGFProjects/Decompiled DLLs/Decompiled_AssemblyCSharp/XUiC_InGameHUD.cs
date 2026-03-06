using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameHUD : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] statBarList;

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_HUDStatBar>();
		statBarList = childrenByType;
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			_ = base.xui.playerUI.entityPlayer;
			IsDirty = true;
		}
	}
}

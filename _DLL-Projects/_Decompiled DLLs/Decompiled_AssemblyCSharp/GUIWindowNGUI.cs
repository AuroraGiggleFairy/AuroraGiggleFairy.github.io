using UnityEngine;

public class GUIWindowNGUI : GUIWindow
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumNGUIWindow nguiEnum;

	public GUIWindowNGUI(EnumNGUIWindow _nguiEnum)
		: base(_nguiEnum.ToStringCached(), default(Rect))
	{
		nguiEnum = _nguiEnum;
	}

	public GUIWindowNGUI(EnumNGUIWindow _nguiEnum, bool _bDrawBackground)
		: base(_nguiEnum.ToStringCached(), default(Rect), _bDrawBackground)
	{
		nguiEnum = _nguiEnum;
	}

	public override void OnOpen()
	{
		nguiWindowManager.Show(nguiEnum, _bEnable: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		nguiWindowManager.Show(nguiEnum, _bEnable: false);
	}
}

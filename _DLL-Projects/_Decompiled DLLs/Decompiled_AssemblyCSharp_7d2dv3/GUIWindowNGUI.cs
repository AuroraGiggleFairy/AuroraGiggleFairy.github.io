public class GUIWindowNGUI : GUIWindow
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumNGUIWindow nguiEnum;

	public GUIWindowNGUI(EnumNGUIWindow _nguiEnum)
		: base(_nguiEnum.ToStringCached())
	{
		nguiEnum = _nguiEnum;
	}

	public override void OnOpen()
	{
		playerUI.nguiWindowManager.Show(nguiEnum, _bEnable: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		playerUI.nguiWindowManager.Show(nguiEnum, _bEnable: false);
	}
}

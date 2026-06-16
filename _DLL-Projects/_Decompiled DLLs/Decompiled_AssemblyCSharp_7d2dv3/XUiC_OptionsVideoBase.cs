using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideoBase : XUiC_OptionsDialogBase
{
	[XuiXmlBinding("vsync_count_pref")]
	public EnumGamePrefs VSyncCountPref
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return PlatformApplicationManager.Application.VSyncCountPref;
		}
	}

	[XuiXmlBinding("ui_size_limited")]
	public bool UiSizeLimited
	{
		get
		{
			float num = (float)GameOptionsManager.GetUiSizeLimit();
			return GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize) > num;
		}
	}

	[XuiXmlBinding("ui_size_limit")]
	public float UiSizeLimit => (float)GameOptionsManager.GetUiSizeLimit();

	[XuiXmlBinding("fov_min")]
	public int FovMin => Constants.cMinCameraFieldOfView;

	[XuiXmlBinding("fov_max")]
	public int FovMax => Constants.cMaxCameraFieldOfView;

	[XuiXmlBinding("ui_opacity_background_min")]
	public float UiOpacityBackgroundMin => Constants.cMinGlobalBackgroundOpacity;

	[XuiXmlBinding("ui_opacity_foreground_min")]
	public float UiOpacityForegroundMin => Constants.cMinGlobalForegroundOpacity;
}

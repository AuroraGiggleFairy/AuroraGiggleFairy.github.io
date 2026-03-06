using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DemoWindow : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GameManager.Instance.Pause(_bOn: true);
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager.Instance.Pause(_bOn: false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (!(bindingName == "is_xbox"))
		{
			if (bindingName == "is_ps5")
			{
				value = (DeviceFlag.PS5.IsCurrent() || (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent()).ToString();
				return true;
			}
			return false;
		}
		value = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent().ToString();
		return true;
	}
}

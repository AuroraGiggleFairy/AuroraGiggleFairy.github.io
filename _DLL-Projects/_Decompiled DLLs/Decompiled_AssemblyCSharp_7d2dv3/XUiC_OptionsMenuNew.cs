using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsMenuNew : XUiController
{
	public static XUiC_WindowSelector ParentSelector;

	[XuiXmlBinding("video_options_simplified")]
	public bool VideoOptionsSimplified
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			switch (UIOptions.OptionsVideoWindow)
			{
			case OptionsVideoWindowMode.Simplified:
				return true;
			case OptionsVideoWindowMode.Detailed:
				return false;
			default:
				Log.Error($"Unknown video options menu {UIOptions.OptionsVideoWindow}");
				return false;
			}
		}
	}

	[XuiXmlBinding("has_block_list")]
	public bool HasBlockList
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return BlockedPlayerList.Instance != null;
		}
	}

	public override void Init()
	{
		base.Init();
		ParentSelector = GetParentByType<XUiC_WindowSelector>();
		UIOptions.OnOptionsVideoWindowChanged += OnVideoOptionsWindowChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		ParentSelector = null;
		UIOptions.OnOptionsVideoWindowChanged -= OnVideoOptionsWindowChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoOptionsWindowChanged(OptionsVideoWindowMode _mode)
	{
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GameManager.Instance.Pause(_bOn: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager.Instance.Pause(_bOn: false);
	}
}

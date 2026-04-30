using UnityEngine.Scripting;

[Preserve]
public class XUiC_InteractionPrompt : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string text;

	public string Text
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return text;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != text)
			{
				text = value;
				IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "text")
		{
			_value = text;
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static void SetText(LocalPlayerUI _playerUi, string _text)
	{
		if (!(_playerUi != null) || !(_playerUi.xui != null))
		{
			return;
		}
		XUiC_InteractionPrompt xUiC_InteractionPrompt = _playerUi.xui.FindWindowGroupByName(ID)?.GetChildByType<XUiC_InteractionPrompt>();
		if (xUiC_InteractionPrompt != null)
		{
			xUiC_InteractionPrompt.Text = _text;
			if (string.IsNullOrEmpty(_text))
			{
				_playerUi.windowManager.Close(ID);
			}
			else
			{
				_playerUi.windowManager.Open(ID, _bModal: false, _bIsNotEscClosable: false, _bCloseAllOpenWindows: false);
			}
		}
	}
}

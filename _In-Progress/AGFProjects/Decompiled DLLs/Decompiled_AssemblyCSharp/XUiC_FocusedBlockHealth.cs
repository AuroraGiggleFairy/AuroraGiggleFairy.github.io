using UnityEngine.Scripting;

[Preserve]
public class XUiC_FocusedBlockHealth : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string text = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float fill;

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
				text = value ?? "";
				IsDirty = true;
			}
		}
	}

	public float Fill
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return fill;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != fill)
			{
				fill = value;
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
		if (!(_bindingName == "text"))
		{
			if (_bindingName == "fill")
			{
				_value = fill.ToCultureInvariantString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = text;
		return true;
	}

	public static void SetData(LocalPlayerUI _playerUi, string _text, float _fill)
	{
		if (!(_playerUi != null) || !(_playerUi.xui != null))
		{
			return;
		}
		XUiC_FocusedBlockHealth xUiC_FocusedBlockHealth = _playerUi.xui.FindWindowGroupByName(ID)?.GetChildByType<XUiC_FocusedBlockHealth>();
		if (xUiC_FocusedBlockHealth != null)
		{
			xUiC_FocusedBlockHealth.Text = _text;
			xUiC_FocusedBlockHealth.Fill = _fill;
			if (_text == null)
			{
				_playerUi.windowManager.Close(ID);
			}
			else
			{
				_playerUi.windowManager.Open(ID, _bModal: false, _bIsNotEscClosable: false, _bCloseAllOpenWindows: false);
			}
		}
	}

	public static bool IsWindowOpen(LocalPlayerUI _playerUi)
	{
		return _playerUi.windowManager.IsWindowOpen(ID);
	}
}

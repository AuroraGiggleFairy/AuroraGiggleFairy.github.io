using UnityEngine.Scripting;

[Preserve]
public class XUiC_ThrowPower : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentPower;

	public float CurrentPower
	{
		get
		{
			return currentPower;
		}
		set
		{
			if (value != currentPower)
			{
				currentPower = value;
				RefreshBindings();
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)viewComponent).ForceVisible();
	}

	public override void OnClose()
	{
		base.OnClose();
		CurrentPower = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "fill")
		{
			_value = currentPower.ToCultureInvariantString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static void Status(LocalPlayerUI _playerUi, float _currentPower = -1f)
	{
		XUiC_ThrowPower windowByType = _playerUi.xui.GetWindowByType<XUiC_ThrowPower>();
		if (windowByType != null)
		{
			windowByType.CurrentPower = _currentPower;
			if (_currentPower >= 0f)
			{
				_playerUi.windowManager.Open(windowByType.windowGroup, _bModal: false, _bIsNotEscClosable: true);
			}
			else
			{
				_playerUi.windowManager.Close(windowByType.windowGroup);
			}
		}
	}
}

using System;
using InControl;

public class NGuiAction
{
	[Flags]
	public enum EnumKeyMode
	{
		None = 0,
		FireOnPress = 1,
		FireOnRelease = 2,
		FireOnRepeat = 4
	}

	public delegate void OnClickActionDelegate();

	public delegate void OnReleaseActionDelegate();

	public delegate void OnDoubleClickActionDelegate();

	public delegate void OnSelectActionDelegate(bool _bSelected);

	public delegate bool IsEnabledDelegate();

	public delegate bool IsVisibleDelegate();

	public delegate bool IsCheckedDelegate();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction hotkey;

	public static NGuiAction Separator = new NGuiAction("Sep", null);

	[PublicizedFrom(EAccessModifier.Private)]
	public string text;

	[PublicizedFrom(EAccessModifier.Private)]
	public string icon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string description;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltip;

	[PublicizedFrom(EAccessModifier.Private)]
	public OnClickActionDelegate clickActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public OnReleaseActionDelegate releaseActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public OnDoubleClickActionDelegate doubleClickActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public OnSelectActionDelegate selectActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public IsVisibleDelegate isVisibleDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public IsCheckedDelegate isCheckedDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public IsEnabledDelegate isEnabledDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bToggle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChecked;

	public EnumKeyMode KeyMode = EnumKeyMode.FireOnPress;

	public NGuiAction()
	{
	}

	public NGuiAction(string _text, PlayerAction _hotkey)
		: this(_text, null, _isToggle: false)
	{
		hotkey = _hotkey;
	}

	public NGuiAction(string _text, string _icon, bool _isToggle)
		: this(_text, _icon, null, _isToggle, null)
	{
	}

	public NGuiAction(string _text, string _icon, string _description, bool _isToggle, PlayerAction _hotkey)
	{
		text = _text;
		icon = _icon;
		description = _description;
		hotkey = _hotkey;
		bToggle = _isToggle;
		bEnabled = true;
	}

	public virtual void OnClick()
	{
		if (IsEnabled() && clickActionDelegate != null)
		{
			clickActionDelegate();
		}
		UpdateUI();
	}

	public virtual void OnRelease()
	{
		if (IsEnabled() && releaseActionDelegate != null)
		{
			releaseActionDelegate();
		}
		UpdateUI();
	}

	public virtual void OnDoubleClick()
	{
		if (IsEnabled() && doubleClickActionDelegate != null)
		{
			doubleClickActionDelegate();
		}
		UpdateUI();
	}

	public virtual void OnSelect(bool _bSelected)
	{
		if (IsEnabled())
		{
			if (IsToggle())
			{
				SetChecked(_bSelected);
			}
			if (selectActionDelegate != null)
			{
				selectActionDelegate(_bSelected);
			}
		}
		UpdateUI();
	}

	public virtual bool IsActive()
	{
		if (isVisibleDelegate != null)
		{
			return isVisibleDelegate();
		}
		return true;
	}

	public virtual string GetIcon()
	{
		return icon;
	}

	public virtual string GetText()
	{
		return text;
	}

	public void SetText(string _text)
	{
		text = _text;
		UpdateUI();
	}

	public virtual string GetTooltip()
	{
		return tooltip;
	}

	public NGuiAction SetTooltip(string _tooltip)
	{
		tooltip = (string.IsNullOrEmpty(_tooltip) ? null : Localization.Get(_tooltip));
		return this;
	}

	public virtual int GetColumnCount()
	{
		return 0;
	}

	public virtual string GetColumnIcon(int _col)
	{
		return null;
	}

	public virtual string GetColumnText(int _col)
	{
		return null;
	}

	public virtual string GetDescription()
	{
		return description;
	}

	public virtual NGuiAction SetDescription(string _desc)
	{
		description = _desc;
		return this;
	}

	public virtual PlayerAction GetHotkey()
	{
		return hotkey;
	}

	public virtual NGuiAction SetEnabled(bool _bEnabled)
	{
		bEnabled = _bEnabled;
		UpdateUI();
		return this;
	}

	public virtual bool IsEnabled()
	{
		if (isEnabledDelegate != null)
		{
			return isEnabledDelegate();
		}
		return bEnabled;
	}

	public virtual bool IsToggle()
	{
		return bToggle;
	}

	public virtual void SetChecked(bool _bChecked)
	{
		bChecked = _bChecked;
		UpdateUI();
	}

	public virtual bool IsChecked()
	{
		if (isCheckedDelegate != null)
		{
			return isCheckedDelegate();
		}
		return bChecked;
	}

	public virtual NGuiAction SetIsCheckedDelegate(IsCheckedDelegate _checkedDelegate)
	{
		isCheckedDelegate = _checkedDelegate;
		return this;
	}

	public virtual NGuiAction SetIsVisibleDelegate(IsVisibleDelegate _isVisibleDelegate)
	{
		isVisibleDelegate = _isVisibleDelegate;
		return this;
	}

	public virtual NGuiAction SetIsEnabledDelegate(IsEnabledDelegate _isEnabledDelegate)
	{
		isEnabledDelegate = _isEnabledDelegate;
		return this;
	}

	public virtual NGuiAction SetClickActionDelegate(OnClickActionDelegate _actionDelegate)
	{
		clickActionDelegate = _actionDelegate;
		return this;
	}

	public virtual NGuiAction SetReleaseActionDelegate(OnReleaseActionDelegate _actionDelegate)
	{
		releaseActionDelegate = _actionDelegate;
		return this;
	}

	public virtual NGuiAction SetDoubleClickActionDelegate(OnDoubleClickActionDelegate _actionDelegate)
	{
		doubleClickActionDelegate = _actionDelegate;
		return this;
	}

	public virtual NGuiAction SetSelectActionDelegate(OnSelectActionDelegate _selectActionDelegate)
	{
		selectActionDelegate = _selectActionDelegate;
		return this;
	}

	public void UpdateUI()
	{
	}

	public override string ToString()
	{
		if (text == null)
		{
			return string.Empty;
		}
		return text;
	}
}

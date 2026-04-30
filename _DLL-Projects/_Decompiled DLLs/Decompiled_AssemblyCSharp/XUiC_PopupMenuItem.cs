using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenuItem : XUiController
{
	public enum EEntryType
	{
		Button,
		Slider
	}

	public class Entry
	{
		public delegate void MenuItemClickedDelegate(Entry _entry);

		public delegate void MenuItemValueChangedDelegate(Entry _entry, double _newValue);

		public readonly string Text;

		public readonly string IconName;

		public readonly bool IsEnabled;

		public double Value;

		public readonly EEntryType EntryType;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public object Tag { get; set; }

		public event MenuItemClickedDelegate ItemClicked;

		public event MenuItemValueChangedDelegate ValueChanged;

		public void HandleItemClicked()
		{
			this.ItemClicked?.Invoke(this);
		}

		public void HandleValueChanged(double _newValue)
		{
			Value = _newValue;
			this.ValueChanged?.Invoke(this, Value);
		}

		public Entry(string _text, string _iconName, bool _isEnabled = true, object _tag = null, MenuItemClickedDelegate _handler = null)
		{
			Text = _text;
			IconName = _iconName;
			IsEnabled = _isEnabled;
			Tag = _tag;
			EntryType = EEntryType.Button;
			if (_handler != null)
			{
				ItemClicked += _handler;
			}
		}

		public Entry(string _text, string _iconName, double _initialValue, bool _isEnabled = true, object _tag = null, MenuItemValueChangedDelegate _handler = null)
		{
			Text = _text;
			IconName = _iconName;
			IsEnabled = _isEnabled;
			Tag = _tag;
			EntryType = EEntryType.Slider;
			Value = _initialValue;
			if (_handler != null)
			{
				ValueChanged += _handler;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PopupMenu parentPopup;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat slider;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entry itemEntry;

	public override void Init()
	{
		base.Init();
		base.OnPress += onPressed;
		base.OnHover += OnHovered;
		parentPopup = GetParentByType<XUiC_PopupMenu>();
		label = (XUiV_Label)GetChildById("lblText").ViewComponent;
		label.Overflow = UILabel.Overflow.ResizeFreely;
		slider = GetChildByType<XUiC_ComboBoxFloat>();
		slider.OnValueChanged += onValueChanged;
		slider.OnHoveredStateChanged += sliderHoveredChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sliderHoveredChanged(XUiController _sender, bool _isOverMainArea, bool _isOverAnyPart)
	{
		OnHovered(this, _isOverAnyPart);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHovered(XUiController _sender, bool _isOver)
	{
		isOver = _isOver;
		base.xui.currentPopupMenu.IsOver = _isOver;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "menuicon":
			_value = itemEntry?.IconName ?? "";
			return true;
		case "menutext":
			_value = itemEntry?.Text ?? "";
			return true;
		case "enabled":
			_value = (itemEntry?.IsEnabled ?? false).ToString();
			return true;
		case "hovered":
			_value = isOver.ToString();
			return true;
		case "type":
			_value = (itemEntry?.EntryType ?? EEntryType.Button).ToStringCached();
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressed(XUiController _sender, int _mouseButton)
	{
		if (itemEntry != null && itemEntry.EntryType == EEntryType.Button)
		{
			if (itemEntry.IsEnabled)
			{
				itemEntry.HandleItemClicked();
			}
			base.xui.currentPopupMenu.ClearItems();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		if (itemEntry != null && itemEntry.EntryType == EEntryType.Slider && itemEntry.IsEnabled)
		{
			itemEntry.HandleValueChanged(_newValue);
		}
	}

	public int SetEntry(Entry _entry)
	{
		itemEntry = _entry;
		RefreshBindings();
		label.SetTextImmediately(label.Text);
		base.ViewComponent.IsVisible = _entry != null;
		if (_entry == null)
		{
			return 0;
		}
		if (_entry.EntryType == EEntryType.Slider)
		{
			slider.Value = _entry.Value;
			slider.Enabled = _entry.IsEnabled;
		}
		if (_entry.EntryType != EEntryType.Button)
		{
			return parentPopup.SliderMinWidth;
		}
		return (int)label.Label.printedSize.x;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (base.ParseAttribute(_name, _value, _parent))
		{
			return true;
		}
		return false;
	}
}

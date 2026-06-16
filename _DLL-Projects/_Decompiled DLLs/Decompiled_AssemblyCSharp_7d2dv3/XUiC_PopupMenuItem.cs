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

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PopupMenu parentPopup;

	[XuiBindComponent("lblText", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxFloat slider;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entry itemEntry;

	[XuiXmlBinding("menuicon")]
	public string Icon
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return itemEntry?.IconName ?? "";
		}
	}

	[XuiXmlBinding("menutext")]
	public string Text
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return itemEntry?.Text ?? "";
		}
	}

	[XuiXmlBinding("enabled")]
	public bool EntryEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return itemEntry?.IsEnabled ?? false;
		}
	}

	[XuiXmlBinding("hovered")]
	public bool EntryHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return isOver;
		}
	}

	[XuiXmlBinding("type")]
	public EEntryType EntryType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return itemEntry?.EntryType ?? EEntryType.Button;
		}
	}

	public override void Init()
	{
		base.Init();
		label.Overflow = UILabel.Overflow.ResizeFreely;
	}

	[XuiBindEvent("OnHoveredStateChanged", "slider")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void sliderHoveredChanged(XUiController _sender, bool _isOverMainArea, bool _isOverAnyPart)
	{
		OnHovered(this, _isOverAnyPart);
	}

	[XuiBindEvent("OnHover", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHovered(XUiController _sender, bool _isOver)
	{
		isOver = _isOver;
		parentPopup.IsOver = _isOver;
		RefreshBindings();
	}

	[XuiBindEvent("OnPress", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressed(XUiController _sender, int _mouseButton)
	{
		Entry entry = itemEntry;
		if (entry != null && entry.EntryType == EEntryType.Button)
		{
			parentPopup.Close();
			if (entry.IsEnabled)
			{
				entry.HandleItemClicked();
			}
		}
	}

	[XuiBindEvent("OnValueChanged", "slider")]
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
		return (int)label.PrintedSize.x;
	}
}

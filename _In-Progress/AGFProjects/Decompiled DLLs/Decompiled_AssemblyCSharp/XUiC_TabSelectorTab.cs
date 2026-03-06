using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelectorTab : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector parentSelector;

	public XUiC_TabSelectorButton TabButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tabKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tabHeaderText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tabHeaderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabVisible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabSelected;

	public string TabKey
	{
		get
		{
			return tabKey;
		}
		set
		{
			tabKey = value;
			TabHeaderText = Localization.Get(tabKey);
		}
	}

	public string TabHeaderText
	{
		get
		{
			return tabHeaderText;
		}
		set
		{
			tabHeaderText = value;
			if (TabButton != null)
			{
				TabButton.IsDirty = true;
			}
		}
	}

	public string TabHeaderSprite
	{
		get
		{
			return tabHeaderSprite;
		}
		set
		{
			tabHeaderSprite = value;
			if (TabButton != null)
			{
				TabButton.IsDirty = true;
			}
		}
	}

	public bool TabVisible
	{
		get
		{
			return tabVisible;
		}
		set
		{
			if (value != tabVisible)
			{
				tabVisible = value;
				TabButton.UpdateVisibilityState();
				parentSelector.TabVisibilityChanged(this, tabVisible);
			}
		}
	}

	public bool TabSelected
	{
		get
		{
			return tabSelected;
		}
		set
		{
			if (value != tabSelected)
			{
				tabSelected = value;
				TabButton.UpdateSelectionState();
				base.ViewComponent.IsVisible = tabSelected;
				parentSelector.SelectedTab = this;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		parentSelector = GetParentByType<XUiC_TabSelector>();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "tab_key":
			TabKey = _value;
			return true;
		case "tab_caption":
			TabHeaderText = _value;
			return true;
		case "tab_caption_key":
			TabHeaderText = Localization.Get(_value);
			return true;
		case "tab_sprite":
			TabHeaderSprite = _value;
			return true;
		case "tab_visible":
			TabVisible = StringParsers.ParseBool(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}
}

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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabHighlight;

	[XuiXmlAttribute("tab_key", false)]
	public string TabKey
	{
		get
		{
			return tabKey;
		}
		set
		{
			if (!(tabKey == value))
			{
				tabKey = value;
				TabHeaderText = Localization.Get(tabKey);
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("tab_caption", false)]
	public string TabHeaderText
	{
		get
		{
			return tabHeaderText;
		}
		set
		{
			if (!(tabHeaderText == value))
			{
				tabHeaderText = value;
				if (TabButton != null)
				{
					TabButton.IsDirty = true;
				}
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("tab_sprite", false)]
	public string TabHeaderSprite
	{
		get
		{
			return tabHeaderSprite;
		}
		set
		{
			if (!(tabHeaderSprite == value))
			{
				tabHeaderSprite = value;
				if (TabButton != null)
				{
					TabButton.IsDirty = true;
				}
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("tab_visible", false)]
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
				TabButton?.UpdateVisibilityState();
				parentSelector?.TabVisibilityChanged(this, tabVisible);
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("tab_selected")]
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
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("tab_highlight")]
	public bool TabHighlight
	{
		get
		{
			return tabHighlight;
		}
		set
		{
			if (value != tabHighlight)
			{
				tabHighlight = value;
				TabButton.IsDirty = true;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("tab_caption_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeTabCaptionKey(string _value)
	{
		TabHeaderText = Localization.Get(_value);
	}

	public override void Init()
	{
		base.Init();
		parentSelector = GetParentByType<XUiC_TabSelector>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}
}

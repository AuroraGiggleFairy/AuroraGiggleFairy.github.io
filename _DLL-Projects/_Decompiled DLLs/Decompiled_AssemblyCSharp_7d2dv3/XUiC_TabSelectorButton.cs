using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelectorButton : XUiController
{
	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelector parentSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelectorTab tab;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SimpleButton simpleButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView interactionView;

	public Transform OuterDimensionsTransform;

	public XUiC_TabSelectorTab Tab
	{
		get
		{
			return tab;
		}
		set
		{
			if (tab != value)
			{
				if (tab != null)
				{
					tab.CustomAttributes.EntryModified -= tabCustomAttributesListener;
				}
				tab = value;
				if (tab != null)
				{
					tab.CustomAttributes.EntryModified += tabCustomAttributesListener;
				}
				RefreshBindings();
			}
		}
	}

	[XuiXmlBinding("tab_selected")]
	public bool TabSelected => Tab?.TabSelected ?? false;

	[XuiXmlBinding("tab_name")]
	public string TabName => Tab?.TabKey ?? "";

	[XuiXmlBinding("tab_name_localized")]
	public string TabNameLocalized => Tab?.TabHeaderText ?? "";

	[XuiXmlBinding("tab_sprite")]
	public string TabSprite => Tab?.TabHeaderSprite ?? "";

	[XuiXmlBinding("tab_highlight")]
	public bool TabHighlight => Tab?.TabHighlight ?? false;

	[XuiXmlBinding("tab_custom_attributes")]
	public ObservableDictionary<string, object> TabCustomAttributes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Tab?.CustomAttributes ?? CustomAttributes;
		}
	}

	public override void Init()
	{
		base.Init();
		OuterDimensionsTransform = base.ViewComponent.UiTransform.FindRecursive("border");
		if (simpleButton != null)
		{
			simpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				parentSelector.TabButtonClicked(this);
			};
			return;
		}
		interactionView = findClickableChild(this);
		if (interactionView == null)
		{
			Log.Error("[XUi] TabSelectorButton without SimpleButton or other view with 'on_press=true' in windowGroup '" + windowGroup.Id + "'");
			return;
		}
		interactionView.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
		{
			parentSelector.TabButtonClicked(this);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView findClickableChild(XUiController _controller)
	{
		if (_controller.ViewComponent.EventOnPress)
		{
			return _controller.ViewComponent;
		}
		foreach (XUiController child in _controller.Children)
		{
			XUiView xUiView = findClickableChild(child);
			if (xUiView != null)
			{
				return xUiView;
			}
		}
		return null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tabCustomAttributesListener(object _sender, DictionaryChangedEventArgs<string, object> _e)
	{
		RefreshBindings();
	}

	public void UpdateSelectionState()
	{
		if (simpleButton != null)
		{
			simpleButton.Button.Selected = Tab.TabSelected;
		}
		RefreshBindings();
	}

	public void UpdateVisibilityState()
	{
		base.ViewComponent.IsVisible = Tab.TabVisible;
	}

	public void PlayClickSound()
	{
		if (simpleButton != null)
		{
			simpleButton.Button.PlayClickSound();
		}
		else if (interactionView != null)
		{
			interactionView.PlayClickSound();
		}
	}
}

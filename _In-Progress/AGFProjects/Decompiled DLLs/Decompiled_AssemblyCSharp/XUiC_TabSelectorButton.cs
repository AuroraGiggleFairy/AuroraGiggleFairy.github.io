using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelectorButton : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector parentSelector;

	public XUiC_TabSelectorTab Tab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton simpleButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView interactionView;

	public Transform OuterDimensionsTransform;

	public override void Init()
	{
		base.Init();
		parentSelector = GetParentByType<XUiC_TabSelector>();
		OuterDimensionsTransform = base.ViewComponent.UiTransform.FindRecursive("border");
		XUiC_SimpleButton childByType = GetChildByType<XUiC_SimpleButton>();
		bool isSnappable;
		if (childByType != null)
		{
			simpleButton = childByType;
			childByType.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				parentSelector.TabButtonClicked(this);
			};
			XUiV_Button button = childByType.Button;
			isSnappable = (childByType.Button.IsNavigatable = false);
			button.IsSnappable = isSnappable;
			return;
		}
		interactionView = findClickableChild(this);
		if (interactionView == null)
		{
			Log.Error("[XUi] TabSelectorButton without SimpleButton or other view with 'on_press=true' in windowGroup '" + windowGroup.ID + "'");
			return;
		}
		interactionView.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
		{
			parentSelector.TabButtonClicked(this);
		};
		XUiView xUiView = interactionView;
		isSnappable = (interactionView.IsNavigatable = false);
		xUiView.IsSnappable = isSnappable;
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
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "tab_selected":
			_value = (Tab?.TabSelected ?? false).ToString();
			return true;
		case "tab_name_localized":
			_value = Tab?.TabHeaderText ?? "";
			return true;
		case "tab_sprite":
			_value = Tab?.TabHeaderSprite ?? "";
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
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

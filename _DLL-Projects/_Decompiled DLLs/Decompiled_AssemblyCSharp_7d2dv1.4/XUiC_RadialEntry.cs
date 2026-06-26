using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RadialEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label text;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color backgroundColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color highlightColor;

	public string SelectionText;

	public int MenuItemIndex;

	public int CommandIndex;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("icon");
		XUiController childById2 = GetChildById("background");
		XUiController childById3 = GetChildById("text");
		icon = childById.ViewComponent as XUiV_Sprite;
		background = childById2.ViewComponent as XUiV_Sprite;
		if (childById3 != null)
		{
			text = childById3.ViewComponent as XUiV_Label;
		}
		if (background != null)
		{
			backgroundColor = background.Color;
		}
	}

	public void SetHighlighted(bool _highlighted)
	{
		background.Color = (_highlighted ? highlightColor : backgroundColor);
	}

	public void SetSprite(string _sprite, Color _color)
	{
		icon.SpriteName = _sprite;
		icon.Color = _color;
	}

	public void SetText(string _text)
	{
		if (text != null)
		{
			text.Text = _text;
			text.IsVisible = _text != "";
		}
	}

	public void SetAtlas(string _atlas)
	{
		icon.UIAtlas = _atlas;
	}

	public void ResetScale()
	{
		SetScale(1f, _instant: true);
	}

	public void SetScale(float _scale, bool _instant = false)
	{
		float duration = (_instant ? 0f : 0.15f);
		TweenScale.Begin(viewComponent.UiTransform.gameObject, duration, Vector3.one * _scale);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(_name, _value, _parent);
		if (!flag && _name.EqualsCaseInsensitive("highlight_color"))
		{
			highlightColor = StringParsers.ParseColor32(_value);
			return true;
		}
		return flag;
	}
}

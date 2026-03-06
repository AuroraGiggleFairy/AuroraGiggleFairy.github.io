using System;
using UnityEngine;

public class XUiV_FilledSprite : XUiV_Sprite
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public BoxCollider boxCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i spriteBorder;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideFill;

	public override UIBasicSprite.Type Type
	{
		get
		{
			return type;
		}
		set
		{
		}
	}

	public XUiV_FilledSprite(string _id)
		: base(_id)
	{
	}

	public override void UpdateData()
	{
		if (applyAtlasAndSprite())
		{
			Vector4 border = sprite.border;
			spriteBorder = new Vector2i(Mathf.RoundToInt(border.x + border.z), Mathf.RoundToInt(border.y + border.w));
		}
		sprite.centerType = (fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
		int num = ((fillDirection == UIBasicSprite.FillDirection.Horizontal) ? Mathf.RoundToInt(fillAmount * (float)size.x) : size.x);
		int num2 = ((fillDirection == UIBasicSprite.FillDirection.Vertical) ? Mathf.RoundToInt(fillAmount * (float)size.y) : size.y);
		if (num != sprite.width || num2 != sprite.height || positionDirty)
		{
			positionDirty = false;
			sprite.SetDimensions(num, num2);
			hideFill = (fillDirection == UIBasicSprite.FillDirection.Horizontal && num < spriteBorder.x) || (fillDirection == UIBasicSprite.FillDirection.Vertical && num2 < spriteBorder.y);
			if (hideFill)
			{
				sprite.color = Color.clear;
			}
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			if (fillDirection == UIBasicSprite.FillDirection.Horizontal)
			{
				if (!fillInvert)
				{
					num3 = 0;
					num4 = Mathf.FloorToInt((float)(-size.x + num) / 2f);
					num5 = -size.x + num;
				}
				else
				{
					num3 = size.x - num;
					num4 = Mathf.CeilToInt((float)(size.x - num) / 2f);
					num5 = 0;
				}
			}
			else
			{
				if (fillDirection != UIBasicSprite.FillDirection.Vertical)
				{
					Log.Warning("[XUi] FilledSprite only allows FillDirections Horizontal and Vertical");
					return;
				}
				if (!fillInvert)
				{
					num6 = 0;
					num7 = Mathf.FloorToInt((float)(-size.y + num2) / 2f);
					num8 = -size.y + num2;
				}
				else
				{
					num6 = size.y - num2;
					num7 = Mathf.CeilToInt((float)(size.y - num2) / 2f);
					num8 = 0;
				}
			}
			int num9;
			switch (pivot)
			{
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.Left:
			case UIWidget.Pivot.BottomLeft:
				num9 = num3;
				break;
			case UIWidget.Pivot.Top:
			case UIWidget.Pivot.Center:
			case UIWidget.Pivot.Bottom:
				num9 = num4;
				break;
			case UIWidget.Pivot.TopRight:
			case UIWidget.Pivot.Right:
			case UIWidget.Pivot.BottomRight:
				num9 = num5;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			int num10;
			switch (pivot)
			{
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.Top:
			case UIWidget.Pivot.TopRight:
				num10 = num8;
				break;
			case UIWidget.Pivot.Left:
			case UIWidget.Pivot.Center:
			case UIWidget.Pivot.Right:
				num10 = num7;
				break;
			case UIWidget.Pivot.BottomLeft:
			case UIWidget.Pivot.Bottom:
			case UIWidget.Pivot.BottomRight:
				num10 = num6;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			positionDirty = false;
			uiTransform.localPosition = new Vector3(base.PaddedPosition.x + num9, base.PaddedPosition.y + num10, 0f);
			if (EventOnHover || EventOnPress)
			{
				boxCollider.center = sprite.localCenter;
				boxCollider.size = new Vector3(sprite.localSize.x * colliderScale, sprite.localSize.y * colliderScale, 0f);
			}
		}
		if (!hideFill && sprite.color != color)
		{
			sprite.color = color;
		}
		if (!hideFill && globalOpacityModifier != 0f && (foregroundLayer ? (base.xui.ForegroundGlobalOpacity < 1f) : (base.xui.BackgroundGlobalOpacity < 1f)))
		{
			float a = Mathf.Clamp01(color.a * (globalOpacityModifier * (foregroundLayer ? base.xui.ForegroundGlobalOpacity : base.xui.BackgroundGlobalOpacity)));
			sprite.color = new Color(color.r, color.g, color.b, a);
		}
		if (sprite.type != type)
		{
			sprite.type = type;
		}
		if (sprite.flip != flip)
		{
			sprite.flip = flip;
		}
		if (!initialized)
		{
			initialized = true;
			sprite.pivot = pivot;
			sprite.depth = depth;
			uiTransform.localScale = Vector3.one;
			uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
			if (EventOnHover || EventOnPress)
			{
				boxCollider = collider;
				boxCollider.center = sprite.localCenter;
				boxCollider.size = new Vector3(sprite.localSize.x * colliderScale, sprite.localSize.y * colliderScale, 0f);
			}
		}
		RefreshBoxCollider();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		base.FillCenter = true;
		type = UIBasicSprite.Type.Sliced;
		base.FillDirection = UIBasicSprite.FillDirection.Horizontal;
	}

	public override bool ParseAttribute(string _attribute, string _value, XUiController _parent)
	{
		if (!(_attribute == "type"))
		{
			return base.ParseAttribute(_attribute, _value, _parent);
		}
		return true;
	}
}

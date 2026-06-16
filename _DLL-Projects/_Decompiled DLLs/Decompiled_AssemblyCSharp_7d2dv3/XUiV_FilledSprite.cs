using System;
using UnityEngine;

public class XUiV_FilledSprite : XUiV_Sprite
{
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

	public XUiV_FilledSprite(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		if (applyAtlasAndSprite())
		{
			Vector4 border = sprite.border;
			spriteBorder = new Vector2i(Mathf.RoundToInt(border.x + border.z), Mathf.RoundToInt(border.y + border.w));
		}
		if (fillSpritePad)
		{
			XUiUtils.ApplyFillPaddedSprite(sprite, spriteName);
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
					Log.Warning("[XUi] FilledSprite only allows FillDirections Horizontal and Vertical. On " + base.Controller.GetParentWindow().ID + "." + base.ID);
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
			int num10 = num9;
			switch (pivot)
			{
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.Top:
			case UIWidget.Pivot.TopRight:
				num9 = num8;
				break;
			case UIWidget.Pivot.Left:
			case UIWidget.Pivot.Center:
			case UIWidget.Pivot.Right:
				num9 = num7;
				break;
			case UIWidget.Pivot.BottomLeft:
			case UIWidget.Pivot.Bottom:
			case UIWidget.Pivot.BottomRight:
				num9 = num6;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			int num11 = num9;
			positionDirty = false;
			uiTransform.localPosition = new Vector3(base.PaddedPosition.x + num10, base.PaddedPosition.y + num11, 0f);
		}
		if (!hideFill)
		{
			sprite.color = opacityModColor(color);
		}
		sprite.type = type;
		sprite.flip = flip;
		refreshBoxCollider();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
		float num = (float)size.x * 0.5f;
		float num2 = (float)size.y * 0.5f;
		float x;
		float y;
		switch (pivot)
		{
		case UIWidget.Pivot.TopLeft:
			x = num;
			y = 0f - num2;
			break;
		case UIWidget.Pivot.Top:
			x = 0f;
			y = 0f - num2;
			break;
		case UIWidget.Pivot.TopRight:
			x = 0f - num;
			y = 0f - num2;
			break;
		case UIWidget.Pivot.Left:
			x = num;
			y = 0f;
			break;
		case UIWidget.Pivot.Center:
			x = 0f;
			y = 0f;
			break;
		case UIWidget.Pivot.Right:
			x = 0f - num;
			y = 0f;
			break;
		case UIWidget.Pivot.BottomLeft:
			x = num;
			y = num2;
			break;
		case UIWidget.Pivot.Bottom:
			x = 0f;
			y = num2;
			break;
		case UIWidget.Pivot.BottomRight:
			x = 0f - num;
			y = num2;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		collider.center = new Vector3(x, y, 0f);
		collider.size = new Vector3((float)size.x * base.ColliderScale + (float)(2 * base.ColliderPadding.x), (float)size.y * base.ColliderScale + (float)(2 * base.ColliderPadding.y), 0f);
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		base.FillCenter = true;
		type = UIBasicSprite.Type.Sliced;
		base.FillDirection = UIBasicSprite.FillDirection.Horizontal;
	}
}

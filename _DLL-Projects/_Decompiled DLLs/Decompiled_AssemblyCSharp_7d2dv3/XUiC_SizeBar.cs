using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SizeBar : XUiController
{
	public struct BarRegionFloat(float offset, float size) : IEquatable<BarRegionFloat>
	{
		public readonly float Start = offset;

		public readonly float Size = size;

		public readonly float End = Start + Size;

		public static readonly BarRegionFloat None = new BarRegionFloat(0f, 0f);

		public bool Equals(BarRegionFloat other)
		{
			if (Start == other.Start)
			{
				return Size == other.Size;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int minBarWidth = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_used;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_excess;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_hovered_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public float used;

	[PublicizedFrom(EAccessModifier.Private)]
	public float allowance;

	[PublicizedFrom(EAccessModifier.Private)]
	public BarRegionFloat selectedRegion = BarRegionFloat.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public BarRegionFloat hoveredRegion = BarRegionFloat.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColor = new Color32(250, byte.MaxValue, 163, 193);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectionFillColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 normalBorderColor;

	public override void Init()
	{
		base.Init();
		border = (XUiV_Sprite)GetChildById("border").ViewComponent;
		background = (XUiV_Sprite)GetChildById("background").ViewComponent;
		bar_used = (XUiV_Sprite)GetChildById("bar_used").ViewComponent;
		bar_excess = (XUiV_Sprite)GetChildById("bar_excess").ViewComponent;
		bar_selected_fill = (XUiV_Sprite)GetChildById("bar_selected_fill").ViewComponent;
		bar_selected_outline = (XUiV_Sprite)GetChildById("bar_selected_outline").ViewComponent;
		bar_hovered_outline = (XUiV_Sprite)GetChildById("bar_hovered_outline").ViewComponent;
		normalBorderColor = border.Color;
		selectionFillColor = bar_selected_fill.Color;
		fullWidth = background.Size.x;
		fullHeight = background.Size.y;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	public void SetSelectedRegion(BarRegionFloat primaryRegion)
	{
		if (!selectedRegion.Equals(primaryRegion))
		{
			selectedRegion = primaryRegion;
			IsDirty = true;
		}
	}

	public void SetHoveredRegion(BarRegionFloat region)
	{
		if (!hoveredRegion.Equals(region))
		{
			hoveredRegion = region;
			IsDirty = true;
		}
	}

	public void SetUsed(float used)
	{
		if (this.used != used)
		{
			this.used = used;
			IsDirty = true;
		}
	}

	public void SetAllowance(float allowance)
	{
		if (this.allowance != allowance)
		{
			this.allowance = allowance;
			IsDirty = true;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Refresh()
	{
		bar_used.IsVisible = false;
		bar_excess.IsVisible = false;
		bar_selected_fill.IsVisible = false;
		bar_selected_outline.IsVisible = false;
		bar_hovered_outline.IsVisible = false;
		RefreshSelectionMode();
		RefreshBindings();
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshSelectionMode()
	{
		float pixelScaleFactor;
		int maxPosition;
		if (!(allowance <= 0f) && !(used <= 0f))
		{
			float num = Mathf.Max(allowance, used);
			pixelScaleFactor = ((num > 0f) ? ((float)fullWidth / num) : 0f);
			int val = Mathf.CeilToInt(pixelScaleFactor * used);
			bar_used.IsVisible = true;
			bar_used.Size = new Vector2i(Math.Max(val, minBarWidth), fullHeight);
			maxPosition = fullWidth;
			if (used > allowance)
			{
				bar_excess.IsVisible = true;
				int x = Mathf.Min(Mathf.CeilToInt(pixelScaleFactor * allowance), maxPosition - minBarWidth);
				int x2 = Math.Max(Mathf.FloorToInt(pixelScaleFactor * (used - allowance)), minBarWidth);
				bar_excess.Position = new Vector2i(x, 0);
				bar_excess.Size = new Vector2i(x2, fullHeight);
			}
			UpdateRegion(selectedRegion, bar_selected_fill, bar_selected_outline);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void GetPixelValues(BarRegionFloat region, int num2, out int position, out int width)
		{
			position = Mathf.Min(Mathf.FloorToInt(pixelScaleFactor * region.Start), num2 - minBarWidth);
			width = Mathf.Max(Mathf.CeilToInt(pixelScaleFactor * region.Size), minBarWidth);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void UpdateRegion(BarRegionFloat region, XUiV_Sprite fillSprite, XUiV_Sprite outlineSprite)
		{
			if (hoveredRegion.Size > 0f)
			{
				GetPixelValues(hoveredRegion, maxPosition, out var position, out var width);
				bar_hovered_outline.IsVisible = true;
				bar_hovered_outline.Position = new Vector2i(position - 1, 0);
				bar_hovered_outline.Size = new Vector2i(width + 2, fullHeight);
			}
			if (!(region.Size <= 0f))
			{
				GetPixelValues(region, maxPosition, out var position2, out var width2);
				fillSprite.IsVisible = true;
				fillSprite.Position = new Vector2i(position2, 0);
				fillSprite.Size = new Vector2i(width2, fullHeight);
				fillSprite.Color = selectionFillColor;
				outlineSprite.IsVisible = true;
				outlineSprite.Position = fillSprite.Position;
				outlineSprite.Size = fillSprite.Size;
				outlineSprite.Color = selectionOutlineColor;
				maxPosition = position2 + width2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "showwarningoverlay")
		{
			_value = (used > allowance).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}

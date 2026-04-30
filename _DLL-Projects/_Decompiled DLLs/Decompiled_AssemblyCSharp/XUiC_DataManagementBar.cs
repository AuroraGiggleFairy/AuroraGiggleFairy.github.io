using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DataManagementBar : XUiController
{
	public enum SelectionDepth
	{
		Primary,
		Secondary,
		Tertiary
	}

	public struct BarRegion(long offset, long size) : IEquatable<BarRegion>
	{
		public readonly long Start = offset;

		public readonly long Size = size;

		public readonly long End = Start + Size;

		public static readonly BarRegion None = new BarRegion(0L, 0L);

		public bool Equals(BarRegion other)
		{
			if (Start == other.Start)
			{
				return Size == other.Size;
			}
			return false;
		}
	}

	public enum DisplayMode
	{
		Selection,
		Preview
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_used;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_required;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_pending;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_primary_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_secondary_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_tertiary_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_primary_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_secondary_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_tertiary_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_hovered_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public DisplayMode displayMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public long usedBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long allowanceBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public BarRegion primaryByteRegion = BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public BarRegion secondaryByteRegion = BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public BarRegion tertiaryByteRegion = BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public BarRegion hoveredByteRegion = BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public BarRegion archivePreviewByteRegion = BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public SelectionDepth focusedSelectionDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deleteHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deleteWindowDisplayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bytesToPixels;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColor = new Color32(250, byte.MaxValue, 163, 193);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColorFade = new Color32(250, byte.MaxValue, 163, 86);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColorDelete = new Color32(234, 67, 53, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectionFillColor;

	public override void Init()
	{
		base.Init();
		background = (XUiV_Sprite)GetChildById("background").ViewComponent;
		bar_used = (XUiV_Sprite)GetChildById("bar_used").ViewComponent;
		bar_selected_primary_fill = (XUiV_Sprite)GetChildById("bar_selected_primary_fill").ViewComponent;
		bar_selected_secondary_fill = (XUiV_Sprite)GetChildById("bar_selected_secondary_fill").ViewComponent;
		bar_selected_tertiary_fill = (XUiV_Sprite)GetChildById("bar_selected_tertiary_fill").ViewComponent;
		bar_selected_primary_outline = (XUiV_Sprite)GetChildById("bar_selected_primary_outline").ViewComponent;
		bar_selected_secondary_outline = (XUiV_Sprite)GetChildById("bar_selected_secondary_outline").ViewComponent;
		bar_selected_tertiary_outline = (XUiV_Sprite)GetChildById("bar_selected_tertiary_outline").ViewComponent;
		bar_hovered_outline = (XUiV_Sprite)GetChildById("bar_hovered_outline").ViewComponent;
		bar_required = (XUiV_Sprite)GetChildById("bar_required").ViewComponent;
		bar_pending = (XUiV_Sprite)GetChildById("bar_pending").ViewComponent;
		selectionFillColor = bar_selected_primary_fill.Color;
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

	public void SetDisplayMode(DisplayMode displayMode)
	{
		if (this.displayMode != displayMode)
		{
			this.displayMode = displayMode;
			IsDirty = true;
		}
	}

	public void SetSelectedByteRegion(BarRegion primaryRegion)
	{
		SetSelectedByteRegion(primaryRegion, BarRegion.None, BarRegion.None);
	}

	public void SetSelectedByteRegion(BarRegion primaryRegion, BarRegion secondaryRegion)
	{
		SetSelectedByteRegion(primaryRegion, secondaryRegion, BarRegion.None);
	}

	public void SetSelectedByteRegion(BarRegion primaryRegion, BarRegion secondaryRegion, BarRegion tertiaryRegion)
	{
		if (!primaryByteRegion.Equals(primaryRegion))
		{
			primaryByteRegion = primaryRegion;
			IsDirty = true;
		}
		if (!secondaryByteRegion.Equals(secondaryRegion))
		{
			secondaryByteRegion = secondaryRegion;
			IsDirty = true;
		}
		if (!tertiaryByteRegion.Equals(tertiaryRegion))
		{
			tertiaryByteRegion = tertiaryRegion;
			IsDirty = true;
		}
	}

	public void SetHoveredByteRegion(BarRegion region)
	{
		if (!hoveredByteRegion.Equals(region))
		{
			hoveredByteRegion = region;
			IsDirty = true;
		}
	}

	public void SetArchivePreviewRegion(BarRegion region)
	{
		if (!archivePreviewByteRegion.Equals(region))
		{
			archivePreviewByteRegion = region;
			IsDirty = true;
		}
	}

	public void SetSelectionDepth(SelectionDepth selectionDepth)
	{
		if (focusedSelectionDepth != selectionDepth)
		{
			focusedSelectionDepth = selectionDepth;
			IsDirty = true;
		}
	}

	public void SetDeleteHovered(bool hovered)
	{
		if (deleteHovered != hovered)
		{
			deleteHovered = hovered;
			IsDirty = true;
		}
	}

	public void SetDeleteWindowDisplayed(bool displayed)
	{
		if (deleteWindowDisplayed != displayed)
		{
			deleteWindowDisplayed = displayed;
			IsDirty = true;
		}
	}

	public void SetUsedBytes(long usedBytes)
	{
		if (this.usedBytes != usedBytes)
		{
			this.usedBytes = usedBytes;
			IsDirty = true;
		}
	}

	public void SetAllowanceBytes(long allowanceBytes)
	{
		if (this.allowanceBytes != allowanceBytes)
		{
			this.allowanceBytes = allowanceBytes;
			bytesToPixels = (((float)allowanceBytes > 0f) ? ((float)fullWidth / (float)allowanceBytes) : 0f);
			IsDirty = true;
		}
	}

	public void SetPendingBytes(long pendingBytes)
	{
		if (this.pendingBytes != pendingBytes)
		{
			this.pendingBytes = pendingBytes;
			IsDirty = true;
		}
	}

	public long GetPendingBytes()
	{
		return pendingBytes;
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
		bar_selected_primary_fill.IsVisible = false;
		bar_selected_secondary_fill.IsVisible = false;
		bar_selected_tertiary_fill.IsVisible = false;
		bar_selected_primary_outline.IsVisible = false;
		bar_selected_secondary_outline.IsVisible = false;
		bar_selected_tertiary_outline.IsVisible = false;
		bar_hovered_outline.IsVisible = false;
		bar_required.IsVisible = false;
		bar_pending.IsVisible = false;
		DisplayMode displayMode = this.displayMode;
		if (displayMode != DisplayMode.Selection && displayMode == DisplayMode.Preview)
		{
			RefreshPreviewMode();
		}
		else
		{
			RefreshSelectionMode();
		}
		RefreshBindings(_forceAll: true);
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshPreviewMode()
	{
		if (allowanceBytes <= 0 || (usedBytes <= 0 && pendingBytes <= 0))
		{
			return;
		}
		int num = Mathf.CeilToInt(bytesToPixels * (float)usedBytes);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (pendingBytes > 0)
		{
			long num5 = allowanceBytes - usedBytes;
			if (num5 < pendingBytes)
			{
				num3 = fullWidth - num;
				long num6 = pendingBytes - num5;
				num2 = Math.Clamp(Mathf.CeilToInt(bytesToPixels * (float)num6), 3, num);
				num4 = num - num2;
			}
			else
			{
				num3 = Math.Clamp(Mathf.CeilToInt(bytesToPixels * (float)pendingBytes), 3, fullWidth - 3);
				num4 = Math.Min(num, fullWidth - num3);
			}
		}
		else
		{
			num4 = num;
		}
		if (num4 > 0)
		{
			bar_used.IsVisible = true;
			bar_used.Size = new Vector2i(num4, fullHeight);
		}
		if (num2 > 0)
		{
			bar_required.IsVisible = true;
			bar_required.Position = new Vector2i(num4, 0);
			bar_required.Size = new Vector2i(num2, fullHeight);
		}
		if (num3 > 0)
		{
			bar_pending.IsVisible = true;
			bar_pending.Position = new Vector2i(num4 + num2, 0);
			bar_pending.Size = new Vector2i(num3, fullHeight);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshSelectionMode()
	{
		int maxPosition;
		if (allowanceBytes > 0 && usedBytes > 0)
		{
			int val = Mathf.CeilToInt(bytesToPixels * (float)usedBytes);
			bar_used.IsVisible = true;
			bar_used.Size = new Vector2i(Math.Max(val, 8), fullHeight);
			maxPosition = fullWidth;
			UpdateRegion(primaryByteRegion, bar_selected_primary_fill, bar_selected_primary_outline, SelectionDepth.Primary);
			UpdateRegion(secondaryByteRegion, bar_selected_secondary_fill, bar_selected_secondary_outline, SelectionDepth.Secondary);
			UpdateRegion(tertiaryByteRegion, bar_selected_tertiary_fill, bar_selected_tertiary_outline, SelectionDepth.Tertiary);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void GetPixelValues(BarRegion byteRegion, int num, out int position, out int width)
		{
			position = Mathf.Min(Mathf.FloorToInt(bytesToPixels * (float)byteRegion.Start), num - 8);
			width = Mathf.Max(Mathf.CeilToInt(bytesToPixels * (float)byteRegion.Size), 8);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void UpdateRegion(BarRegion byteRegion, XUiV_Sprite fillSprite, XUiV_Sprite outlineSprite, SelectionDepth depth)
		{
			if (focusedSelectionDepth == depth && hoveredByteRegion.Size > 0)
			{
				GetPixelValues(hoveredByteRegion, maxPosition, out var position, out var width);
				bar_hovered_outline.IsVisible = true;
				bar_hovered_outline.Position = new Vector2i(position, 0);
				bar_hovered_outline.Size = new Vector2i(width, fullHeight);
			}
			if (byteRegion.Size > 0)
			{
				GetPixelValues(byteRegion, maxPosition, out var position2, out var width2);
				fillSprite.IsVisible = true;
				fillSprite.Position = new Vector2i(position2, 0);
				fillSprite.Size = new Vector2i(width2, fullHeight);
				float num = ((focusedSelectionDepth == SelectionDepth.Secondary) ? 1f : 0.5f) * (float)(focusedSelectionDepth - depth);
				Color a = selectionFillColor;
				a = Color.Lerp(a, Color.white, 0.5f * Mathf.Abs(num));
				bool flag = deleteHovered || deleteWindowDisplayed;
				if (num > 0f)
				{
					a.a = Mathf.Lerp(a.a, 0.5f, num);
				}
				else if (flag)
				{
					a = Color.Lerp(a, selectionOutlineColorDelete, 0.2f);
				}
				fillSprite.Color = a;
				if (!flag || num >= 0f)
				{
					outlineSprite.IsVisible = true;
					outlineSprite.Position = fillSprite.Position;
					outlineSprite.Size = fillSprite.Size;
					Color color = ((focusedSelectionDepth != depth) ? ((Color)selectionOutlineColorFade) : ((Color)(flag ? selectionOutlineColorDelete : selectionOutlineColor)));
					outlineSprite.Color = color;
				}
				if (focusedSelectionDepth == depth && archivePreviewByteRegion.Size > 0)
				{
					GetPixelValues(archivePreviewByteRegion, maxPosition, out var position3, out var width3);
					bar_pending.IsVisible = true;
					bar_pending.Position = new Vector2i(position3, 0);
					bar_pending.Size = new Vector2i(width3, fullHeight);
				}
				maxPosition = position2 + width2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "warningtext")
		{
			long num = allowanceBytes - usedBytes;
			if (displayMode == DisplayMode.Selection || num >= pendingBytes)
			{
				_value = string.Empty;
				return true;
			}
			long bytes = pendingBytes - num;
			_value = string.Format(Localization.Get("xuiDmBarRequiredSpaceWarning"), XUiC_DataManagement.FormatMemoryString(bytes));
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}

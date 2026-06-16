using System;
using Platform;
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

	public readonly struct BarRegion(long _offset, long _size) : IEquatable<BarRegion>
	{
		public readonly long Start = _offset;

		public readonly long Size = _size;

		public readonly long End = Start + Size;

		public static readonly BarRegion None = new BarRegion(0L, 0L);

		public bool Equals(BarRegion _other)
		{
			if (Start == _other.Start)
			{
				return Size == _other.Size;
			}
			return false;
		}
	}

	public enum DisplayMode
	{
		Selection,
		Preview
	}

	[XuiBindComponent("background", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite background;

	[XuiBindComponent("bar_used", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barUsed;

	[XuiBindComponent("bar_required", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barRequired;

	[XuiBindComponent("bar_pending", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barPending;

	[XuiBindComponent("bar_selected_primary_fill", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedPrimaryFill;

	[XuiBindComponent("bar_selected_secondary_fill", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedSecondaryFill;

	[XuiBindComponent("bar_selected_tertiary_fill", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedTertiaryFill;

	[XuiBindComponent("bar_selected_primary_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedPrimaryOutline;

	[XuiBindComponent("bar_selected_secondary_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedSecondaryOutline;

	[XuiBindComponent("bar_selected_tertiary_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedTertiaryOutline;

	[XuiBindComponent("bar_hovered_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barHoveredOutline;

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

	[XuiXmlBinding("isroamingoptional")]
	public bool IsRoamingOptional
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
		}
	}

	[XuiXmlBinding("warningtext")]
	public string WarningText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			long num = allowanceBytes - usedBytes;
			if (displayMode == DisplayMode.Selection || num >= pendingBytes)
			{
				return string.Empty;
			}
			long bytes = pendingBytes - num;
			return string.Format(Localization.Get("xuiDmBarRequiredSpaceWarning"), ValueDisplayFormatters.MemoryMiB(bytes));
		}
	}

	public override void Init()
	{
		base.Init();
		selectionFillColor = barSelectedPrimaryFill.Color;
		fullWidth = background.Size.x;
		fullHeight = background.Size.y;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	public void SetDisplayMode(DisplayMode _displayMode)
	{
		if (displayMode != _displayMode)
		{
			displayMode = _displayMode;
			IsDirty = true;
		}
	}

	public void SetSelectedByteRegion(BarRegion _primaryRegion)
	{
		SetSelectedByteRegion(_primaryRegion, BarRegion.None, BarRegion.None);
	}

	public void SetSelectedByteRegion(BarRegion _primaryRegion, BarRegion _secondaryRegion)
	{
		SetSelectedByteRegion(_primaryRegion, _secondaryRegion, BarRegion.None);
	}

	public void SetSelectedByteRegion(BarRegion _primaryRegion, BarRegion _secondaryRegion, BarRegion _tertiaryRegion)
	{
		if (!primaryByteRegion.Equals(_primaryRegion))
		{
			primaryByteRegion = _primaryRegion;
			IsDirty = true;
		}
		if (!secondaryByteRegion.Equals(_secondaryRegion))
		{
			secondaryByteRegion = _secondaryRegion;
			IsDirty = true;
		}
		if (!tertiaryByteRegion.Equals(_tertiaryRegion))
		{
			tertiaryByteRegion = _tertiaryRegion;
			IsDirty = true;
		}
	}

	public void SetHoveredByteRegion(BarRegion _region)
	{
		if (!hoveredByteRegion.Equals(_region))
		{
			hoveredByteRegion = _region;
			IsDirty = true;
		}
	}

	public void SetArchivePreviewRegion(BarRegion _region)
	{
		if (!archivePreviewByteRegion.Equals(_region))
		{
			archivePreviewByteRegion = _region;
			IsDirty = true;
		}
	}

	public void SetSelectionDepth(SelectionDepth _selectionDepth)
	{
		if (focusedSelectionDepth != _selectionDepth)
		{
			focusedSelectionDepth = _selectionDepth;
			IsDirty = true;
		}
	}

	public void SetDeleteHovered(bool _hovered)
	{
		if (deleteHovered != _hovered)
		{
			deleteHovered = _hovered;
			IsDirty = true;
		}
	}

	public void SetDeleteWindowDisplayed(bool _displayed)
	{
		if (deleteWindowDisplayed != _displayed)
		{
			deleteWindowDisplayed = _displayed;
			IsDirty = true;
		}
	}

	public void SetUsedBytes(long _usedBytes)
	{
		if (usedBytes != _usedBytes)
		{
			usedBytes = _usedBytes;
			IsDirty = true;
		}
	}

	public void SetAllowanceBytes(long _allowanceBytes)
	{
		if (allowanceBytes != _allowanceBytes)
		{
			allowanceBytes = _allowanceBytes;
			bytesToPixels = (((float)_allowanceBytes > 0f) ? ((float)fullWidth / (float)_allowanceBytes) : 0f);
			IsDirty = true;
		}
	}

	public void SetPendingBytes(long _pendingBytes)
	{
		if (pendingBytes != _pendingBytes)
		{
			pendingBytes = _pendingBytes;
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
			refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refresh()
	{
		barUsed.IsVisible = false;
		barSelectedPrimaryFill.IsVisible = false;
		barSelectedSecondaryFill.IsVisible = false;
		barSelectedTertiaryFill.IsVisible = false;
		barSelectedPrimaryOutline.IsVisible = false;
		barSelectedSecondaryOutline.IsVisible = false;
		barSelectedTertiaryOutline.IsVisible = false;
		barHoveredOutline.IsVisible = false;
		barRequired.IsVisible = false;
		barPending.IsVisible = false;
		DisplayMode displayMode = this.displayMode;
		if (displayMode != DisplayMode.Selection && displayMode == DisplayMode.Preview)
		{
			refreshPreviewMode();
		}
		else
		{
			refreshSelectionMode();
		}
		RefreshBindings();
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshPreviewMode()
	{
		if (allowanceBytes <= 0 || (usedBytes <= 0 && pendingBytes <= 0))
		{
			return;
		}
		int num = Mathf.CeilToInt(bytesToPixels * (float)usedBytes);
		int num2 = 0;
		int num3 = 0;
		int num6;
		if (pendingBytes > 0)
		{
			long num4 = allowanceBytes - usedBytes;
			if (num4 < pendingBytes)
			{
				num3 = fullWidth - num;
				long num5 = pendingBytes - num4;
				num2 = Math.Clamp(Mathf.CeilToInt(bytesToPixels * (float)num5), 3, num);
				num6 = num - num2;
			}
			else
			{
				num3 = Math.Clamp(Mathf.CeilToInt(bytesToPixels * (float)pendingBytes), 3, fullWidth - 3);
				num6 = Math.Min(num, fullWidth - num3);
			}
		}
		else
		{
			num6 = num;
		}
		if (num6 > 0)
		{
			barUsed.IsVisible = true;
			barUsed.Size = new Vector2i(num6, fullHeight);
		}
		if (num2 > 0)
		{
			barRequired.IsVisible = true;
			barRequired.Position = new Vector2i(num6, 0);
			barRequired.Size = new Vector2i(num2, fullHeight);
		}
		if (num3 > 0)
		{
			barPending.IsVisible = true;
			barPending.Position = new Vector2i(num6 + num2, 0);
			barPending.Size = new Vector2i(num3, fullHeight);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshSelectionMode()
	{
		int maxPosition;
		if (allowanceBytes > 0 && usedBytes > 0)
		{
			int val = Mathf.CeilToInt(bytesToPixels * (float)usedBytes);
			barUsed.IsVisible = true;
			barUsed.Size = new Vector2i(Math.Max(val, 8), fullHeight);
			maxPosition = fullWidth;
			UpdateRegion(primaryByteRegion, barSelectedPrimaryFill, barSelectedPrimaryOutline, SelectionDepth.Primary);
			UpdateRegion(secondaryByteRegion, barSelectedSecondaryFill, barSelectedSecondaryOutline, SelectionDepth.Secondary);
			UpdateRegion(tertiaryByteRegion, barSelectedTertiaryFill, barSelectedTertiaryOutline, SelectionDepth.Tertiary);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void GetPixelValues(BarRegion _byteRegion, int _maxPosition, out int _position, out int _width)
		{
			_position = Mathf.Min(Mathf.FloorToInt(bytesToPixels * (float)_byteRegion.Start), _maxPosition - 8);
			_width = Mathf.Max(Mathf.CeilToInt(bytesToPixels * (float)_byteRegion.Size), 8);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void UpdateRegion(BarRegion _byteRegion, XUiV_Sprite _fillSprite, XUiV_Sprite _outlineSprite, SelectionDepth _depth)
		{
			if (focusedSelectionDepth == _depth && hoveredByteRegion.Size > 0)
			{
				GetPixelValues(hoveredByteRegion, maxPosition, out var _position, out var _width);
				barHoveredOutline.IsVisible = true;
				barHoveredOutline.Position = new Vector2i(_position, 0);
				barHoveredOutline.Size = new Vector2i(_width, fullHeight);
			}
			if (_byteRegion.Size > 0)
			{
				GetPixelValues(_byteRegion, maxPosition, out var _position2, out var _width2);
				_fillSprite.IsVisible = true;
				_fillSprite.Position = new Vector2i(_position2, 0);
				_fillSprite.Size = new Vector2i(_width2, fullHeight);
				float num = ((focusedSelectionDepth == SelectionDepth.Secondary) ? 1f : 0.5f) * (float)(focusedSelectionDepth - _depth);
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
				_fillSprite.Color = a;
				if (!flag || num >= 0f)
				{
					_outlineSprite.IsVisible = true;
					_outlineSprite.Position = _fillSprite.Position;
					_outlineSprite.Size = _fillSprite.Size;
					Color color = ((focusedSelectionDepth != _depth) ? ((Color)selectionOutlineColorFade) : ((Color)(flag ? selectionOutlineColorDelete : selectionOutlineColor)));
					_outlineSprite.Color = color;
				}
				if (focusedSelectionDepth == _depth && archivePreviewByteRegion.Size > 0)
				{
					GetPixelValues(archivePreviewByteRegion, maxPosition, out var _position3, out var _width3);
					barPending.IsVisible = true;
					barPending.Position = new Vector2i(_position3, 0);
					barPending.Size = new Vector2i(_width3, fullHeight);
				}
				maxPosition = _position2 + _width2;
			}
		}
	}
}

using System;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_SignGridEntry : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	public Action<XUiC_SignGridEntry> OnBecameSelected;

	public virtual Color32 BackgroundColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return backgroundColor;
		}
	}

	public virtual bool IsSelectable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public XUiC_SignGridEntry()
	{
		IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		highlightOverlay = GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite;
		background = GetChildById("background").ViewComponent as XUiV_Sprite;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		base.SelectedChanged(isSelected);
		if (isSelected)
		{
			background.Color = selectColor;
			OnBecameSelected?.Invoke(this);
		}
		else
		{
			background.Color = BackgroundColor;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		PlayerActionsGUI gUIActions = xui.playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			CursorControllerAbs cursorController = xui.playerUI.CursorController;
			_ = (Vector3)cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (isOver && base.ViewComponent.UiTransformIsHovered && base.ViewComponent.EventOnPress)
			{
				if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					if (gUIActions.Submit.WasReleased && IsSelectable)
					{
						HandleClick();
					}
				}
				else if (mouseButtonUp && IsSelectable)
				{
					HandleClick();
				}
			}
			else
			{
				if (highlightOverlay != null)
				{
					highlightOverlay.Color = BackgroundColor;
				}
				if (!base.IsSelected)
				{
					background.Color = BackgroundColor;
				}
				if (isOver)
				{
					isOver = false;
				}
			}
		}
		if (IsDirty)
		{
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleClick()
	{
		base.IsSelected = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isOver = _isOver;
		if (!base.IsSelected)
		{
			if (_isOver)
			{
				background.Color = highlightColor;
			}
			else
			{
				background.Color = BackgroundColor;
			}
		}
		base.OnHovered(_isOver);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			switch (name)
			{
			case "select_color":
				selectColor = StringParsers.ParseColor32(value);
				break;
			case "background_color":
				backgroundColor = StringParsers.ParseColor32(value);
				break;
			case "highlight_color":
				highlightColor = StringParsers.ParseColor32(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}
}

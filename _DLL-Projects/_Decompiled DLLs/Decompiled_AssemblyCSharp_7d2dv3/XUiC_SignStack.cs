using System;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_SignStack : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	public static Color32 libraryMarkerColorDefault = new Color32(120, 0, 200, byte.MaxValue);

	public static Color32 libraryMarkerColorPrefab = new Color32(45, 220, 30, byte.MaxValue);

	public static Color32 libraryMarkerColorUser = new Color32(0, 170, 190, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 libraryColor;

	public SignData signData;

	public string libraryName;

	public Action<XUiC_SignStack> OnBecameSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId signId;

	public SignData SignData => signData;

	public string LibraryName => libraryName;

	public GlobalSignId SignId
	{
		get
		{
			return signId;
		}
		set
		{
			if (signId != value)
			{
				signId = value;
				IsDirty = true;
				if (!signId.IsValid)
				{
					SetItemNameText("");
					libraryColor = backgroundColor;
				}
				else
				{
					libraryName = SignDataManager.GetLibraryNiceName(signId);
					if (!SignDataManager.Instance.TryGetSignData(signId, out signData))
					{
						SetItemNameText("");
						libraryColor = backgroundColor;
						Log.Error("Failed to retrieve sign data for global id: '" + signId.ToString() + "'.");
					}
					else
					{
						SetItemNameText(signData.name + " (" + libraryName + ")");
						string libraryId = signId.libraryId;
						if (!(libraryId == "[D]"))
						{
							if (libraryId == "[U]")
							{
								libraryColor = libraryMarkerColorUser;
							}
							else
							{
								libraryColor = libraryMarkerColorPrefab;
							}
						}
						else
						{
							libraryColor = libraryMarkerColorDefault;
						}
					}
				}
			}
			signMaterial.IsVisible = signId.IsValid;
			base.ViewComponent.Enabled = signId.IsValid;
			SetColor(libraryColor);
			RefreshBindings();
		}
	}

	public XUiC_SignStack()
	{
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			SetColor(selectColor);
			OnBecameSelected?.Invoke(this);
		}
		else
		{
			SetColor(libraryColor);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetColor(Color32 color)
	{
		background.Color = color;
	}

	public override void Init()
	{
		base.Init();
		highlightOverlay = GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite;
		background = GetChildById("background").ViewComponent as XUiV_Sprite;
		signMaterial = GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xUiV_Texture = signMaterial;
		xUiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(OnWillRender));
		signMaterial.Texture = Texture2D.whiteTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		SignDataManager.Instance.TryApplyRenderingData(signId, 1f, mat);
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
					if (gUIActions.Submit.WasReleased)
					{
						_ = signId;
						base.IsSelected = true;
					}
				}
				else if (mouseButtonUp)
				{
					_ = signId;
					base.IsSelected = true;
				}
			}
			else
			{
				currentColor = libraryColor;
				if (highlightOverlay != null)
				{
					highlightOverlay.Color = backgroundColor;
				}
				if (!base.IsSelected)
				{
					background.Color = currentColor;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetItemNameText(string name)
	{
		XUiView xUiView = viewComponent;
		_ = signId;
		xUiView.ToolTip = name;
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
				background.Color = libraryColor;
			}
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		return base.GetBindingValueInternal(ref value, bindingName);
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
			case "lib_marker_color_default":
				libraryMarkerColorDefault = StringParsers.ParseColor32(value);
				break;
			case "lib_marker_color_prefab":
				libraryMarkerColorPrefab = StringParsers.ParseColor32(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}
}

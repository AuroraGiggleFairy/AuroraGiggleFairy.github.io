using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_MaterialStack : XUiC_SelectableEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip selectSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClicked;

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label stackValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTextureData textureData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsLocked
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public BlockTextureData TextureData
	{
		get
		{
			return textureData;
		}
		set
		{
			textMaterial.IsVisible = false;
			base.ViewComponent.Enabled = value != null;
			if (textureData != value)
			{
				textureData = value;
				isDirty = true;
				if (textureData == null)
				{
					SetItemNameText("");
					IsLocked = false;
				}
				else
				{
					textMaterial.IsVisible = true;
					MeshDescription meshDescription = MeshDescription.meshes[0];
					int textureID = textureData.TextureID;
					Rect uVRect = ((textureID != 0) ? meshDescription.textureAtlas.uvMapping[textureID].uv : WorldConstants.uvRectZero);
					textMaterial.Texture = meshDescription.textureAtlas.diffuseTexture;
					if (meshDescription.bTextureArray)
					{
						textMaterial.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
						textMaterial.Material.SetFloat("_Index", meshDescription.textureAtlas.uvMapping[textureID].index);
						textMaterial.Material.SetFloat("_Size", meshDescription.textureAtlas.uvMapping[textureID].blockW);
					}
					else
					{
						textMaterial.UVRect = uVRect;
					}
					SetItemNameText($"({textureData.ID}) {textureData.LocalizedName}");
				}
			}
			if (textureData != null)
			{
				if (!(textureData.LockedByPerk != ""))
				{
					IsLocked = false;
				}
				textMaterial.IsVisible = true;
			}
			RefreshBindings();
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MaterialInfoWindow InfoWindow { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			SetColor(selectColor);
			if (base.xui.currentSelectedEntry == this)
			{
				InfoWindow.SetMaterial(textureData);
			}
		}
		else
		{
			SetColor(backgroundColor);
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
		tintedOverlay = GetChildById("tintedOverlay");
		highlightOverlay = GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite;
		background = GetChildById("background").ViewComponent as XUiV_Sprite;
		textMaterial = GetChildById("textMaterial").ViewComponent as XUiV_Texture;
		textMaterial.CreateMaterial();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
			_ = (Vector3)cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
			{
				if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					bool wasReleased = gUIActions.Submit.WasReleased;
					_ = gUIActions.HalfStack.WasReleased;
					_ = gUIActions.Inspect.WasReleased;
					_ = gUIActions.RightStick.WasReleased;
					if (wasReleased && textureData != null)
					{
						SetSelectedTextureForItem();
					}
				}
				else if (mouseButtonUp && textureData != null)
				{
					SetSelectedTextureForItem();
				}
			}
			else
			{
				currentColor = backgroundColor;
				if (highlightOverlay != null)
				{
					highlightOverlay.Color = backgroundColor;
				}
				if (!base.Selected)
				{
					background.Color = currentColor;
				}
				lastClicked = false;
				if (isOver)
				{
					isOver = false;
				}
			}
		}
		if (isDirty)
		{
			isDirty = false;
		}
	}

	public void SetSelectedTextureForItem()
	{
		if (!IsLocked)
		{
			if (base.xui.playerUI.entityPlayer.inventory.holdingItem is ItemClassBlock)
			{
				base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.TextureFullArray[0] = Chunk.TextureIdxToTextureFullValue64(textureData.ID);
			}
			else
			{
				((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx = textureData.ID;
				base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.Meta = (byte)textureData.ID;
				base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1].invData.itemValue = base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue;
			}
		}
		base.Selected = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleItemInspect()
	{
		if (textureData != null && InfoWindow != null)
		{
			base.Selected = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetItemNameText(string name)
	{
		viewComponent.ToolTip = ((textureData != null) ? name : string.Empty);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		isOver = _isOver;
		if (!base.Selected)
		{
			if (_isOver)
			{
				background.Color = highlightColor;
			}
			else
			{
				background.Color = backgroundColor;
			}
		}
		base.OnHovered(_isOver);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
	}

	public void ClearSelectedInfoWindow()
	{
		if (base.Selected)
		{
			InfoWindow.SetMaterial(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "islocked")
		{
			value = IsLocked.ToString();
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "select_color":
				selectColor = StringParsers.ParseColor32(value);
				break;
			case "press_color":
				pressColor = StringParsers.ParseColor32(value);
				break;
			case "background_color":
				backgroundColor = StringParsers.ParseColor32(value);
				break;
			case "highlight_color":
				highlightColor = StringParsers.ParseColor32(value);
				break;
			case "select_sound":
				base.xui.LoadData(value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
				{
					selectSound = o;
				});
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}
}

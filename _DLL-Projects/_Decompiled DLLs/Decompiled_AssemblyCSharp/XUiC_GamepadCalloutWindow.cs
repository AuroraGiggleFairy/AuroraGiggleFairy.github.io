using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GamepadCalloutWindow : XUiController
{
	public enum CalloutType
	{
		Menu,
		MenuLoot,
		MenuHoverItem,
		MenuHoverAir,
		SelectedOption,
		MenuCategory,
		MenuPaging,
		MenuComboBox,
		MenuShortcuts,
		World,
		Tabs,
		ColorPicker,
		CharacterEditor,
		RWGEditor,
		RWGCamera,
		CharacterPreview,
		CameraZoom,
		Count
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class VisibilityData
	{
		public bool isVisible;

		public float duration;

		public float activeDuration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Callout : MonoBehaviour
	{
		public UIUtils.ButtonIcon icon;

		public string action;

		public static NGUIFont calloutFont;

		public UISprite iconSprite;

		public UILabel actionLabel;

		public CalloutType type;

		public bool bIsVisible = true;

		public bool isFree = true;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public UIAtlas atlasToSet;

		public void Awake()
		{
			bIsVisible = true;
			if (iconSprite == null)
			{
				iconSprite = base.gameObject.AddChild<UISprite>();
			}
			iconSprite.pivot = UIWidget.Pivot.TopLeft;
			iconSprite.height = 35;
			iconSprite.width = 35;
			iconSprite.transform.localPosition = Vector3.zero;
			if (atlasToSet != null)
			{
				iconSprite.atlas = atlasToSet;
				atlasToSet = null;
			}
			iconSprite.fixedAspect = true;
			actionLabel = base.gameObject.AddChild<UILabel>();
			actionLabel.font = calloutFont;
			actionLabel.fontSize = 32;
			actionLabel.pivot = UIWidget.Pivot.TopLeft;
			actionLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			actionLabel.alignment = NGUIText.Alignment.Left;
			actionLabel.transform.localPosition = new Vector2(40f, 0f);
			actionLabel.effectStyle = UILabel.Effect.Outline;
			actionLabel.effectColor = new Color32(0, 0, 0, byte.MaxValue);
			actionLabel.effectDistance = new Vector2(1.5f, 1.5f);
		}

		public void SetupCallout(UIUtils.ButtonIcon _icon, string _action)
		{
			if (iconSprite != null && actionLabel != null)
			{
				icon = _icon;
				action = _action;
				iconSprite.spriteName = UIUtils.GetSpriteName(_icon);
				actionLabel.text = Localization.Get(_action);
			}
		}

		public void SetAtlas(UIAtlas _atlas)
		{
			if (iconSprite != null)
			{
				iconSprite.atlas = _atlas;
			}
			else
			{
				atlasToSet = _atlas;
			}
		}

		public void FreeCallout()
		{
			isFree = true;
			icon = UIUtils.ButtonIcon.Count;
			action = "";
		}

		public void RefreshIcon()
		{
			if (!isFree && icon != UIUtils.ButtonIcon.Count && iconSprite != null)
			{
				iconSprite.spriteName = UIUtils.GetSpriteName(icon);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Y_OFFSET = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int X_OFFSET = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle controllerStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Callout> callouts = new List<Callout>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<CalloutType, List<Callout>> calloutGroups = new EnumDictionary<CalloutType, List<Callout>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public VisibilityData[] typeVisible = new VisibilityData[17];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool localActionsEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject stackObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideCallouts = true;

	public override void Init()
	{
		base.Init();
		IsDormant = false;
		Callout.calloutFont = base.xui.GetUIFontByName("ReferenceFont");
		for (int i = 0; i < 17; i++)
		{
			CalloutType key = (CalloutType)i;
			calloutGroups.Add(key, new List<Callout>());
			typeVisible[i] = new VisibilityData();
		}
		InitWorldCallouts();
		InitContextMenuCallouts();
		InitRWGCallouts();
		InitCharacterPreviewCallouts();
		InitCameraZoomCallouts();
		controllerStyle = PlatformManager.NativePlatform.Input.CurrentControllerInputStyle;
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		for (int i = 0; i < callouts.Count; i++)
		{
			callouts[i].RefreshIcon();
		}
		HideCallouts(base.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWorldCallouts()
	{
		AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoInventory", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoRadialMenu", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoJump", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.DPadUp, "igcoQuickSlot1", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.DPadRight, "igcoQuickSlot2", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.DPadDown, "igcoQuickSlot3", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.DPadLeft, "igcoToggleLight", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoActivate", CalloutType.World);
		AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoReload", CalloutType.World);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitContextMenuCallouts()
	{
		AddCallout(UIUtils.ButtonIcon.RightStickLeftRight, "igcoPageSelection", CalloutType.MenuComboBox);
		AddCallout(UIUtils.ButtonIcon.RightStickLeftRight, "igcoPaging", CalloutType.MenuPaging);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRWGCallouts()
	{
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igco_rwgCameraMode", CalloutType.RWGEditor);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStick, "igco_moveCamera", CalloutType.RWGCamera);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightStick, "igco_pivotCamera", CalloutType.RWGCamera);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igco_rwgCameraSpeed", CalloutType.RWGCamera);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitCharacterPreviewCallouts()
	{
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightStickLeftRight, "igco_rwgRotateCharacterPreview", CalloutType.CharacterPreview);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitCameraZoomCallouts()
	{
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoZoomInCamera", CalloutType.CameraZoom);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoZoomOutCamera", CalloutType.CameraZoom);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = false;
		flag = base.xui.playerUI != null && base.xui.playerUI.playerInput != null && base.xui.playerUI.playerInput.Enabled;
		if (localActionsEnabled != flag)
		{
			localActionsEnabled = flag;
			if (flag)
			{
				DisableCallouts(CalloutType.MenuHoverItem);
			}
		}
		UpdateVisibility(_dt);
		if (stackObject != null && !stackObject.activeInHierarchy)
		{
			ClearCallouts(CalloutType.MenuHoverItem);
			stackObject = null;
		}
		if (IsDirty)
		{
			ResetFreeCallouts();
			ShowCallouts();
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetFreeCallouts()
	{
		for (int i = 0; i < callouts.Count; i++)
		{
			Callout callout = callouts[i];
			if (callout != null && callout.isFree)
			{
				callout.gameObject.SetActive(value: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowCallouts()
	{
		int _currentOffset = 0;
		for (int i = 0; i < 17; i++)
		{
			VisibilityData visibilityData = typeVisible[i];
			if (!hideCallouts)
			{
				ShowCallouts((CalloutType)i, visibilityData.isVisible, ref _currentOffset);
			}
			else
			{
				ShowCallouts((CalloutType)i, _visible: false, ref _currentOffset);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowCallouts(CalloutType _type, bool _visible, ref int _currentOffset)
	{
		List<Callout> list = calloutGroups[_type];
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			Callout callout = list[i];
			bool flag = _visible && callout.bIsVisible;
			if (flag)
			{
				callout.transform.localPosition = new Vector2(0f, _currentOffset);
				_currentOffset -= 5 + callout.iconSprite.height;
			}
			if (callout != null)
			{
				callout.gameObject.SetActive(flag);
			}
		}
	}

	public void AddCallout(UIUtils.ButtonIcon _button, string _action, CalloutType _type)
	{
		if (!ContainsCallout(_button, _action))
		{
			Callout callout = GetCallout(_type);
			callout.SetupCallout(_button, _action);
			calloutGroups[_type]?.Add(callout);
			IsDirty = true;
		}
	}

	public void RemoveCallout(UIUtils.ButtonIcon _button, string _action, CalloutType _type)
	{
		Callout callout = null;
		foreach (Callout callout2 in callouts)
		{
			if (callout2 != null && callout2.icon == _button && callout2.action == _action && callout2.type == _type)
			{
				callout = callout2;
				break;
			}
		}
		if (callout != null)
		{
			callout.FreeCallout();
		}
	}

	public void ShowCallout(UIUtils.ButtonIcon _button, CalloutType _type, bool _visible)
	{
		List<Callout> list = calloutGroups[_type];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && list[i].iconSprite.spriteName == UIUtils.GetSpriteName(_button))
			{
				list[i].bIsVisible = _visible;
				break;
			}
		}
		IsDirty = true;
	}

	public bool ContainsCallout(UIUtils.ButtonIcon _button, string _action)
	{
		foreach (Callout callout in callouts)
		{
			if (callout != null && callout.icon == _button && callout.action == _action)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearCallouts(CalloutType _type)
	{
		if (!calloutGroups.TryGetValue(_type, out var value) || value.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < callouts.Count; i++)
		{
			Callout callout = callouts[i];
			if (callout.type == _type)
			{
				callout.FreeCallout();
			}
		}
		value.Clear();
		IsDirty = true;
	}

	public void SetCalloutsEnabled(CalloutType _type, bool _enabled)
	{
		VisibilityData visibilityData = typeVisible[(int)_type];
		if (visibilityData != null && visibilityData.isVisible != _enabled)
		{
			visibilityData.isVisible = _enabled;
			IsDirty = true;
		}
		if (_enabled && _type != CalloutType.World)
		{
			DisableCallouts(CalloutType.World);
		}
	}

	public void EnableCallouts(CalloutType _type, float _duration = 0f)
	{
		SetCalloutsEnabled(_type, _enabled: true);
		VisibilityData obj = typeVisible[(int)_type];
		obj.activeDuration = 0f;
		obj.duration = _duration;
	}

	public void DisableCallouts(CalloutType _type)
	{
		SetCalloutsEnabled(_type, _enabled: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisibility(float _dt)
	{
		for (int i = 0; i < 17; i++)
		{
			VisibilityData visibilityData = typeVisible[i];
			if (visibilityData.isVisible && visibilityData.duration != 0f)
			{
				visibilityData.activeDuration += Time.unscaledDeltaTime;
				if (visibilityData.activeDuration > visibilityData.duration)
				{
					DisableCallouts((CalloutType)i);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Callout GetCallout(CalloutType _type)
	{
		Callout callout = null;
		bool flag = false;
		for (int i = 0; i < callouts.Count; i++)
		{
			Callout callout2 = callouts[i];
			if (callout2 != null && callout2.isFree)
			{
				callout = callout2;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			callout = viewComponent.UiTransform.gameObject.AddChild<Callout>();
			if (callout.iconSprite == null)
			{
				callout.iconSprite = callout.gameObject.AddChild<UISprite>();
			}
			callouts.Add(callout);
			callout.SetAtlas(UIUtils.IconAtlas);
		}
		callout.type = _type;
		callout.isFree = false;
		callout.bIsVisible = true;
		return callout;
	}

	public void UpdateCalloutsForItemStack(GameObject _stackObject, ItemStack _itemStack, bool _isHovered, bool _canSwap = true, bool _canRemove = true, bool _canPlaceOne = true)
	{
		XUiC_DragAndDropWindow dragAndDrop = base.xui.dragAndDrop;
		ClearCallouts(CalloutType.MenuHoverItem);
		if (_isHovered)
		{
			stackObject = _stackObject;
			if (_itemStack.itemValue.ItemClass != null)
			{
				if (dragAndDrop.CurrentStack.IsEmpty())
				{
					if (_canRemove)
					{
						if (_itemStack.itemValue.ItemClass.CanStack() && _itemStack.count > 1)
						{
							AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoTakeAll", CalloutType.MenuHoverItem);
							AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoTakeHalf", CalloutType.MenuHoverItem);
							ShowCallout(UIUtils.ButtonIcon.FaceButtonSouth, CalloutType.Menu, _visible: false);
						}
						else
						{
							AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoTake", CalloutType.MenuHoverItem);
							ShowCallout(UIUtils.ButtonIcon.FaceButtonSouth, CalloutType.Menu, _visible: false);
						}
						AddCallout(UIUtils.ButtonIcon.RightStick, "igcoQuickMove", CalloutType.MenuHoverItem);
					}
				}
				else if (_itemStack.CanStackWith(dragAndDrop.CurrentStack))
				{
					AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoPlaceAll", CalloutType.MenuHoverItem);
					if (_canPlaceOne)
					{
						AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoPlaceOne", CalloutType.MenuHoverItem);
					}
				}
				else if (_canSwap)
				{
					AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSwap", CalloutType.MenuHoverItem);
				}
			}
			else if (!dragAndDrop.CurrentStack.IsEmpty())
			{
				ItemClass itemClass = dragAndDrop.CurrentStack.itemValue.ItemClass;
				if (itemClass != null && _canSwap)
				{
					if (itemClass.CanStack())
					{
						AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoPlaceAll", CalloutType.MenuHoverItem);
						AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoPlaceOne", CalloutType.MenuHoverItem);
					}
					else
					{
						AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoPlace", CalloutType.MenuHoverItem);
					}
				}
			}
			EnableCallouts(CalloutType.MenuHoverItem);
		}
		else
		{
			stackObject = null;
			DisableCallouts(CalloutType.MenuHoverItem);
			ShowCallout(UIUtils.ButtonIcon.FaceButtonSouth, CalloutType.Menu, _visible: true);
		}
	}

	public void HideCallouts(bool _hideCallouts)
	{
		hideCallouts = _hideCallouts;
		ShowCallouts();
	}
}

using System;
using Audio;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_ItemActionEntry : XUiController
{
	public class TimedAction : MonoBehaviour
	{
		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public BaseItemActionEntry itemActionEntry;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public float waitTime;

		public void InitiateTimer(BaseItemActionEntry itemActionEntry, float _amount)
		{
			this.itemActionEntry = itemActionEntry;
			waitTime = Time.realtimeSinceStartup + _amount;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			if (waitTime != 0f && Time.realtimeSinceStartup >= waitTime)
			{
				waitTime = 0f;
				itemActionEntry.OnTimerCompleted();
				UnityEngine.Object.Destroy(this);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BaseItemActionEntry itemActionEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite icoIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_GamepadIcon gamepadIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label keyboardButton;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 defaultBackgroundColor = Color.gray;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 disabledFontColor = Color.gray;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 defaultFontColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor statuscolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasPressed;

	public BaseItemActionEntry ItemActionEntry
	{
		get
		{
			return itemActionEntry;
		}
		set
		{
			if (itemActionEntry != null)
			{
				itemActionEntry.ParentItem = null;
			}
			itemActionEntry = value;
			background.Enabled = value != null;
			if (itemActionEntry != null)
			{
				PlayerAction playerAction = itemActionEntry.ShortCut switch
				{
					BaseItemActionEntry.GamepadShortCut.DPadUp => base.xui.playerUI.playerInput.GUIActions.DPad_Up, 
					BaseItemActionEntry.GamepadShortCut.DPadLeft => base.xui.playerUI.playerInput.GUIActions.DPad_Left, 
					BaseItemActionEntry.GamepadShortCut.DPadRight => base.xui.playerUI.playerInput.GUIActions.DPad_Right, 
					BaseItemActionEntry.GamepadShortCut.DPadDown => base.xui.playerUI.playerInput.GUIActions.DPad_Down, 
					_ => null, 
				};
				gamepadIcon.SetIconFromPlayerAction(playerAction);
				keyboardButton.Text = playerAction.GetBindingString(_forController: false, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.KeyboardWithAngleBrackets);
				itemActionEntry.ParentItem = this;
				itemActionEntry.RefreshEnabled();
			}
			background.IsNavigatable = itemActionEntry != null;
			UpdateBindingsVisibility();
			RefreshBindings();
		}
	}

	public XUiController Background => background.Controller;

	public XUiV_GamepadIcon GamepadIcon => gamepadIcon;

	public override void Init()
	{
		base.Init();
		lblName = GetChildById("name").ViewComponent as XUiV_Label;
		icoIcon = GetChildById("icon").ViewComponent as XUiV_Sprite;
		background = GetChildById("background").ViewComponent as XUiV_Sprite;
		gamepadIcon = GetChildById("gamepadIcon").ViewComponent as XUiV_GamepadIcon;
		keyboardButton = GetChildById("keyboardButton").ViewComponent as XUiV_Label;
		background.Controller.OnPress += OnPressAction;
		background.Controller.OnHover += OnHover;
		isDirty = true;
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void OnHover(XUiController _sender, bool _isOver)
	{
		XUiV_Sprite xUiV_Sprite = (XUiV_Sprite)_sender.ViewComponent;
		isOver = _isOver;
		if (itemActionEntry == null)
		{
			xUiV_Sprite.Color = defaultBackgroundColor;
			xUiV_Sprite.SpriteName = "menu_empty";
		}
		else if (xUiV_Sprite != null)
		{
			if (_isOver)
			{
				xUiV_Sprite.Color = Color.white;
				xUiV_Sprite.SpriteName = "ui_game_select_row";
			}
			else
			{
				xUiV_Sprite.Color = defaultBackgroundColor;
				xUiV_Sprite.SpriteName = "menu_empty";
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressAction(XUiController _sender, int _mouseButton)
	{
		_ = base.xui.playerUI.entityPlayer;
		if (itemActionEntry != null)
		{
			if (itemActionEntry.Enabled)
			{
				Manager.PlayInsidePlayerHead(itemActionEntry.SoundName);
				itemActionEntry.OnActivated();
			}
			else
			{
				Manager.PlayInsidePlayerHead(itemActionEntry.DisabledSound);
				itemActionEntry.OnDisabledActivate();
			}
			background.Color = defaultBackgroundColor;
			background.SpriteName = "menu_empty";
			wasPressed = true;
		}
	}

	public override void Update(float _dt)
	{
		if (isOver && UICamera.hoveredObject != background.UiTransform.gameObject)
		{
			background.Color = defaultBackgroundColor;
			background.SpriteName = "menu_empty";
			isOver = false;
		}
		if (isOver && wasPressed && itemActionEntry != null)
		{
			background.Color = Color.white;
			background.SpriteName = "ui_game_select_row";
			wasPressed = false;
		}
		if (isDirty)
		{
			RefreshBindings();
			isDirty = false;
		}
		RefreshBindings();
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		UpdateBindingsVisibility();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "actionicon":
			value = ((itemActionEntry != null) ? itemActionEntry.IconName : "");
			return true;
		case "actionname":
			value = ((itemActionEntry != null) ? itemActionEntry.ActionName : "");
			return true;
		case "statuscolor":
			value = "255,255,255,255";
			if (itemActionEntry != null)
			{
				Color32 v = (itemActionEntry.Enabled ? defaultFontColor : disabledFontColor);
				value = statuscolorFormatter.Format(v);
			}
			return true;
		case "inspectheld":
			value = ((itemActionEntry != null && base.xui.playerUI.playerInput.GUIActions.Inspect.IsPressed) ? "true" : "false");
			return true;
		default:
			return false;
		}
	}

	public void StartTimedAction(float time)
	{
		GameManager.Instance.gameObject.AddComponent<TimedAction>().InitiateTimer(itemActionEntry, time);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			switch (name)
			{
			case "default_font_color":
				defaultFontColor = StringParsers.ParseColor32(value);
				break;
			case "disabled_font_color":
				disabledFontColor = StringParsers.ParseColor32(value);
				break;
			case "default_background_color":
				defaultBackgroundColor = StringParsers.ParseColor32(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBindingsVisibility()
	{
		bool flag = base.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard;
		bool flag2 = itemActionEntry != null && itemActionEntry.ShortCut != BaseItemActionEntry.GamepadShortCut.None;
		gamepadIcon.IsVisible = !flag && flag2;
		keyboardButton.IsVisible = flag && flag2;
	}

	public void MarkDirty()
	{
		isDirty = true;
	}
}

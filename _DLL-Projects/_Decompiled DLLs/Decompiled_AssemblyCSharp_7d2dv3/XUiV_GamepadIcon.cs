using System;
using GUI_2;
using InControl;
using Platform;

public class XUiV_GamepadIcon(XUi _xui, string _id) : XUiV_Sprite(_xui, _id)
{
	public enum EActionSet
	{
		Global,
		Local,
		Vehicle,
		Gui,
		Permanent
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle curInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIUtils.ButtonIcon? button;

	[PublicizedFrom(EAccessModifier.Private)]
	public EActionSet actionSet = EActionSet.Gui;

	[PublicizedFrom(EAccessModifier.Private)]
	public string actionName;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction action;

	[XuiXmlAttribute("button", false)]
	public UIUtils.ButtonIcon Button
	{
		get
		{
			return button ?? UIUtils.ButtonIcon.None;
		}
		set
		{
			if (button != value)
			{
				button = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("actionset", false)]
	public EActionSet ActionSet
	{
		get
		{
			return actionSet;
		}
		set
		{
			if (actionSet != value)
			{
				actionSet = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("actionname", false)]
	public string ActionName
	{
		get
		{
			return actionName;
		}
		set
		{
			if (!(actionName == value))
			{
				actionName = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("action", false)]
	public PlayerAction Action
	{
		get
		{
			return action;
		}
		set
		{
			if (action != value)
			{
				action = value;
				SetDirty();
			}
		}
	}

	public override void InitView()
	{
		base.UIAtlas = UIUtils.IconAtlas.Name;
		base.InitView();
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += OnLastInputStyleChanged;
		curInput = PlatformManager.NativePlatform.Input.CurrentInputStyle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		curInput = _style;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (PlatformManager.NativePlatform?.Input != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= OnLastInputStyleChanged;
		}
	}

	public override void Update(float _dt)
	{
		if (curInput != lastInputStyle)
		{
			lastInputStyle = curInput;
			SetDirty();
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.UIAtlas = ((curInput == PlayerInputManager.InputStyle.Keyboard) ? "" : UIUtils.IconAtlas.Name);
		PlayerAction playerAction = null;
		if (!string.IsNullOrEmpty(ActionName))
		{
			playerAction = (ActionSet switch
			{
				EActionSet.Global => PlayerActionsGlobal.Instance, 
				EActionSet.Local => LocalPlayerUI.primaryUI.playerInput, 
				EActionSet.Vehicle => LocalPlayerUI.primaryUI.playerInput.VehicleActions, 
				EActionSet.Gui => LocalPlayerUI.primaryUI.playerInput.GUIActions, 
				EActionSet.Permanent => LocalPlayerUI.primaryUI.playerInput.PermanentActions, 
				_ => throw new ArgumentOutOfRangeException(), 
			}).GetPlayerActionByName(ActionName);
		}
		else if (Action != null)
		{
			playerAction = Action;
		}
		UIUtils.ButtonIcon icon = ((playerAction != null) ? UIUtils.GetButtonIconForAction(playerAction) : Button);
		base.SpriteName = UIUtils.GetSpriteName(icon);
		base.updateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		base.KeepSourceAspectRatio = true;
	}
}

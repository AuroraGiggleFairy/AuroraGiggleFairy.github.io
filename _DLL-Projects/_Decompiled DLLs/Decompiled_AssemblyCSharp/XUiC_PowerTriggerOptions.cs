using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerTriggerOptions : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnOn_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController pnlTargeting;

	[PublicizedFrom(EAccessModifier.Private)]
	public string startTimeText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string endTimeText;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 onColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 offColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetSelf;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetAllies;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetStrangers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetZombies;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredTrigger tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsSelector optionSelector1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsSelector optionSelector2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsSelector optionSelector3;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> delayStrings = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> durationStrings = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public TileEntityPoweredTrigger TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerTriggerWindowGroup Owner
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		optionSelector1 = GetChildById("optionSelector1") as XUiC_OptionsSelector;
		optionSelector2 = GetChildById("optionSelector2") as XUiC_OptionsSelector;
		pnlTargeting = GetChildById("pnlTargeting");
		btnOn = GetChildById("btnOn");
		btnOn_Background = (XUiV_Button)btnOn.GetChildById("clickable").ViewComponent;
		btnOn_Background.Controller.OnPress += btnOn_OnPress;
		btnTargetSelf = GetChildById("btnTargetSelf");
		btnTargetAllies = GetChildById("btnTargetAllies");
		btnTargetStrangers = GetChildById("btnTargetStrangers");
		btnTargetZombies = GetChildById("btnTargetZombies");
		if (btnTargetSelf != null)
		{
			btnTargetSelf.OnPress += btnTargetSelf_OnPress;
		}
		if (btnTargetAllies != null)
		{
			btnTargetAllies.OnPress += btnTargetAllies_OnPress;
		}
		if (btnTargetStrangers != null)
		{
			btnTargetStrangers.OnPress += btnTargetStrangers_OnPress;
		}
		if (btnTargetZombies != null)
		{
			btnTargetZombies.OnPress += btnTargetZombies_OnPress;
		}
		isDirty = true;
		startTimeText = Localization.Get("xuiStartTime");
		endTimeText = Localization.Get("xuiEndTime");
		string format = Localization.Get("goSecond");
		string format2 = Localization.Get("goSeconds");
		string format3 = Localization.Get("goMinute");
		string format4 = Localization.Get("goMinutes");
		delayStrings.Add(Localization.Get("xuiInstant"));
		delayStrings.Add(string.Format(format, 1));
		delayStrings.Add(string.Format(format2, 2));
		delayStrings.Add(string.Format(format2, 3));
		delayStrings.Add(string.Format(format2, 4));
		delayStrings.Add(string.Format(format2, 5));
		durationStrings.Add(Localization.Get("xuiAlways"));
		durationStrings.Add(Localization.Get("xuiTriggered"));
		durationStrings.Add(string.Format(format, 1));
		durationStrings.Add(string.Format(format2, 2));
		durationStrings.Add(string.Format(format2, 3));
		durationStrings.Add(string.Format(format2, 4));
		durationStrings.Add(string.Format(format2, 5));
		durationStrings.Add(string.Format(format2, 6));
		durationStrings.Add(string.Format(format2, 7));
		durationStrings.Add(string.Format(format2, 8));
		durationStrings.Add(string.Format(format2, 9));
		durationStrings.Add(string.Format(format2, 10));
		durationStrings.Add(string.Format(format2, 15));
		durationStrings.Add(string.Format(format2, 30));
		durationStrings.Add(string.Format(format2, 45));
		durationStrings.Add(string.Format(format3, 1));
		durationStrings.Add(string.Format(format4, 5));
		durationStrings.Add(string.Format(format4, 10));
		durationStrings.Add(string.Format(format4, 30));
		durationStrings.Add(string.Format(format4, 60));
		XUiView xUiView = btnTargetAllies.ViewComponent;
		XUiView xUiView2 = btnTargetSelf.ViewComponent;
		XUiView xUiView3 = btnTargetStrangers.ViewComponent;
		XUiView xUiView4 = (btnTargetZombies.ViewComponent.NavDownTarget = optionSelector1.ViewComponent);
		XUiView xUiView6 = (xUiView3.NavDownTarget = xUiView4);
		XUiView navDownTarget = (xUiView2.NavDownTarget = xUiView6);
		xUiView.NavDownTarget = navDownTarget;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetSelf_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button obj = btnTargetSelf.ViewComponent as XUiV_Button;
		obj.Selected = !obj.Selected;
		if (obj.Selected)
		{
			TileEntity.TargetType |= 1;
		}
		else
		{
			TileEntity.TargetType &= -2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetAllies_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button obj = btnTargetAllies.ViewComponent as XUiV_Button;
		obj.Selected = !obj.Selected;
		if (obj.Selected)
		{
			TileEntity.TargetType |= 2;
		}
		else
		{
			TileEntity.TargetType &= -3;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetStrangers_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button obj = btnTargetStrangers.ViewComponent as XUiV_Button;
		obj.Selected = !obj.Selected;
		if (obj.Selected)
		{
			TileEntity.TargetType |= 4;
		}
		else
		{
			TileEntity.TargetType &= -5;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTargetZombies_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiV_Button obj = btnTargetZombies.ViewComponent as XUiV_Button;
		obj.Selected = !obj.Selected;
		if (obj.Selected)
		{
			TileEntity.TargetType |= 8;
		}
		else
		{
			TileEntity.TargetType &= -9;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		TileEntity.ResetTrigger();
	}

	public override void Update(float _dt)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && tileEntity != null)
		{
			if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				btnOn_OnPress(this, 0);
			}
			base.Update(_dt);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		tileEntity.SetUserAccessing(_bUserAccessing: true);
		SetupSliders();
		RefreshBindings();
		tileEntity.SetModified();
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightBumper, "igcoPoweredTriggerReset", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	public override void OnClose()
	{
		GameManager instance = GameManager.Instance;
		Vector3i blockPos = tileEntity.ToWorldPos();
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			tileEntity.SetUserAccessing(_bUserAccessing: false);
			instance.TEUnlockServer(tileEntity.GetClrIdx(), blockPos, tileEntity.entityId);
			tileEntity.SetModified();
		}
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupSliders()
	{
		if (pnlTargeting != null)
		{
			pnlTargeting.ViewComponent.IsVisible = tileEntity.TriggerType == PowerTrigger.TriggerTypes.Motion;
		}
		switch (tileEntity.TriggerType)
		{
		case PowerTrigger.TriggerTypes.TimerRelay:
		{
			optionSelector1.Title = startTimeText;
			optionSelector1.ClearItems();
			for (int k = 0; k < 48; k++)
			{
				int num = k / 2;
				bool flag = k % 2 == 1;
				optionSelector1.AddItem(num.ToString("00") + (flag ? ":30" : ":00"));
			}
			optionSelector1.OnSelectionChanged -= OptionSelector1_OnSelectionChanged;
			optionSelector1.OnSelectionChanged += OptionSelector1_OnSelectionChanged;
			optionSelector2.Title = endTimeText;
			optionSelector2.ClearItems();
			for (int l = 0; l < 48; l++)
			{
				int num2 = l / 2;
				bool flag2 = l % 2 == 1;
				optionSelector2.AddItem(num2.ToString("00") + (flag2 ? ":30" : ":00"));
			}
			optionSelector2.OnSelectionChanged -= OptionSelector2_OnSelectionChanged;
			optionSelector2.OnSelectionChanged += OptionSelector2_OnSelectionChanged;
			optionSelector1.SetIndex(TileEntity.Property1);
			optionSelector2.SetIndex(TileEntity.Property2);
			if (btnOn != null)
			{
				btnOn.ViewComponent.IsVisible = false;
			}
			break;
		}
		case PowerTrigger.TriggerTypes.PressurePlate:
		case PowerTrigger.TriggerTypes.Motion:
		case PowerTrigger.TriggerTypes.TripWire:
		{
			optionSelector1.Title = Localization.Get("xuiPowerDelay");
			optionSelector1.ClearItems();
			for (int i = 0; i < delayStrings.Count; i++)
			{
				optionSelector1.AddItem(delayStrings[i]);
			}
			optionSelector2.Title = Localization.Get("xuiPowerDuration");
			optionSelector2.ClearItems();
			for (int j = 0; j < durationStrings.Count; j++)
			{
				optionSelector2.AddItem(durationStrings[j]);
			}
			optionSelector1.OnSelectionChanged -= OptionSelector1_OnSelectionChanged;
			optionSelector2.OnSelectionChanged -= OptionSelector2_OnSelectionChanged;
			optionSelector1.SetIndex(TileEntity.Property1);
			optionSelector2.SetIndex(TileEntity.Property2);
			optionSelector2.OnSelectionChanged += OptionSelector2_OnSelectionChanged;
			optionSelector1.OnSelectionChanged += OptionSelector1_OnSelectionChanged;
			if (btnOn != null)
			{
				btnOn.ViewComponent.IsVisible = true;
			}
			if (pnlTargeting == null)
			{
				break;
			}
			pnlTargeting.ViewComponent.IsVisible = tileEntity.TriggerType == PowerTrigger.TriggerTypes.Motion;
			if (TileEntity.TriggerType == PowerTrigger.TriggerTypes.Motion)
			{
				if (btnTargetSelf != null)
				{
					btnTargetSelf.OnPress -= btnTargetSelf_OnPress;
					((XUiV_Button)btnTargetSelf.ViewComponent).Selected = TileEntity.TargetSelf;
					btnTargetSelf.OnPress += btnTargetSelf_OnPress;
				}
				if (btnTargetAllies != null)
				{
					btnTargetAllies.OnPress -= btnTargetAllies_OnPress;
					((XUiV_Button)btnTargetAllies.ViewComponent).Selected = TileEntity.TargetAllies;
					btnTargetAllies.OnPress += btnTargetAllies_OnPress;
				}
				if (btnTargetStrangers != null)
				{
					btnTargetStrangers.OnPress -= btnTargetStrangers_OnPress;
					((XUiV_Button)btnTargetStrangers.ViewComponent).Selected = TileEntity.TargetStrangers;
					btnTargetStrangers.OnPress += btnTargetStrangers_OnPress;
				}
				if (btnTargetZombies != null)
				{
					btnTargetZombies.OnPress -= btnTargetZombies_OnPress;
					((XUiV_Button)btnTargetZombies.ViewComponent).Selected = TileEntity.TargetZombies;
					btnTargetZombies.OnPress += btnTargetZombies_OnPress;
				}
			}
			break;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OptionSelector1_OnSelectionChanged(XUiController _sender, int newSelectedIndex)
	{
		TileEntity.Property1 = (byte)newSelectedIndex;
		TileEntity.ResetTrigger();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OptionSelector2_OnSelectionChanged(XUiController _sender, int newSelectedIndex)
	{
		TileEntity.Property2 = (byte)newSelectedIndex;
		TileEntity.ResetTrigger();
	}
}

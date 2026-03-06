using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToolbeltWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lmpPositionAdjustment = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDeficitValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string standardXPColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string updatingXPColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string expDeficitColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float xpFillSpeed = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public CachedStringFormatterFloat bindingXp = new CachedStringFormatterFloat();

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("btnClearInventory1");
		if (childById != null)
		{
			childById.OnPress += BtnClearInventory1_OnPress;
		}
		childById = GetChildById("btnClearInventory2");
		if (childById != null)
		{
			childById.OnPress += BtnClearInventory2_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory1_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.entityPlayer.EmptyToolbelt(0, 10);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory2_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.entityPlayer.EmptyToolbelt(10, 20);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		deltaTime = _dt;
		if ((DateTime.Now - updateTime).TotalSeconds > 0.5)
		{
			updateTime = DateTime.Now;
		}
		RefreshBindings();
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		base.ViewComponent.IsVisible = (!(localPlayer.AttachedToEntity != null) || !(localPlayer.AttachedToEntity is EntityVehicle)) && !localPlayer.IsDead() && (windowManager.IsHUDEnabled() || (base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden()));
		if (CustomAttributes.ContainsKey("standard_xp_color"))
		{
			standardXPColor = CustomAttributes["standard_xp_color"];
		}
		else
		{
			standardXPColor = "128,4,128";
		}
		if (CustomAttributes.ContainsKey("updating_xp_color"))
		{
			updatingXPColor = CustomAttributes["updating_xp_color"];
		}
		else
		{
			updatingXPColor = "128,4,128";
		}
		if (CustomAttributes.ContainsKey("deficit_xp_color"))
		{
			expDeficitColor = CustomAttributes["deficit_xp_color"];
		}
		else
		{
			expDeficitColor = "222,20,20";
		}
		if (CustomAttributes.ContainsKey("xp_fill_speed"))
		{
			xpFillSpeed = StringParsers.ParseFloat(CustomAttributes["xp_fill_speed"]);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (localPlayer == null)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		currentValue = (lastValue = XUiM_Player.GetLevelPercent(localPlayer));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "xp":
			if (localPlayer != null)
			{
				if (localPlayer.Progression.ExpDeficit > 0)
				{
					float v = Math.Max(lastDeficitValue, 0f) * 1.01f;
					value = bindingXp.Format(v);
					currentValue = (float)localPlayer.Progression.ExpDeficit / (float)localPlayer.Progression.GetExpForNextLevel();
					if (currentValue != lastDeficitValue)
					{
						lastDeficitValue = Mathf.Lerp(lastDeficitValue, currentValue, Time.deltaTime * xpFillSpeed);
						if (Mathf.Abs(currentValue - lastDeficitValue) < 0.005f)
						{
							lastDeficitValue = currentValue;
						}
					}
				}
				else
				{
					float v2 = Math.Max(lastValue, 0f) * 1.01f;
					value = bindingXp.Format(v2);
					currentValue = XUiM_Player.GetLevelPercent(localPlayer);
					if (currentValue != lastValue)
					{
						lastValue = Mathf.Lerp(lastValue, currentValue, Time.deltaTime * xpFillSpeed);
						if (Mathf.Abs(currentValue - lastValue) < 0.005f)
						{
							lastValue = currentValue;
						}
					}
				}
			}
			return true;
		case "xpcolor":
			if (localPlayer != null)
			{
				if (localPlayer.Progression.ExpDeficit > 0)
				{
					value = expDeficitColor;
				}
				else
				{
					value = ((currentValue == lastValue) ? standardXPColor : updatingXPColor);
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "creativewindowopen":
			value = base.xui.playerUI.windowManager.IsWindowOpen("creative").ToString();
			return true;
		default:
			return false;
		}
	}
}

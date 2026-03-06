using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerSourceWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceStats GeneratorStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceSlots PowerSourceSlots;

	public static string ID = "powersource";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource tileEntity;

	public TileEntityPowerSource TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			GeneratorStats.TileEntity = tileEntity;
			PowerSourceSlots.TileEntity = tileEntity;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childByType = GetChildByType<XUiC_WindowNonPagingHeader>();
		if (childByType != null)
		{
			nonPagingHeader = (XUiC_WindowNonPagingHeader)childByType;
		}
		childByType = GetChildByType<XUiC_PowerSourceStats>();
		if (childByType != null)
		{
			GeneratorStats = (XUiC_PowerSourceStats)childByType;
			GeneratorStats.Owner = this;
		}
		childByType = GetChildByType<XUiC_PowerSourceSlots>();
		if (childByType != null)
		{
			PowerSourceSlots = (XUiC_PowerSourceSlots)childByType;
			PowerSourceSlots.Owner = this;
		}
	}

	public void SetOn(bool isOn)
	{
		if (PowerSourceSlots != null && PowerSourceSlots.ViewComponent.IsVisible)
		{
			PowerSourceSlots.SetOn(isOn);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!windowGroup.isShowing)
		{
			return;
		}
		if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			wasReleased = true;
		}
		if (!wasReleased)
		{
			return;
		}
		if (base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			activeKeyDown = true;
		}
		if (base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
		{
			activeKeyDown = false;
			if (!base.xui.playerUI.windowManager.IsInputActive())
			{
				base.xui.playerUI.windowManager.CloseAllOpenWindows();
			}
		}
	}

	public override bool AlwaysUpdate()
	{
		return false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (nonPagingHeader != null)
		{
			string header = "";
			_ = base.xui.playerUI.entityPlayer;
			switch (TileEntity.PowerItemType)
			{
			case PowerItem.PowerItemTypes.BatteryBank:
				header = Localization.Get("batterybank");
				break;
			case PowerItem.PowerItemTypes.Generator:
				header = Localization.Get("generatorbank");
				break;
			case PowerItem.PowerItemTypes.SolarPanel:
				header = Localization.Get("solarbank");
				break;
			}
			nonPagingHeader.SetHeader(header);
		}
		base.xui.RecenterWindowGroup(windowGroup);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		if (PowerSourceSlots != null && TileEntity != null)
		{
			PowerSourceSlots.OnOpen();
		}
		Manager.BroadcastPlayByLocalPlayer(TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "open_vending");
		IsDirty = true;
		TileEntity.Destroyed += TileEntity_Destroyed;
	}

	public override void OnClose()
	{
		base.OnClose();
		wasReleased = false;
		activeKeyDown = false;
		Manager.BroadcastPlayByLocalPlayer(TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "close_vending");
		TileEntity.Destroyed -= TileEntity_Destroyed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_Destroyed(ITileEntity te)
	{
		if (TileEntity == te)
		{
			if (GameManager.Instance != null)
			{
				base.xui.playerUI.windowManager.Close("powersource");
			}
		}
		else
		{
			te.Destroyed -= TileEntity_Destroyed;
		}
	}
}

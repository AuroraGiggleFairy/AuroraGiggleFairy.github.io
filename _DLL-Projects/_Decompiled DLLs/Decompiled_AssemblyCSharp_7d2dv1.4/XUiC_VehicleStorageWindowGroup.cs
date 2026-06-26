using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleStorageWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_VehicleContainer containerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "vehicleStorage";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle currentVehicleEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	public EntityVehicle CurrentVehicleEntity
	{
		get
		{
			return currentVehicleEntity;
		}
		set
		{
			base.xui.vehicle = value;
			currentVehicleEntity = value;
			containerWindow.SetSlots(value.bag.GetSlots());
		}
	}

	public override void Init()
	{
		base.Init();
		containerWindow = GetChildByType<XUiC_VehicleContainer>();
		nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public override void Update(float _dt)
	{
		if (windowGroup.isShowing)
		{
			if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				wasReleased = true;
			}
			if (wasReleased)
			{
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
		}
		if (currentVehicleEntity != null && !currentVehicleEntity.CheckUIInteraction())
		{
			base.xui.playerUI.windowManager.Close(ID);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		_ = base.xui.playerUI.windowManager;
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(Localization.Get("xuiStorage"));
		}
		ITileEntityLootable lootContainer = CurrentVehicleEntity.lootContainer;
		if (lootContainer == null)
		{
			return;
		}
		LootContainer lootContainer2 = LootContainer.GetLootContainer(lootContainer.lootListName);
		if (lootContainer2 == null || lootContainer2.soundClose == null)
		{
			return;
		}
		Vector3 position = lootContainer.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
		if (lootContainer.EntityId != -1 && GameManager.Instance.World != null)
		{
			Entity entity = GameManager.Instance.World.GetEntity(lootContainer.EntityId);
			if (entity != null)
			{
				position = entity.GetPosition();
			}
		}
		Manager.BroadcastPlayByLocalPlayer(position, lootContainer2.soundOpen);
	}

	public override void OnClose()
	{
		base.OnClose();
		wasReleased = false;
		activeKeyDown = false;
		_ = base.xui.playerUI.windowManager;
		CurrentVehicleEntity.StopUIInteraction();
		base.xui.vehicle = null;
		ITileEntityLootable lootContainer = CurrentVehicleEntity.lootContainer;
		if (lootContainer == null)
		{
			return;
		}
		LootContainer lootContainer2 = LootContainer.GetLootContainer(lootContainer.lootListName);
		if (lootContainer2 == null || lootContainer2.soundClose == null)
		{
			return;
		}
		Vector3 position = lootContainer.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
		if (lootContainer.EntityId != -1 && GameManager.Instance.World != null)
		{
			Entity entity = GameManager.Instance.World.GetEntity(lootContainer.EntityId);
			if (entity != null)
			{
				position = entity.GetPosition();
			}
		}
		Manager.BroadcastPlayByLocalPlayer(position, lootContainer2.soundClose);
	}
}

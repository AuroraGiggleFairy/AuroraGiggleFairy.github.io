using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerRangedTrapWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerRangedTrapOptions optionsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerRangedAmmoSlots ammoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CameraWindow cameraWindowPreview;

	public static string ID = "powerrangedtrap";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredRangedTrap tileEntity;

	public TileEntityPoweredRangedTrap TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			ammoWindow.TileEntity = tileEntity;
			if (tileEntity.PowerItemType == PowerItem.PowerItemTypes.RangedTrap)
			{
				optionsWindow.TileEntity = tileEntity;
			}
			cameraWindowPreview.TileEntity = tileEntity;
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
		childByType = GetChildByType<XUiC_PowerRangedAmmoSlots>();
		if (childByType != null)
		{
			ammoWindow = (XUiC_PowerRangedAmmoSlots)childByType;
			ammoWindow.Owner = this;
		}
		childByType = GetChildByType<XUiC_PowerRangedTrapOptions>();
		if (childByType != null)
		{
			optionsWindow = (XUiC_PowerRangedTrapOptions)childByType;
			optionsWindow.Owner = this;
		}
		childByType = GetChildById("windowPowerCameraControlPreview");
		if (childByType != null)
		{
			cameraWindowPreview = (XUiC_CameraWindow)childByType;
			cameraWindowPreview.Owner = this;
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
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (nonPagingHeader != null)
		{
			string text = "";
			_ = GameManager.Instance.World;
			text = tileEntity.GetChunk().GetBlock(tileEntity.localChunkPos).Block.GetLocalizedBlockName();
			nonPagingHeader.SetHeader(text);
		}
		base.xui.RecenterWindowGroup(windowGroup);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		if (!tileEntity.ShowTargeting)
		{
			optionsWindow.ViewComponent.IsVisible = false;
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
		Vector3 position = TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			Manager.BroadcastPlayByLocalPlayer(position, "close_vending");
		}
		TileEntity.Destroyed -= TileEntity_Destroyed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_Destroyed(ITileEntity te)
	{
		if (TileEntity == te)
		{
			if (GameManager.Instance != null)
			{
				base.xui.playerUI.windowManager.Close("powerrangedtrap");
				base.xui.playerUI.windowManager.Close("powercamera");
			}
		}
		else
		{
			te.Destroyed -= TileEntity_Destroyed;
		}
	}
}

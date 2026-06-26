using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerCameraWindowGroup : XUiController
{
	public bool UseEdgeDetection = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CameraWindow cameraWindow;

	public static string ID = "powercamera";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered tileEntity;

	public TileEntityPowered TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			cameraWindow.TileEntity = tileEntity;
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
		childByType = GetChildByType<XUiC_CameraWindow>();
		if (childByType != null)
		{
			cameraWindow = (XUiC_CameraWindow)childByType;
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
		IsDirty = true;
		base.xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		bool flag = true;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer.IsDead() || Vector3.Distance(entityPlayer.position, TileEntity.ToWorldPos().ToVector3()) > Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance)
		{
			flag = false;
		}
		wasReleased = false;
		activeKeyDown = false;
		if (flag && base.xui.playerUI.windowManager.HasWindow(XUiC_CameraWindow.lastWindowGroup))
		{
			if (XUiC_CameraWindow.lastWindowGroup == "powerrangedtrap")
			{
				((XUiC_PowerRangedTrapWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_CameraWindow.lastWindowGroup)).Controller).TileEntity = (TileEntityPoweredRangedTrap)TileEntity;
			}
			else if (XUiC_CameraWindow.lastWindowGroup == "powertrigger")
			{
				((XUiC_PowerTriggerWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_CameraWindow.lastWindowGroup)).Controller).TileEntity = (TileEntityPoweredTrigger)TileEntity;
			}
			else
			{
				((XUiC_PoweredGenericWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_CameraWindow.lastWindowGroup)).Controller).TileEntity = TileEntity;
			}
			base.xui.playerUI.windowManager.Open(XUiC_CameraWindow.lastWindowGroup, _bModal: true);
		}
		base.xui.playerUI.CursorController.Locked = false;
	}
}

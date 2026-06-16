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
		if (!xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			wasReleased = true;
		}
		if (!wasReleased)
		{
			return;
		}
		if (xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			activeKeyDown = true;
		}
		if (xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
		{
			activeKeyDown = false;
			if (!xui.playerUI.windowManager.IsInputActive())
			{
				xui.playerUI.windowManager.CloseAllOpenModalWindows();
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (nonPagingHeader != null)
		{
			string text = "";
			_ = GameManager.Instance.World;
			text = tileEntity.GetChunk().GetBlock(tileEntity.localChunkPos).Block.GetLocalizedBlockName();
			nonPagingHeader.SetHeader(text);
		}
		IsDirty = true;
		xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		bool flag = true;
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		if (entityPlayer.IsDead() || Vector3.Distance(entityPlayer.position, TileEntity.ToWorldPos().ToVector3()) > Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance)
		{
			flag = false;
		}
		wasReleased = false;
		activeKeyDown = false;
		if (flag && xui.playerUI.windowManager.TryGetWindow(XUiC_CameraWindow.lastWindowGroup, out var _window))
		{
			XUiController controller = ((XUiWindowGroup)_window).Controller;
			if (XUiC_CameraWindow.lastWindowGroup == "powerrangedtrap")
			{
				((XUiC_PowerRangedTrapWindowGroup)controller).TileEntity = (TileEntityPoweredRangedTrap)TileEntity;
			}
			else if (XUiC_CameraWindow.lastWindowGroup == "powertrigger")
			{
				((XUiC_PowerTriggerWindowGroup)controller).TileEntity = (TileEntityPoweredTrigger)TileEntity;
			}
			else
			{
				((XUiC_PoweredGenericWindowGroup)controller).TileEntity = TileEntity;
			}
			xui.playerUI.windowManager.Open(XUiC_CameraWindow.lastWindowGroup, _bModal: true);
		}
		xui.playerUI.CursorController.Locked = false;
	}
}

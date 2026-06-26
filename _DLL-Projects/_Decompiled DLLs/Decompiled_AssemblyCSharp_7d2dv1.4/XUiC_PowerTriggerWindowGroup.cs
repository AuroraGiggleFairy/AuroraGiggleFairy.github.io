using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerTriggerWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerTriggerOptions triggerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CameraWindow cameraWindow;

	public static string ID = "powertrigger";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredTrigger tileEntity;

	public TileEntityPoweredTrigger TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			triggerWindow.TileEntity = tileEntity;
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
		childByType = GetChildByType<XUiC_PowerTriggerOptions>();
		if (childByType != null)
		{
			triggerWindow = (XUiC_PowerTriggerOptions)childByType;
			triggerWindow.Owner = this;
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
			text = Localization.Get("xuiTrigger");
			nonPagingHeader.SetHeader(text);
		}
		base.xui.RecenterWindowGroup(windowGroup);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
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
				base.xui.playerUI.windowManager.Close("powertrigger");
				base.xui.playerUI.windowManager.Close("powercamera");
			}
		}
		else
		{
			te.Destroyed -= TileEntity_Destroyed;
		}
	}
}

using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoweredGenericWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public TileEntityPowered tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	public TileEntityPowered TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			setupWindowTileEntities();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setupWindowTileEntities()
	{
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
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		wasReleased = false;
		activeKeyDown = false;
		if (tileEntity != null && !XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			LockManager.Instance.UnlockRequestLocal();
			tileEntity = null;
		}
	}
}

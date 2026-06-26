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
		base.xui.RecenterWindowGroup(windowGroup);
		for (int i = 0; i < children.Count; i++)
		{
			children[i].OnOpen();
		}
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		wasReleased = false;
		activeKeyDown = false;
		if (tileEntity != null && !XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			GameManager.Instance.TEUnlockServer(tileEntity.GetClrIdx(), tileEntity.ToWorldPos(), tileEntity.entityId);
			tileEntity = null;
		}
	}
}

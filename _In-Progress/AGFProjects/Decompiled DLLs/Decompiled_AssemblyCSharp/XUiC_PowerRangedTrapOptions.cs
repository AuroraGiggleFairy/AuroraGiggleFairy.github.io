using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerRangedTrapOptions : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController pnlTargeting;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetSelf;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetAllies;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetStrangers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTargetZombies;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredRangedTrap tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public TileEntityPoweredRangedTrap TileEntity
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
	public XUiC_PowerRangedTrapWindowGroup Owner
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		pnlTargeting = GetChildById("pnlTargeting");
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

	public override void Update(float _dt)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && tileEntity != null)
		{
			base.Update(_dt);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		tileEntity.SetUserAccessing(_bUserAccessing: true);
		SetupTargeting();
		RefreshBindings();
		tileEntity.SetModified();
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
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTargeting()
	{
		if (pnlTargeting != null)
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
	}

	public Vector3i GetBlockPos()
	{
		if (TileEntity != null)
		{
			return TileEntity.ToWorldPos();
		}
		return Vector3i.zero;
	}
}

using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerRangedAmmoSlots : XUiC_ItemStackGrid
{
	public static XUiC_PowerRangedAmmoSlots Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnOn_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 onColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 offColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOn;

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
			SetSlots(tileEntity.ItemSlots);
		}
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerRangedTrapWindowGroup Owner { get; set; }

	public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		base.HandleSlotChangedEvent(slotNumber, stack);
		tileEntity.SetSendSlots();
		tileEntity.SetModified();
	}

	public override void Init()
	{
		base.Init();
		btnOn = windowGroup.Controller.GetChildById("windowPowerTrapSlots").GetChildById("btnOn");
		btnOn_Background = (XUiV_Button)btnOn.GetChildById("clickable").ViewComponent;
		btnOn_Background.Controller.OnPress += btnOn_OnPress;
		XUiController childById = btnOn.GetChildById("lblOnOff");
		if (childById != null)
		{
			lblOnOff = (XUiV_Label)childById.ViewComponent;
		}
		childById = btnOn.GetChildById("sprOnOff");
		if (childById != null)
		{
			sprOnOff = (XUiV_Sprite)childById.ViewComponent;
		}
		isDirty = true;
		turnOff = Localization.Get("xuiUnlockAmmo");
		turnOn = Localization.Get("xuiLockAmmo");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		bool flag = false;
		for (int i = 0; i < TileEntity.ItemSlots.Length; i++)
		{
			if (!TileEntity.ItemSlots[i].IsEmpty())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttRequiresAmmo"));
		}
		else
		{
			tileEntity.IsLocked = !tileEntity.IsLocked;
			tileEntity.SetModified();
			RefreshIsLocked(tileEntity.IsLocked);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRequirements()
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.RequiredType = XUiC_RequiredItemStack.RequiredTypes.ItemClass;
				xUiC_RequiredItemStack.RequiredItemClass = tileEntity.AmmoItem;
			}
		}
	}

	public virtual void SetSlots(ItemStack[] stacks)
	{
		items = stacks;
		base.SetStacks(stacks);
	}

	public virtual bool HasRequirement(Recipe recipe)
	{
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		tileEntity.SetUserAccessing(_bUserAccessing: true);
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		IsDirty = true;
		SetRequirements();
		bool isLocked = tileEntity.IsLocked;
		RefreshIsLocked(isLocked);
		XUiC_PowerRangedAmmoSlots current = (base.xui.powerAmmoSlots = this);
		Current = current;
		IsDormant = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager instance = GameManager.Instance;
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		Vector3i blockPos = tileEntity.ToWorldPos();
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			tileEntity.SetUserAccessing(_bUserAccessing: false);
			instance.TEUnlockServer(tileEntity.GetClrIdx(), blockPos, tileEntity.entityId);
			tileEntity.SetModified();
		}
		IsDirty = true;
		tileEntity = null;
		XUiC_PowerRangedAmmoSlots current = (base.xui.powerAmmoSlots = null);
		Current = current;
		IsDormant = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIsLocked(bool isOn)
	{
		if (isOn)
		{
			lblOnOff.Text = turnOff;
			if (sprOnOff != null)
			{
				sprOnOff.Color = onColor;
				sprOnOff.SpriteName = "ui_game_symbol_lock";
			}
		}
		else
		{
			lblOnOff.Text = turnOn;
			if (sprOnOff != null)
			{
				sprOnOff.Color = offColor;
				sprOnOff.SpriteName = "ui_game_symbol_unlock";
			}
		}
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.ToolLock = isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetLocked(bool isOn)
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.ToolLock = isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		tileEntity.ItemSlots = stackList;
		tileEntity.SetSendSlots();
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Update(float _dt)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && tileEntity != null)
		{
			base.Update(_dt);
			if (lastLocked != tileEntity.IsLocked)
			{
				lastLocked = tileEntity.IsLocked;
				RefreshIsLocked(tileEntity.IsLocked);
			}
			if (tileEntity.IsLocked)
			{
				SetSlots(tileEntity.ItemSlots);
			}
			RefreshBindings();
		}
	}

	public void Refresh()
	{
		SetSlots(tileEntity.ItemSlots);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (itemClass != tileEntity.AmmoItem)
		{
			return false;
		}
		tileEntity.TryStackItem(itemStack);
		SetSlots(tileEntity.ItemSlots);
		return itemStack.count == 0;
	}
}

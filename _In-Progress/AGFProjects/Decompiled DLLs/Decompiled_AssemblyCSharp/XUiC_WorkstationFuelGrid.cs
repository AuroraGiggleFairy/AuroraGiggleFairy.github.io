using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationFuelGrid : XUiC_WorkstationGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController button;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController onOffLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController flameIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 flameOnColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 flameOffColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public float normalizedDt;

	public event XuiEvent_WorkstationItemsChanged OnWorkstationFuelChanged;

	public override void Init()
	{
		base.Init();
		flameIcon = windowGroup.Controller.GetChildById("flameIcon");
		button = windowGroup.Controller.GetChildById("button");
		button.OnPress += Button_OnPress;
		onOffLabel = windowGroup.Controller.GetChildById("onoff");
		items = new ItemStack[itemControllers.Length];
		turnOff = Localization.Get("xuiTurnOff");
		turnOn = Localization.Get("xuiTurnOn");
	}

	public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		base.HandleSlotChangedEvent(slotNumber, stack);
		((XUiV_Button)button.ViewComponent).Enabled = workstationData.GetBurnTimeLeft() > 0f || hasAnyFuel();
		onFuelItemsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onFuelItemsChanged()
	{
		if (this.OnWorkstationFuelChanged != null)
		{
			this.OnWorkstationFuelChanged();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (workstationData == null)
		{
			return;
		}
		XUiC_ItemStack xUiC_ItemStack = itemControllers[0];
		if (xUiC_ItemStack == null)
		{
			return;
		}
		if (!hasFuelStack() && hasAnyFuel())
		{
			for (int i = 0; i < itemControllers.Length - 1; i++)
			{
				CycleStacks();
				if (hasFuelStack())
				{
					UpdateBackend(getUISlots());
					break;
				}
			}
		}
		if (isOn && (!HasRequirement(null) || workstationData.GetIsBesideWater()))
		{
			TurnOff();
			onFuelItemsChanged();
			return;
		}
		if (isOn && workstationData != null && xUiC_ItemStack.ItemStack != null)
		{
			if (!xUiC_ItemStack.ItemStack.IsEmpty())
			{
				if (xUiC_ItemStack.IsLocked)
				{
					xUiC_ItemStack.LockTime = workstationData.GetBurnTimeLeft();
				}
				else
				{
					xUiC_ItemStack.LockStack(XUiC_ItemStack.LockTypes.Burning, workstationData.GetBurnTimeLeft(), 0, null);
				}
			}
			else
			{
				xUiC_ItemStack.UnlockStack();
			}
		}
		if (xUiC_ItemStack != null && (workstationData == null || xUiC_ItemStack.ItemStack == null || xUiC_ItemStack.ItemStack.IsEmpty() || !isOn))
		{
			xUiC_ItemStack.UnlockStack();
		}
		if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			Button_OnPress(null, 0);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isOn = workstationData.GetIsBurning();
		((XUiV_Label)onOffLabel.ViewComponent).Text = (isOn ? turnOff : turnOn);
		if (flameIcon != null)
		{
			((XUiV_Sprite)flameIcon.ViewComponent).Color = (isOn ? flameOnColor : flameOffColor);
		}
		base.xui.currentWorkstationFuelGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentWorkstationFuelGrid = null;
	}

	public void TurnOn()
	{
		if (!isOn)
		{
			Manager.PlayInsidePlayerHead("forge_burn_fuel");
		}
		isOn = true;
		workstationData.SetIsBurning(isOn);
		((XUiV_Label)onOffLabel.ViewComponent).Text = turnOff;
		if (flameIcon != null)
		{
			((XUiV_Sprite)flameIcon.ViewComponent).Color = flameOnColor;
		}
	}

	public void TurnOff()
	{
		if (isOn)
		{
			Manager.PlayInsidePlayerHead("forge_fire_die");
		}
		isOn = false;
		workstationData.SetIsBurning(isOn);
		((XUiV_Label)onOffLabel.ViewComponent).Text = turnOn;
		itemControllers[0]?.UnlockStack();
		if (flameIcon != null)
		{
			((XUiV_Sprite)flameIcon.ViewComponent).Color = flameOffColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAnyFuel()
	{
		int num = 0;
		if (!XUi.IsGameRunning())
		{
			return false;
		}
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			if (xUiC_ItemStack == null || xUiC_ItemStack.ItemStack.IsEmpty())
			{
				continue;
			}
			ItemClass itemClass = ItemClass.list[xUiC_ItemStack.ItemStack.itemValue.type];
			if (itemClass == null)
			{
				continue;
			}
			if (!itemClass.IsBlock())
			{
				if (itemClass != null && itemClass.FuelValue != null)
				{
					num += itemClass.FuelValue.Value;
				}
				continue;
			}
			Block block = Block.list[itemClass.Id];
			if (block != null)
			{
				num += block.FuelValue;
			}
		}
		return num > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasFuelStack()
	{
		XUiC_ItemStack xUiC_ItemStack = itemControllers[0];
		if (!XUi.IsGameRunning() || xUiC_ItemStack == null || xUiC_ItemStack.ItemStack.IsEmpty())
		{
			return false;
		}
		int num = 0;
		ItemClass itemClass = ItemClass.list[xUiC_ItemStack.ItemStack.itemValue.type];
		if (itemClass == null)
		{
			return false;
		}
		if (!itemClass.IsBlock())
		{
			if (itemClass != null && itemClass.FuelValue != null)
			{
				num = itemClass.FuelValue.Value;
			}
		}
		else
		{
			Block block = Block.list[itemClass.Id];
			if (block == null)
			{
				return false;
			}
			num = block.FuelValue;
		}
		return num > 0;
	}

	public override bool HasRequirement(Recipe recipe)
	{
		XUiC_ItemStack xUiC_ItemStack = itemControllers[0];
		if (!XUi.IsGameRunning() || xUiC_ItemStack == null || xUiC_ItemStack.ItemStack.IsEmpty())
		{
			return workstationData.GetBurnTimeLeft() > 0f;
		}
		int num = 0;
		ItemClass itemClass = ItemClass.list[xUiC_ItemStack.ItemStack.itemValue.type];
		if (itemClass == null)
		{
			return workstationData.GetBurnTimeLeft() > 0f;
		}
		if (!itemClass.IsBlock())
		{
			if (itemClass != null && itemClass.FuelValue != null)
			{
				num = itemClass.FuelValue.Value;
			}
		}
		else
		{
			Block block = Block.list[itemClass.Id];
			if (block == null)
			{
				return workstationData.GetBurnTimeLeft() > 0f;
			}
			num = block.FuelValue;
		}
		return num > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CycleStacks()
	{
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			if (xUiC_ItemStack != null && xUiC_ItemStack.ItemStack.count <= 0 && i + 1 < itemControllers.Length)
			{
				XUiC_ItemStack xUiC_ItemStack2 = itemControllers[i + 1];
				if (xUiC_ItemStack2 != null)
				{
					xUiC_ItemStack.ItemStack = xUiC_ItemStack2.ItemStack.Clone();
					xUiC_ItemStack2.ItemStack = ItemStack.Empty.Clone();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Button_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!isOn && (hasAnyFuel() || workstationData.GetBurnTimeLeft() > 0f) && !workstationData.GetIsBesideWater())
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		workstationData.SetFuelStacks(stackList);
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public override bool AddItem(ItemClass _itemClass, ItemStack _itemStack)
	{
		int startIndex = (isOn ? 1 : 0);
		TryStackItem(startIndex, _itemStack);
		if (_itemStack.count > 0 && AddItem(_itemStack))
		{
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "flameoncolor"))
			{
				if (!(name == "flameoffcolor"))
				{
					return false;
				}
				flameOffColor = StringParsers.ParseColor32(value);
			}
			else
			{
				flameOnColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}
}

using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ContainerStandardControls : XUiController
{
	public delegate bool MoveAllowedDelegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _sourceGrid, out IInventory _destinationInventory);

	public Func<PackedBoolArray> GetLockedSlotsFromStorage;

	public Action<PackedBoolArray> SetLockedSlotsToStorage;

	public Action<PackedBoolArray> ApplyLockedSlotStates;

	public Action<XUiC_ContainerStandardControls> UpdateLockedSlotStates;

	public Action<PackedBoolArray> SortPressed;

	public Action LockModeToggled;

	public MoveAllowedDelegate MoveAllowed;

	public Action<bool, bool> MoveAllDone;

	public bool MoveStartBottomRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LockModeEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenColor lockModeButtonColorTweener;

	public PackedBoolArray LockedSlots
	{
		get
		{
			return GetLockedSlotsFromStorage?.Invoke();
		}
		set
		{
			SetLockedSlotsToStorage?.Invoke(value);
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("btnSort");
		if (childById != null)
		{
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				Sort();
			};
		}
		childById = GetChildById("btnMoveAll");
		if (childById != null)
		{
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				MoveAll();
			};
		}
		childById = GetChildById("btnMoveFillAndSmart");
		if (childById != null)
		{
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				MoveFillAndSmart();
			};
		}
		childById = GetChildById("btnMoveFillStacks");
		if (childById != null)
		{
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				MoveFillStacks();
			};
		}
		childById = GetChildById("btnMoveSmart");
		if (childById != null)
		{
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				MoveSmart();
			};
		}
		childById = GetChildById("btnToggleLockMode");
		if (childById != null)
		{
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				ToggleLockMode();
			};
			if (childById.ViewComponent is XUiV_Button xUiV_Button)
			{
				lockModeButtonColorTweener = xUiV_Button.UiTransform.gameObject.GetOrAddComponent<TweenColor>();
				lockModeButtonColorTweener.from = xUiV_Button.DefaultSpriteColor;
				lockModeButtonColorTweener.to = xUiV_Button.SelectedSpriteColor;
				lockModeButtonColorTweener.style = UITweener.Style.PingPong;
				lockModeButtonColorTweener.enabled = false;
				lockModeButtonColorTweener.duration = 0.4f;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ApplyLockedSlotStates?.Invoke(LockedSlots);
	}

	public void Sort()
	{
		UpdateLockedSlotStates?.Invoke(this);
		SortPressed?.Invoke(LockedSlots);
	}

	public void MoveAll()
	{
		UpdateLockedSlotStates?.Invoke(this);
		if (MoveAllowed(out var _parentWindow, out var _sourceGrid, out var _destinationInventory))
		{
			var (arg, arg2) = XUiM_LootContainer.StashItems(_parentWindow, _sourceGrid, _destinationInventory, 0, LockedSlots, XUiM_LootContainer.EItemMoveKind.All, MoveStartBottomRight);
			MoveAllDone?.Invoke(arg, arg2);
		}
	}

	public void MoveFillAndSmart()
	{
		UpdateLockedSlotStates?.Invoke(this);
		if (MoveAllowed(out var _parentWindow, out var _sourceGrid, out var _destinationInventory))
		{
			XUiM_LootContainer.StashItems(_parentWindow, _sourceGrid, _destinationInventory, 0, LockedSlots, XUiM_LootContainer.EItemMoveKind.FillOnlyFirstCreateSecond, MoveStartBottomRight);
		}
	}

	public void MoveFillStacks()
	{
		UpdateLockedSlotStates?.Invoke(this);
		if (MoveAllowed(out var _parentWindow, out var _sourceGrid, out var _destinationInventory))
		{
			XUiM_LootContainer.StashItems(_parentWindow, _sourceGrid, _destinationInventory, 0, LockedSlots, XUiM_LootContainer.EItemMoveKind.FillOnly, MoveStartBottomRight);
		}
	}

	public void MoveSmart()
	{
		UpdateLockedSlotStates?.Invoke(this);
		if (MoveAllowed(out var _parentWindow, out var _sourceGrid, out var _destinationInventory))
		{
			XUiM_LootContainer.StashItems(_parentWindow, _sourceGrid, _destinationInventory, 0, LockedSlots, XUiM_LootContainer.EItemMoveKind.FillAndCreate, MoveStartBottomRight);
		}
	}

	public void ToggleLockMode()
	{
		UpdateLockedSlotStates?.Invoke(this);
		LockModeToggled?.Invoke();
	}

	public void LockModeChanged(bool _state)
	{
		if (lockModeButtonColorTweener != null)
		{
			lockModeButtonColorTweener.enabled = _state;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "move_start_bottom_left")
		{
			MoveStartBottomRight = StringParsers.ParseBool(_value);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}
}

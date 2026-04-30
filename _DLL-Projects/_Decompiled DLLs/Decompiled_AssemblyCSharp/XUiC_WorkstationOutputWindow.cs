using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationOutputWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationOutputGrid outputGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls controls;

	public override void Init()
	{
		base.Init();
		outputGrid = GetChildByType<XUiC_WorkstationOutputGrid>();
		controls = GetChildByType<XUiC_ContainerStandardControls>();
		if (controls == null)
		{
			return;
		}
		controls.SortPressed = [PublicizedFrom(EAccessModifier.Private)] (PackedBoolArray _ignoredSlots) =>
		{
			ItemStack[] slots = StackSortUtil.CombineAndSortStacks(outputGrid.GetSlots(), 0, _ignoredSlots);
			outputGrid.SetSlots(slots);
		};
		controls.MoveAllowed = [PublicizedFrom(EAccessModifier.Private)] (out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory) =>
		{
			_parentWindow = this;
			_grid = outputGrid;
			_inventory = base.xui.PlayerInventory;
			return true;
		};
		controls.MoveAllDone = [PublicizedFrom(EAccessModifier.Private)] (bool _allMoved, bool _anyMoved) =>
		{
			if (_anyMoved)
			{
				Manager.BroadcastPlayByLocalPlayer(base.xui.playerUI.entityPlayer.position + Vector3.one * 0.5f, "UseActions/takeall1");
			}
		};
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!base.xui.playerUI.windowManager.IsInputActive() && (base.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
		{
			controls.MoveAll();
			windowGroup.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
	}
}

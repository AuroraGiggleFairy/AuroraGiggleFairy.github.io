using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdWorkstationMaterials : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "wsmats", "workstationmaterials" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("This command requires exactly two parameters!");
			return;
		}
		int _result;
		if (_params[0].EqualsCaseInsensitive("all"))
		{
			_result = -1;
		}
		else if (!StringParsers.TryParseSInt32(_params[0], out _result) || _result < -1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Parameter <slot> is not a valid positive integer or the keyword 'all'!");
			return;
		}
		if (!StringParsers.TryParseSInt32(_params[1], out var _result2) || _result2 < 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Parameter <count> is not a valid positive integer!");
			return;
		}
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("This command has to be run from a game client!");
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("This command has to be run from a game client!");
			return;
		}
		foreach (XUiC_WorkstationWindowGroup item in primaryPlayer.PlayerUI.xui.GetWindowsByType<XUiC_WorkstationWindowGroup>())
		{
			if (!item.IsOpen)
			{
				continue;
			}
			XUiM_Workstation workstationData = item.WorkstationData;
			if (workstationData == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Open workstation window has no associated data!");
				return;
			}
			TileEntityWorkstation tileEntity = workstationData.TileEntity;
			ItemStack[] input = tileEntity.Input;
			int inputSlotCount = tileEntity.InputSlotCount;
			int num = input.Length - inputSlotCount;
			if (_result == -1)
			{
				for (int i = 0; i < num; i++)
				{
					setSlot(i, _result2, num, inputSlotCount, input, tileEntity.MaterialNames);
				}
			}
			else
			{
				setSlot(_result, _result2, num, inputSlotCount, input, tileEntity.MaterialNames);
			}
			tileEntity.Input = input;
			item.TileEntity_InputChanged();
			item.SetAllChildrenDirty();
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No open workstation window found!");
		float num2 = 0f;
		if (_params.Count > 0)
		{
			num2 = StringParsers.ParseFloat(_params[0]);
		}
		primaryPlayer.Stats.Stamina.MaxModifier = 0f - primaryPlayer.Stats.Stamina.Max + (float)Mathf.CeilToInt(num2 / 100f * primaryPlayer.Stats.Stamina.Max);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSlot(int _targetSlot, int _count, int _storedSlotCount, int _smeltSlotCount, ItemStack[] _inputStacks, string[] _materialNames)
	{
		if (_targetSlot >= _storedSlotCount)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Parameter <slot> is larger than the amount of materials the currently open workstation knows ({_storedSlotCount})!");
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass("unit_" + _materialNames[_targetSlot]);
		if (itemClass == null || itemClass.MadeOfMaterial.ForgeCategory == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Parameter <slot> specifies a slot that has no workstation materials!");
			return;
		}
		int num = _smeltSlotCount + _targetSlot;
		if (_inputStacks[num].itemValue.type == 0)
		{
			_inputStacks[num] = new ItemStack(new ItemValue(itemClass.Id), _inputStacks[num].count);
		}
		if (_count > itemClass.Stacknumber.Value)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Parameter <count> is larger than the max stack size for the material!");
		}
		else
		{
			_inputStacks[num].count = _count;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Set material counts on workstations.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set the material count for the given material slot on the currently opened workstation\nto the given value.\nUsage:\n   wsmats <slot> <count>\nSlot specifies the index of the material slot in the workstation, e.g. 0 on forge is iron, or \"all\"\nto change all slots at once.Count defines to what you want to set the material count with a maximum of 30000.\n";
	}
}

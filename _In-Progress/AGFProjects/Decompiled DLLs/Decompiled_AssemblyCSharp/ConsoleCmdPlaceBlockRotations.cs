using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPlaceBlockRotations : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Places all rotations of the currently held block";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Places the block you currently hold in your hand in all supported rotations. Starts\nat the current selection box and spreads out towards the right relative to the\ncurrent view direction of the player. Spaces out each block by 1m meter.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "placeblockrotations", "pbr" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients");
			return;
		}
		if (!BlockToolSelection.Instance.SelectionActive)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No selection active. Running this command requires an active 1x1x1 selection box.");
			return;
		}
		Vector3i selectionSize = BlockToolSelection.Instance.SelectionSize;
		Vector3i selectionStart = BlockToolSelection.Instance.SelectionStart;
		if (selectionSize != Vector3i.one)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Selection box size is not 1x1x1.");
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		BlockValue blockValue = primaryPlayer.inventory.holdingItemItemValue.ToBlockValue();
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = primaryPlayer.inventory.holdingItemData as ItemClassBlock.ItemBlockInventoryData;
		if (blockValue.isair || itemBlockInventoryData == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Player is not holding a block.");
			return;
		}
		float y = primaryPlayer.rotation.y;
		Vector3i zero = Vector3i.zero;
		switch (GameUtils.GetClosestDirection(y, _limitTo90Degress: true))
		{
		case GameUtils.DirEightWay.N:
			zero.x = 2;
			break;
		case GameUtils.DirEightWay.E:
			zero.z = -2;
			break;
		case GameUtils.DirEightWay.S:
			zero.x = -2;
			break;
		case GameUtils.DirEightWay.W:
			zero.z = 2;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Vector3i placementPos = selectionStart;
		blockValue.rotation = 0;
		do
		{
			PlaceBlock(blockValue, itemBlockInventoryData, placementPos, primaryPlayer);
			int _localRot = 0;
			blockValue.rotation = blockValue.Block.BlockPlacementHelper.LimitRotation(BlockPlacement.EnumRotationMode.Advanced, ref _localRot, default(HitInfoDetails), _bAdd: true, blockValue, blockValue.rotation);
			placementPos += zero;
		}
		while (blockValue.rotation != 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PlaceBlock(BlockValue _blockValue, ItemClassBlock.ItemBlockInventoryData _holdingData, Vector3i _placementPos, EntityPlayerLocal _player)
	{
		BlockPlacement.Result _bpResult = new BlockPlacement.Result
		{
			clrIdx = 0,
			blockValue = _blockValue,
			blockPos = _placementPos
		};
		Block block = _blockValue.Block;
		block.OnBlockPlaceBefore(GameManager.Instance.World, ref _bpResult, _player, GameManager.Instance.World.GetGameRandom());
		_blockValue = _bpResult.blockValue;
		if (_holdingData.itemValue.TextureFullArray.IsDefault || Block.list[_holdingData.itemValue.type].SelectAlternates)
		{
			block.PlaceBlock(GameManager.Instance.World, _bpResult, _player);
			return;
		}
		BlockChangeInfo item = new BlockChangeInfo(0, _placementPos, _blockValue)
		{
			textureFull = _holdingData.itemValue.TextureFullArray,
			bChangeTexture = true
		};
		GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo> { item });
	}
}

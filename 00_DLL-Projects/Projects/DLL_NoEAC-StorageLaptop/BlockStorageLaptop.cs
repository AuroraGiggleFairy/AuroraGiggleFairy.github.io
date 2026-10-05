using UnityEngine.Scripting;

namespace StorageLaptop
{
	[Preserve]
	public class BlockStorageLaptop : BlockPowered
	{
		private readonly BlockActivationCommand[] commands = new BlockActivationCommand[2]
		{
			new BlockActivationCommand("open", "search", _enabled: true),
			new BlockActivationCommand("take", "hand", _enabled: false)
		};

		public override TileEntityPowered CreateTileEntity(Chunk chunk)
		{
			return new TileEntityPoweredBlock(chunk)
			{
				PowerItemType = PowerItem.PowerItemTypes.Consumer
			};
		}

		public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			return true;
		}

		public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			bool canTake = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()) && TakeDelay > 0f;
			commands[0].enabled = true;
			commands[1].enabled = canTake;
			return commands;
		}

		public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
		{
			EntityPlayerLocal player = _entityFocusing as EntityPlayerLocal;
			if (player != null)
			{
				LocalPlayerUI ui = LocalPlayerUI.GetUIForPlayer(player);
				if (ui != null && ui.windowManager.IsWindowOpen("storageLaptop"))
				{
					XUiC_FocusedBlockHealth.SetData(ui, null, 0f);
					return null;
				}
			}

			string name = _blockValue.Block.GetLocalizedBlockName();
			if (!IsPoweredAt(_world, _blockPos))
			{
				return name + "\n" + Localization.Get("agfStorageLaptopNeedPower");
			}

			return name + "\n" + Localization.Get("useWorkstation");
		}

		public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
		{
			if (_blockValue.ischild)
			{
				Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
				BlockValue parent = _world.GetBlock(parentPos);
				return OnBlockActivated(_commandName, _world, parentPos, parent, _player);
			}

			if (_commandName == "open")
			{
				if (!IsPoweredAt(_world, _blockPos))
				{
					GameManager.ShowTooltip(_player, Localization.Get("agfStorageLaptopNeedPower"));
					return true;
				}

				StorageLaptopSession.Pos = _blockPos;
				StorageLaptopWindow.RequestOpen();
				return true;
			}

			if (_commandName == "take")
			{
				takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
				return true;
			}

			return false;
		}

		public static bool IsPoweredAt(WorldBase world, Vector3i pos)
		{
			return world?.GetTileEntity(pos) is TileEntityPowered powered && powered.IsPowered;
		}
	}

	public static class StorageLaptopSession
	{
		public static Vector3i Pos;
	}
}

using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockDewCollector : Block
{
	public enum ModEffectTypes
	{
		Type,
		Speed,
		Count
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropConvertToItem = "ConvertToItem";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModdedConvertToItem = "ModdedConvertToItem";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinTime = "MinConvertTime";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxTime = "MaxConvertTime";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModdedSpeed = "ModdedConvertSpeed";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModdedCount = "ModdedConvertCount";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOpenSound = "OpenSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCloseSound = "CloseSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropConvertSound = "ConvertSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTakeDelay = "TakeDelay";

	public string ConvertToItem;

	public string ModdedConvertToItem;

	public float MinConvertTime = 21600f;

	public float MaxConvertTime = 43200f;

	public float ModdedConvertSpeed = 0.5f;

	public int ModdedConvertCount = 2;

	public string OpenSound;

	public string CloseSound;

	public string ConvertSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modTransformNames;

	public ModEffectTypes[] ModTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("Search", "search", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public BlockDewCollector()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseString(PropConvertToItem, ref ConvertToItem);
		base.Properties.ParseString(PropModdedConvertToItem, ref ModdedConvertToItem);
		base.Properties.ParseFloat(PropMinTime, ref MinConvertTime);
		base.Properties.ParseFloat(PropMaxTime, ref MaxConvertTime);
		base.Properties.ParseFloat(PropModdedSpeed, ref ModdedConvertSpeed);
		base.Properties.ParseInt(PropModdedCount, ref ModdedConvertCount);
		base.Properties.ParseString(PropOpenSound, ref OpenSound);
		base.Properties.ParseString(PropCloseSound, ref CloseSound);
		base.Properties.ParseString(PropConvertSound, ref ConvertSound);
		base.Properties.ParseFloat(PropTakeDelay, ref TakeDelay);
		string optionalValue = "1,2,3";
		base.Properties.ParseString("ModTransformNames", ref optionalValue);
		modTransformNames = optionalValue.Split(',');
		optionalValue = "Count,Speed,Type";
		base.Properties.ParseString("ModTypes", ref optionalValue);
		string[] array = optionalValue.Split(',');
		ModTypes = new ModEffectTypes[array.Length];
		for (int i = 0; i < array.Length && i < ModTypes.Length; i++)
		{
			ModTypes[i] = Enum.Parse<ModEffectTypes>(array[i]);
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityDewCollector tileEntityDewCollector && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityDewCollector.worldTimeTouched = _world.GetWorldTime();
			tileEntityDewCollector.SetEmpty();
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityDewCollector tileEntityDewCollector))
		{
			return string.Empty;
		}
		string arg = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg2 = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if (tileEntityDewCollector.IsWaterEmpty())
		{
			return string.Format(Localization.Get("dewCollectorEmpty"), arg2, arg);
		}
		if (tileEntityDewCollector.IsModdedConvertItem)
		{
			return string.Format(Localization.Get("dewCollectorHasWater"), arg2, arg);
		}
		return string.Format(Localization.Get("dewCollectorHasDirtyWater"), arg2, arg);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (!_blockValue.ischild)
		{
			addTileEntity(world, _chunk, _blockPos, _blockValue);
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityDewCollector tileEntityDewCollector)
		{
			tileEntityDewCollector.OnDestroy();
		}
		removeTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		TileEntityDewCollector tileEntityDewCollector = new TileEntityDewCollector(_chunk);
		tileEntityDewCollector.localChunkPos = World.toBlock(_blockPos);
		tileEntityDewCollector.SetWorldTime();
		_chunk.AddTileEntity(tileEntityDewCollector);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void removeTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		_chunk.RemoveTileEntityAt<TileEntityDewCollector>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityDewCollector tileEntityDewCollector)
		{
			tileEntityDewCollector.OnDestroy();
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(_blockPos);
		}
		return DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_player.inventory.IsHoldingItemActionRunning())
		{
			return false;
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityDewCollector tileEntityDewCollector))
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityDewCollector.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityDewCollector.entityId, _player.entityId);
		return true;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "Search"))
		{
			if (_commandName == "take")
			{
				TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
				return true;
			}
			return false;
		}
		return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		if (!(GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityDewCollector).IsEmpty())
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttWorkstationNotEmpty"), string.Empty, "ui_denied");
			return;
		}
		LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
		playerUI.windowManager.Open("timer", _bModal: true);
		XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.Data = new object[4] { _cIdx, _blockValue, _blockPos, _player };
		timerEventData.Event += EventData_Event;
		childByType.SetTimer(TakeDelay, timerEventData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] obj = (object[])timerData.Data;
		int clrIdx = (int)obj[0];
		BlockValue blockValue = (BlockValue)obj[1];
		Vector3i vector3i = (Vector3i)obj[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = obj[3] as EntityPlayerLocal;
		if (block.damage > 0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup"), string.Empty, "ui_denied");
			return;
		}
		if ((world.GetTileEntity(clrIdx, vector3i) as TileEntityDewCollector).IsUserAccessing())
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttCantPickupInUse"), string.Empty, "ui_denied");
			return;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
		if (!uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			uIForPlayer.xui.PlayerInventory.DropItem(itemStack);
		}
		world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
	}

	public void UpdateVisible(TileEntityDewCollector _te)
	{
		if ((bool)_te.GetChunk().GetBlockEntity(_te.ToWorldPos()).transform)
		{
			_ = _te.ModSlots;
		}
	}
}

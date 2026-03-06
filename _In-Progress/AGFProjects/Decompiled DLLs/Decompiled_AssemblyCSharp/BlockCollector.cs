using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockCollector : Block
{
	public enum ModEffectTypes
	{
		Type,
		Speed,
		Count,
		Modify
	}

	public class FuelData
	{
		public string FuelName;

		public int FuelCost;

		public FuelData(string name, int cost)
		{
			FuelName = name;
			FuelCost = cost;
		}
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemIconBackdrop = "ItemIconBackdrop";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootLabelKey = "LootLabelKey";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivationEvent = "ActivationEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCloseEvent = "CloseEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRequiredMods = "RequiredMods";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRequiredModsOnly = "RequiredModsOnly";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTypes = "FuelTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTransformEnableNames = "ModTransformEnableNames";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTransformDisableNames = "ModTransformDisableNames";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTypes = "ModTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemBackgroundColor = "ItemBackgroundColor";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCollectorEmptyText = "CollectorEmptyText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCollectorHasItem1Text = "CollectorHasItem1Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCollectorHasItem2Text = "CollectorHasItem2Text";

	public string ConvertToItem;

	public string ModdedConvertToItem;

	public float MinConvertTime = 21600f;

	public float MaxConvertTime = 43200f;

	public float ModdedConvertSpeed = 0.5f;

	public int ModdedConvertCount = 2;

	public string OpenSound;

	public string CloseSound;

	public string ConvertSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string itemIconBackdrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public string itemBackgroundColor = "255,255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootLabelKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activationEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string closeEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] requiredMods;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool requiredModsOnly;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modTransformEnableNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modTransformDisableNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string collectorEmptyText = "CollectorEmptyText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string collectorHasItem1Text = "CollectorHasItem1Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string collectorHasItem2Text = "CollectorHasItem2Text";

	public ModEffectTypes[] ModTypes;

	public FuelData[] FuelTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("Search", "search", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public string ItemIconBackdrop => itemIconBackdrop;

	public string ItemBackgroundColor => itemBackgroundColor;

	public string LootLabelKey => lootLabelKey;

	public string ActivationEvent => activationEvent;

	public string CloseEvent => closeEvent;

	public string[] RequiredMods => requiredMods;

	public bool RequiredModsOnly => requiredModsOnly;

	public string[] ModTransformEnableNames => modTransformEnableNames;

	public string[] ModTransformDisableNames => modTransformDisableNames;

	public BlockCollector()
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
		base.Properties.ParseString(PropItemIconBackdrop, ref itemIconBackdrop);
		base.Properties.ParseString(PropLootLabelKey, ref lootLabelKey);
		base.Properties.ParseString(PropActivationEvent, ref activationEvent);
		base.Properties.ParseString(PropCloseEvent, ref closeEvent);
		base.Properties.ParseString(PropItemBackgroundColor, ref itemBackgroundColor);
		string optionalValue = "";
		requiredMods = null;
		base.Properties.ParseString(PropRequiredMods, ref optionalValue);
		if (!string.IsNullOrEmpty(optionalValue))
		{
			requiredMods = optionalValue.Split(',');
		}
		base.Properties.ParseBool(PropRequiredModsOnly, ref requiredModsOnly);
		string optionalValue2 = ",,";
		base.Properties.ParseString(PropModTransformEnableNames, ref optionalValue2);
		modTransformEnableNames = optionalValue2.Split(',');
		optionalValue2 = ",,";
		base.Properties.ParseString(PropModTransformDisableNames, ref optionalValue2);
		modTransformDisableNames = optionalValue2.Split(',');
		optionalValue2 = "Count,Speed,Type";
		base.Properties.ParseString(PropModTypes, ref optionalValue2);
		string[] array = optionalValue2.Split(',');
		ModTypes = new ModEffectTypes[array.Length];
		for (int i = 0; i < array.Length && i < ModTypes.Length; i++)
		{
			ModTypes[i] = Enum.Parse<ModEffectTypes>(array[i]);
		}
		string optionalValue3 = null;
		base.Properties.ParseString(PropFuelTypes, ref optionalValue3);
		if (optionalValue3 != null)
		{
			string[] array2 = optionalValue3.Split(',');
			int num = array2.Length / 2;
			FuelTypes = new FuelData[num];
			for (int j = 0; j < array2.Length; j += 2)
			{
				int result = 0;
				int.TryParse(array2[j + 1], out result);
				FuelTypes[j / 2] = new FuelData(array2[j], result);
			}
		}
		base.Properties.ParseString(PropCollectorEmptyText, ref collectorEmptyText);
		base.Properties.ParseString(PropCollectorHasItem1Text, ref collectorHasItem1Text);
		base.Properties.ParseString(PropCollectorHasItem2Text, ref collectorHasItem2Text);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild)
		{
			addTileEntity(world, _chunk, _blockPos, _blockValue);
		}
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

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.clrIdx, _result.blockPos) is TileEntityCollector tileEntityCollector && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityCollector.worldTimeTouched = _world.GetWorldTime();
			tileEntityCollector.SetEmpty();
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityCollector tileEntityCollector))
		{
			return string.Empty;
		}
		string arg = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg2 = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if (tileEntityCollector.IsWaterEmpty())
		{
			return string.Format(Localization.Get(collectorEmptyText), arg2, arg);
		}
		if (tileEntityCollector.IsModdedConvertItem)
		{
			return string.Format(Localization.Get(collectorHasItem2Text), arg2, arg);
		}
		return string.Format(Localization.Get(collectorHasItem1Text), arg2, arg);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityCollector tileEntityCollector)
		{
			tileEntityCollector.OnDestroy();
		}
		removeTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		TileEntityCollector tileEntityCollector = new TileEntityCollector(_chunk);
		tileEntityCollector.localChunkPos = World.toBlock(_blockPos);
		tileEntityCollector.SetWorldTime();
		_chunk.AddTileEntity(tileEntityCollector);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void removeTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		_chunk.RemoveTileEntityAt<TileEntityCollector>((World)world, World.toBlock(_blockPos));
	}

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityCollector tileEntityCollector)
		{
			tileEntityCollector.OnDestroy();
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(_blockPos);
		}
		return DestroyedResult.Downgrade;
	}

	public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _blockValue, _ebcd);
		UpdateVisible(_world, _blockPos);
	}

	public void UpdateVisible(WorldBase _world, Vector3i _blockPos)
	{
		if (_world.GetTileEntity(_blockPos) is TileEntityCollector tileEntityCollector)
		{
			tileEntityCollector.UpdateVisible();
		}
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_player.inventory.IsHoldingItemActionRunning())
		{
			return false;
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityCollector tileEntityCollector))
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityCollector.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityCollector.entityId, _player.entityId);
		return true;
	}

	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return;
		}
		if (!(GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityCollector).IsEmpty())
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
		if ((world.GetTileEntity(clrIdx, vector3i) as TileEntityCollector).IsUserAccessing())
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

	public bool NeedsFuel()
	{
		if (FuelTypes != null)
		{
			return FuelTypes.Length != 0;
		}
		return false;
	}
}

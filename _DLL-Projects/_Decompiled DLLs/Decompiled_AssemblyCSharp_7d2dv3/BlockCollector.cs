using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

[Preserve]
public class BlockCollector : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum CollectorTypes
	{
		DewCollector,
		Apiary,
		ChickenCoop
	}

	public class CatalystConvert
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public string convertFrom;

		[PublicizedFrom(EAccessModifier.Private)]
		public string convertTo;

		public CatalystConvert(string _convertFrom, string _convertTo)
		{
			convertFrom = _convertFrom;
			convertTo = _convertTo;
		}

		public ItemStack Convert(ItemStack _inStack)
		{
			ItemStack result = null;
			if (!_inStack.IsEmpty() && _inStack.itemValue.ItemClass.Name == convertFrom)
			{
				result = new ItemStack(ItemClass.GetItem(convertTo), _inStack.count);
			}
			return result;
		}
	}

	public class FuelType
	{
		public string Name;

		public string[] Items;

		public FuelType(string ftDef)
		{
			string[] array = ftDef.Split(',');
			Name = array[0];
			Items = array.Skip(1).ToArray();
			for (int i = 0; i < Items.Length; i++)
			{
				Items[i] = Items[i].Trim();
			}
		}
	}

	public class OutputType
	{
		public string Name;

		public string Fuel;

		public int FuelCost;

		public int AdditionalFuelCost;

		public int DiscountedFuelDivisor;

		public string OutputItem;

		public string OutputItemModded;

		public int ModdedConvertSpeedMultiplier;

		public int ModdedConvertCountMultiplier;

		public int MinConvertTime;

		public int MaxConvertTime;

		public string ConvertSound;

		public OutputType(string otDef)
		{
			string[] array = otDef.Split(",");
			int num = 0;
			Name = array[num++];
			Fuel = array[num++];
			FuelCost = safeParseInt(array[num++]);
			AdditionalFuelCost = safeParseInt(array[num++]);
			DiscountedFuelDivisor = safeParseInt(array[num++]);
			OutputItem = array[num++];
			OutputItemModded = array[num++];
			ModdedConvertSpeedMultiplier = safeParseInt(array[num++]);
			ModdedConvertCountMultiplier = safeParseInt(array[num++]);
			MinConvertTime = safeParseInt(array[num++]);
			MaxConvertTime = safeParseInt(array[num++]);
			ConvertSound = array[num++];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int safeParseInt(string intString)
		{
			if (!int.TryParse(intString, out var result))
			{
				return 0;
			}
			return result;
		}
	}

	public enum ModEffectTypes
	{
		Type,
		Speed,
		Count,
		Modify,
		Expand,
		Cost
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCollectorType = "CollectorType";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTypes = "FuelTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOutputTypes = "OutputTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOutputs = "Outputs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOpenSound = "OpenSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRunningSound = "RunningSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivateSound = "ActivateSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCloseSound = "CloseSound";

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
	public static string PropModTransformEnableNames = "ModTransformEnableNames";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTransformDisableNames = "ModTransformDisableNames";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTypes = "ModTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemBackgroundColor = "ItemBackgroundColor";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEmptyText = "EmptyText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHasItem1Text = "HasItem1Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHasItem2Text = "HasItem2Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOutputGridHeight = "OutputGridHeight";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelGridHeight = "FuelGridHeight";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTitleText = "FuelTitleText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTitleSprite = "FuelTitleSprite";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTitleSpriteAtlas = "FuelTitleSpriteAtlas";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTypesSprites = "FuelTypesSprites";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystGridHeight = "CatalystGridHeight";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTypes = "CatalystTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystMultiplier = "CatalystMultiplier";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystRequirements = "CatalystRequirements";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystConvert = "CatalystConvert";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTitleText = "CatalystTitleText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTitleSprite = "CatalystTitleSprite";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTitleSpriteAtlas = "CatalystTitleSpriteAtlas";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTypesSprites = "CatalystTypesSprites";

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] outputs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public CollectorTypes CollectorType;

	public Dictionary<OutputType, List<int>> OrderedSlotOutputs = new Dictionary<OutputType, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, FuelType> fuelTypes = new Dictionary<string, FuelType>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, OutputType> outputTypes = new Dictionary<string, OutputType>();

	public string OpenSound;

	public string CloseSound;

	public string RunningSound;

	public string ActivateSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] itemIconBackdrop;

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
	public string emptyText = "CollectorEmptyText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string hasItem1Text = "CollectorHasItem1Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string hasItem2Text = "CollectorHasItem2Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fuelGridHeight = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int catalystGridHeight = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int outputGridInitialHeight = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fuelTitleText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fuelTitleSprite = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fuelTitleSpriteAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string catalystTitleText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string catalystTitleSprite = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string catalystTitleSpriteAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] fuelTypesSprites = new string[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] fuelTypesSpriteAtlases = new string[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] catalystTypesSprites = new string[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] catalystTypesSpriteAtlases = new string[0];

	public ModEffectTypes[] ModTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] catalystTypes = new string[0];

	public Dictionary<string, int> CatalystMultipliers = new Dictionary<string, int>();

	public Dictionary<string, int> CatalystRequirements = new Dictionary<string, int>();

	public CatalystConvert[] CatalystConverts = new CatalystConvert[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("Search", "search", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public string[] Outputs => outputs;

	public string[] ItemIconBackdrop => itemIconBackdrop;

	public string ItemBackgroundColor => itemBackgroundColor;

	public string LootLabelKey => lootLabelKey;

	public string ActivationEvent => activationEvent;

	public string CloseEvent => closeEvent;

	public string[] RequiredMods => requiredMods;

	public bool RequiredModsOnly => requiredModsOnly;

	public string[] ModTransformEnableNames => modTransformEnableNames;

	public string[] ModTransformDisableNames => modTransformDisableNames;

	public int FuelGridInitialHeight => fuelGridHeight;

	public int CatalystGridInitialHeight => catalystGridHeight;

	public int OutputGridInitialHeight => outputGridInitialHeight;

	public string FuelTitleText => fuelTitleText;

	public string FuelTitleSprite => fuelTitleSprite;

	public string FuelTitleSpriteAtlas => fuelTitleSpriteAtlas;

	public string CatalystTitleText => catalystTitleText;

	public string CatalystTitleSprite => catalystTitleSprite;

	public string CatalystTitleSpriteAtlas => catalystTitleSpriteAtlas;

	public string[] FuelTypesSprites => fuelTypesSprites;

	public string[] FuelTypesSpriteAtlases => fuelTypesSpriteAtlases;

	public string[] CatalystTypesSprites => catalystTypesSprites;

	public string[] CatalystTypesSpriteAtlases => catalystTypesSpriteAtlases;

	public string[] CatalystTypes => catalystTypes;

	public FuelType GetFuelType(string name)
	{
		if (!fuelTypes.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public OutputType GetOutputType(string name)
	{
		if (!outputTypes.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public BlockCollector()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		string optionalValue = "";
		base.Properties.ParseString(PropCollectorType, ref optionalValue);
		if (optionalValue != "")
		{
			Enum.TryParse<CollectorTypes>(optionalValue, out CollectorType);
		}
		string optionalValue2 = null;
		base.Properties.ParseString(PropFuelTypes, ref optionalValue2);
		string[] array = optionalValue2.TrimEnd('}').Split('{', 125, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			FuelType fuelType = new FuelType(array[i]);
			fuelTypes[fuelType.Name] = fuelType;
		}
		string optionalValue3 = null;
		base.Properties.ParseString(PropOutputTypes, ref optionalValue3);
		string[] array2 = optionalValue3.Split('{', 125, StringSplitOptions.RemoveEmptyEntries);
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = array2[j].TrimEnd('}');
			OutputType outputType = new OutputType(array2[j]);
			outputTypes.Add(outputType.Name, outputType);
		}
		string optionalValue4 = null;
		base.Properties.ParseString(PropOutputs, ref optionalValue4);
		outputs = optionalValue4.Split(',');
		for (int k = 0; k < outputs.Length; k++)
		{
			OutputType outputType2 = GetOutputType(outputs[k]);
			if (outputType2 != null)
			{
				if (!OrderedSlotOutputs.TryGetValue(outputType2, out var value))
				{
					value = new List<int>();
					OrderedSlotOutputs[outputType2] = value;
				}
				value.Add(k);
			}
		}
		base.Properties.ParseString(PropOpenSound, ref OpenSound);
		base.Properties.ParseString(PropRunningSound, ref RunningSound);
		base.Properties.ParseString(PropActivateSound, ref ActivateSound);
		base.Properties.ParseString(PropCloseSound, ref CloseSound);
		base.Properties.ParseFloat(PropTakeDelay, ref TakeDelay);
		string optionalValue5 = null;
		base.Properties.ParseString(PropItemIconBackdrop, ref optionalValue5);
		itemIconBackdrop = optionalValue5.Split(',');
		base.Properties.ParseString(PropLootLabelKey, ref lootLabelKey);
		base.Properties.ParseString(PropActivationEvent, ref activationEvent);
		base.Properties.ParseString(PropCloseEvent, ref closeEvent);
		base.Properties.ParseString(PropItemBackgroundColor, ref itemBackgroundColor);
		string optionalValue6 = "";
		requiredMods = null;
		base.Properties.ParseString(PropRequiredMods, ref optionalValue6);
		if (!string.IsNullOrEmpty(optionalValue6))
		{
			requiredMods = optionalValue6.Split(',');
		}
		base.Properties.ParseBool(PropRequiredModsOnly, ref requiredModsOnly);
		string optionalValue7 = ",,";
		base.Properties.ParseString(PropModTransformEnableNames, ref optionalValue7);
		modTransformEnableNames = optionalValue7.Split(',');
		optionalValue7 = ",,";
		base.Properties.ParseString(PropModTransformDisableNames, ref optionalValue7);
		modTransformDisableNames = optionalValue7.Split(',');
		optionalValue7 = "Count,Speed,Type";
		base.Properties.ParseString(PropModTypes, ref optionalValue7);
		string[] array3 = optionalValue7.Split(',');
		ModTypes = new ModEffectTypes[array3.Length];
		for (int l = 0; l < array3.Length && l < ModTypes.Length; l++)
		{
			ModTypes[l] = Enum.Parse<ModEffectTypes>(array3[l]);
		}
		base.Properties.ParseString(PropEmptyText, ref emptyText);
		base.Properties.ParseString(PropHasItem1Text, ref hasItem1Text);
		base.Properties.ParseString(PropHasItem2Text, ref hasItem2Text);
		base.Properties.ParseInt(PropOutputGridHeight, ref outputGridInitialHeight);
		base.Properties.ParseInt(PropFuelGridHeight, ref fuelGridHeight);
		base.Properties.ParseString(PropFuelTitleText, ref fuelTitleText);
		base.Properties.ParseString(PropFuelTitleSprite, ref fuelTitleSprite);
		base.Properties.ParseString(PropFuelTitleSpriteAtlas, ref fuelTitleSpriteAtlas);
		string optionalValue8 = null;
		base.Properties.ParseString(PropFuelTypesSprites, ref optionalValue8);
		if (optionalValue8 != null)
		{
			string[] array4 = optionalValue8.Split(',');
			int num = array4.Length / 2;
			fuelTypesSprites = new string[num];
			fuelTypesSpriteAtlases = new string[num];
			for (int m = 0; m < num; m++)
			{
				int num2 = m * 2;
				fuelTypesSprites[m] = array4[num2];
				FuelTypesSpriteAtlases[m] = array4[num2 + 1];
			}
		}
		base.Properties.ParseInt(PropCatalystGridHeight, ref catalystGridHeight);
		optionalValue8 = null;
		base.Properties.ParseString(PropCatalystTypes, ref optionalValue8);
		if (optionalValue8 != null)
		{
			catalystTypes = optionalValue8.Split(',');
		}
		base.Properties.ParseString(PropCatalystTypes, ref optionalValue8);
		if (optionalValue8 != null)
		{
			string[] array5 = optionalValue8.Split(',');
			int num3 = array5.Length / 2;
			CatalystConverts = new CatalystConvert[num3];
			for (int n = 0; n < num3; n++)
			{
				CatalystConverts[n] = new CatalystConvert(array5[n * 2], array5[n * 2 + 1]);
			}
		}
		base.Properties.ParseString(PropCatalystMultiplier, ref optionalValue8);
		if (optionalValue8 != null)
		{
			array = optionalValue8.Split(",");
			for (int i = 0; i < array.Length; i++)
			{
				string[] array6 = array[i].Split("=");
				if (array6.Length == 2)
				{
					if (!int.TryParse(array6[1], out var result))
					{
						result = 1;
					}
					CatalystMultipliers.Add(array6[0], result);
				}
			}
		}
		base.Properties.ParseString(PropCatalystRequirements, ref optionalValue8);
		if (optionalValue8 != null)
		{
			array = optionalValue8.Split(",");
			for (int i = 0; i < array.Length; i++)
			{
				string[] array7 = array[i].Split("=");
				if (array7.Length == 2)
				{
					if (!int.TryParse(array7[1], out var result2))
					{
						result2 = 1;
					}
					CatalystRequirements.Add(array7[0], result2);
				}
			}
		}
		base.Properties.ParseString(PropCatalystTitleText, ref catalystTitleText);
		base.Properties.ParseString(PropCatalystTitleSprite, ref catalystTitleSprite);
		base.Properties.ParseString(PropCatalystTitleSpriteAtlas, ref catalystTitleSpriteAtlas);
		base.Properties.ParseString(PropCatalystTypesSprites, ref optionalValue8);
		if (optionalValue8 != null)
		{
			string[] array8 = optionalValue8.Split(',');
			int num4 = array8.Length / 2;
			catalystTypesSprites = new string[num4];
			catalystTypesSpriteAtlases = new string[num4];
			for (int num5 = 0; num5 < num4; num5++)
			{
				int num6 = num5 * 2;
				catalystTypesSprites[num5] = array8[num6];
				catalystTypesSpriteAtlases[num5] = array8[num6 + 1];
			}
		}
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild)
		{
			addTileEntity(world, _chunk, _blockPos, _blockValue);
		}
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "Search"))
		{
			if (_commandName == "take")
			{
				takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
				return true;
			}
			return false;
		}
		return OnBlockActivated(_world, _blockPos, _blockValue, _player);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[1].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		if (_world.GetTileEntity(_result.blockPos) is TileEntityCollector tileEntityCollector && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityCollector.worldTimeTouched = _world.GetWorldTime();
			tileEntityCollector.SetEmpty();
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!(_world.GetTileEntity(_blockPos) is TileEntityCollector tileEntityCollector))
		{
			return string.Empty;
		}
		string arg = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg2 = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		if (tileEntityCollector.IsWaterEmpty())
		{
			return string.Format(Localization.Get(emptyText), arg2, arg);
		}
		if (tileEntityCollector.HasModConvert)
		{
			return string.Format(Localization.Get(hasItem2Text), arg2, arg);
		}
		return string.Format(Localization.Get(hasItem1Text), arg2, arg);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (world.GetTileEntity(_blockPos) is TileEntityCollector tileEntityCollector)
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

	public override DestroyedResult OnBlockDestroyedBy(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (_world.GetTileEntity(_bvRef) is TileEntityCollector tileEntityCollector)
		{
			tileEntityCollector.OnDestroy();
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(_bvRef);
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

	public override bool OnBlockActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_player.inventory.IsHoldingItemActionRunning())
		{
			return false;
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntityCollector target))
		{
			return false;
		}
		_player.AimingGun = false;
		LockManager.Instance.LockRequestLocal(target, null, 0);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool takeItemWithTimerCanTake(Vector3i _blockPos, EntityAlive _player)
	{
		if ((GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityCollector).IsEmpty())
		{
			return true;
		}
		GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttWorkstationNotEmpty"), string.Empty, "ui_denied");
		return false;
	}

	public bool UsesFuel()
	{
		bool flag = false;
		switch (CollectorType)
		{
		case CollectorTypes.Apiary:
			flag = XUiM_Recipes.ApiaryInput == 0f;
			break;
		case CollectorTypes.DewCollector:
			flag = XUiM_Recipes.DewCollectorInput == 0f;
			break;
		}
		if (!flag)
		{
			return fuelTypes.Values.Count > 0;
		}
		return false;
	}

	public bool UsesCatalyst()
	{
		return CatalystTypes.Length != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int modifyTime(int time, float modifier)
	{
		time = ((modifier != 0f) ? ((int)(1f / modifier * (float)time)) : 0);
		return time;
	}

	public int GetSandboxModifiedTime(int time)
	{
		if (time != 0)
		{
			switch (CollectorType)
			{
			case CollectorTypes.Apiary:
				time = modifyTime(time, XUiM_Recipes.ApiaryTimeModifier);
				break;
			case CollectorTypes.DewCollector:
				time = modifyTime(time, XUiM_Recipes.DewCollectorTimeModifier);
				break;
			}
			if (time == 0)
			{
				time = -1;
			}
		}
		return time;
	}

	public int GetSandboxModifiedFuelNeeded(int cost)
	{
		switch (CollectorType)
		{
		case CollectorTypes.Apiary:
			cost = (int)(XUiM_Recipes.ApiaryInput * (float)cost);
			break;
		case CollectorTypes.DewCollector:
			cost = (int)(XUiM_Recipes.DewCollectorInput * (float)cost);
			break;
		}
		return cost;
	}

	public int GetSandboxModifiedOutput(int cost)
	{
		switch (CollectorType)
		{
		case CollectorTypes.Apiary:
			cost = (int)(XUiM_Recipes.ApiaryOutput * (float)cost);
			break;
		case CollectorTypes.DewCollector:
			cost = (int)(XUiM_Recipes.DewCollectorOutput * (float)cost);
			break;
		}
		return cost;
	}
}

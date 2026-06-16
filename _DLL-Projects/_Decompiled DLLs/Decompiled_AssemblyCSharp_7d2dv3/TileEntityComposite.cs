using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TileEntityComposite : TileEntity
{
	public enum EBlockCommandOrder
	{
		First,
		Normal,
		Last
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCompositeData teData;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityFeature[] modulesCustomOrder;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityFeature[] modulesInternalOrder;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastUpdateFrameOfBlockActivationCommands;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastUpdateHadEnabledCommands;

	[PublicizedFrom(EAccessModifier.Private)]
	public const char ModuleCommandSeparator = ':';

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasBlockTriggered
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool OverridesPhysicalChecks
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public TileEntityCompositeData TeData => teData;

	public bool PlayerPlaced => Owner != null;

	public bool LocalPlayerIsOwner
	{
		get
		{
			if (Owner != null)
			{
				return Owner.Equals(PlatformManager.InternalLocalUserIdentifier);
			}
			return false;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs Owner
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Composite;
	}

	public TileEntityComposite(Chunk _chunk)
		: base(_chunk)
	{
	}

	public TileEntityComposite(Chunk _chunk, BlockValue _blockValue)
		: base(_chunk)
	{
		InitModulesFromBlock(_blockValue.Block as BlockCompositeTileEntity);
		TileEntityLegacyUtils.MigrateLegacyMetadata(this, _blockValue);
	}

	public TileEntityComposite(BlockCompositeTileEntity _block, TileEntityLegacyUtils.LegacyState _state)
		: base(null)
	{
		chunkPos = _state.chunkPos;
		heapMapLastTime = _state.heapMapLastTime;
		heapMapUpdateTime = _state.heapMapUpdateTime;
		InitModulesFromBlock(_block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityComposite(TileEntityComposite _original)
		: base(null)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitModulesFromBlock(BlockCompositeTileEntity _block)
	{
		if (!TileEntityCompositeData.FeaturesByBlock.TryGetValue(_block, out teData))
		{
			throw new Exception("Trying to initialize TileEntityComposite for block " + _block.GetBlockName() + " failed, no feature definitions found");
		}
		modulesCustomOrder = new ITileEntityFeature[teData.Features.Count];
		modulesInternalOrder = new ITileEntityFeature[teData.Features.Count];
		for (int i = 0; i < teData.Features.Count; i++)
		{
			TileEntityFeatureData tileEntityFeatureData = teData.Features[i];
			ITileEntityFeature tileEntityFeature = tileEntityFeatureData.InstantiateModule();
			modulesCustomOrder[tileEntityFeatureData.CustomOrder] = tileEntityFeature;
			modulesInternalOrder[i] = tileEntityFeature;
		}
		for (int j = 0; j < teData.Features.Count; j++)
		{
			TileEntityFeatureData featureData = teData.Features[j];
			modulesInternalOrder[j].Init(this, featureData);
		}
		PrecomputeCapabilities();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrecomputeCapabilities()
	{
		for (int i = 0; i < modulesCustomOrder.Length; i++)
		{
			ITileEntityFeature tileEntityFeature = modulesCustomOrder[i];
			HasBlockTriggered |= tileEntityFeature is IFeatureTriggerCapability;
			OverridesPhysicalChecks |= tileEntityFeature is IFeaturePhysicalCapabilities;
		}
	}

	public override TileEntity Clone()
	{
		TileEntityComposite tileEntityComposite = new TileEntityComposite(this);
		tileEntityComposite.CopyFrom(this);
		return tileEntityComposite;
	}

	public override void CopyFrom(TileEntity _other)
	{
		if (!(_other is TileEntityComposite tileEntityComposite))
		{
			Log.Warning($"TEC.CopyFrom with non TEC ({_other})");
			return;
		}
		InitModulesFromBlock(tileEntityComposite.TeData.Block);
		bUserAccessing = _other.IsUserAccessing();
		Owner = tileEntityComposite.Owner;
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CopyFrom(tileEntityComposite);
		}
	}

	public override void UpdateTick(World _world)
	{
		base.UpdateTick(_world);
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateTick(_world);
		}
	}

	public void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetBlockEntityData(_blockEntityData);
		}
	}

	public override void OnRemove(World _world)
	{
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnRemove(_world);
		}
		base.OnRemove(_world);
	}

	public override void OnLoad()
	{
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnLoad();
		}
		base.OnLoad();
	}

	public override void OnUnload(World _world)
	{
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnUnload(_world);
		}
		base.OnUnload(_world);
	}

	public override void OnDestroy()
	{
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnDestroy();
		}
		base.OnDestroy();
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (!HasBlockTriggered)
		{
			return;
		}
		for (int i = 0; i < modulesCustomOrder.Length; i++)
		{
			if (modulesCustomOrder[i] is IFeatureTriggerCapability featureTriggerCapability)
			{
				featureTriggerCapability.OnBlockTriggered(_player, _blockPos, _blockValue, _blockChanges, _triggeredBy);
			}
		}
	}

	public IEnumerable<IFeaturePhysicalCapabilities> GetOverridesPhysicalChecksModules()
	{
		if (!OverridesPhysicalChecks)
		{
			yield break;
		}
		for (int i = 0; i < modulesCustomOrder.Length; i++)
		{
			if (modulesCustomOrder[i] is IFeaturePhysicalCapabilities featurePhysicalCapabilities)
			{
				yield return featurePhysicalCapabilities;
			}
		}
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		if (_other is TileEntityComposite tileEntityComposite)
		{
			Owner = tileEntityComposite.Owner;
			ITileEntityFeature[] array = modulesInternalOrder;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpgradeDowngradeFrom(tileEntityComposite);
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ReplacedBy(_bvOld, _bvNew, _teNew);
		}
	}

	public override void Reset(FastTags<TagGroup.Global> _questTags)
	{
		base.Reset(_questTags);
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(_questTags);
		}
	}

	public override bool IsActive(World world)
	{
		return base.IsActive(world);
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		Owner = _userIdentifier;
		SetModified();
	}

	public string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		ITileEntityFeature[] array = modulesCustomOrder;
		for (int i = 0; i < array.Length; i++)
		{
			string activationText = array[i].GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
			if (activationText != null)
			{
				return activationText;
			}
		}
		return null;
	}

	public BlockActivationCommand[] InitBlockActivationCommands()
	{
		List<(BlockActivationCommand, int)> commands = new List<(BlockActivationCommand, int)>();
		int firsts = 100;
		int regular = 200;
		int last = 300;
		ITileEntityFeature[] array = modulesCustomOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].InitBlockActivationCommands([PublicizedFrom(EAccessModifier.Internal)] (BlockActivationCommand _bac, EBlockCommandOrder _order, TileEntityFeatureData _featureData) =>
			{
				BlockActivationCommand item = new BlockActivationCommand(_featureData.Name + ":" + _bac.text, _bac.icon, _bac.enabled, _bac.highlighted);
				int item2 = _order switch
				{
					EBlockCommandOrder.First => firsts++, 
					EBlockCommandOrder.Normal => regular++, 
					EBlockCommandOrder.Last => last++, 
					_ => throw new ArgumentOutOfRangeException("_order", _order, null), 
				};
				commands.Add((item, item2));
			});
		}
		commands.Sort([PublicizedFrom(EAccessModifier.Internal)] ((BlockActivationCommand, int) _a, (BlockActivationCommand, int) _b) => _a.Item2.CompareTo(_b.Item2));
		BlockActivationCommand[] array2 = new BlockActivationCommand[commands.Count];
		for (int num = 0; num < commands.Count; num++)
		{
			array2[num] = commands[num].Item1;
		}
		return array2;
	}

	public bool UpdateBlockActivationCommands(BlockActivationCommand[] _commands, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		int frameCount = Time.frameCount;
		if (frameCount <= lastUpdateFrameOfBlockActivationCommands)
		{
			return lastUpdateHadEnabledCommands;
		}
		lastUpdateHadEnabledCommands = false;
		for (int i = 0; i < _commands.Length; i++)
		{
			_commands[i].enabled = true;
		}
		for (int j = 0; j < _commands.Length; j++)
		{
			if (!SplitFullCommandName(_commands[j].text, out var _moduleName, out var _commandName))
			{
				_commands[j].enabled = false;
				continue;
			}
			ITileEntityFeature feature = GetFeature(_moduleName);
			if (feature == null)
			{
				_commands[j].enabled = false;
				continue;
			}
			ITileEntityFeature[] array = modulesInternalOrder;
			foreach (ITileEntityFeature tileEntityFeature in array)
			{
				_commands[j].enabled = tileEntityFeature.AllowBlockActivationCommand(feature, _commandName.Span, _world, _blockPos, _blockValue, _entityFocusing);
				if (!_commands[j].enabled)
				{
					break;
				}
			}
			lastUpdateHadEnabledCommands |= _commands[j].enabled;
		}
		lastUpdateFrameOfBlockActivationCommands = frameCount;
		return lastUpdateHadEnabledCommands;
	}

	public void OnBlockAdded(Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _owner)
	{
		Owner = _owner;
		for (int i = 0; i < modulesInternalOrder.Length; i++)
		{
			modulesInternalOrder[i].OnAdded(_blockPos, _blockValue);
		}
	}

	public void OnBlockValueChanged(Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		for (int i = 0; i < modulesInternalOrder.Length; i++)
		{
			modulesInternalOrder[i].OnBlockValueChanged(_blockPos, _oldBlockValue, _newBlockValue);
		}
	}

	public void OnBlockReset(Vector3i _blockPos, BlockValue _blockValue)
	{
		for (int i = 0; i < modulesInternalOrder.Length; i++)
		{
			modulesInternalOrder[i].OnBlockReset(_blockPos, _blockValue);
		}
	}

	public void OnBlockStartsToFall(Vector3i _blockPos, BlockValue _blockValue)
	{
		for (int i = 0; i < modulesInternalOrder.Length; i++)
		{
			modulesInternalOrder[i].OnBlockStartsToFall(_blockPos, _blockValue);
		}
	}

	public Block.DestroyedResult OnBlockDestroyedBy(Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		Block.DestroyedResult destroyedResult = Block.DestroyedResult.None;
		for (int i = 0; i < modulesCustomOrder.Length; i++)
		{
			Block.DestroyedResult destroyedResult2 = modulesCustomOrder[i].OnBlockDestroyedBy(_blockPos, _blockValue, _entityId, _bUseHarvestTool);
			if (destroyedResult == Block.DestroyedResult.None)
			{
				destroyedResult = destroyedResult2;
			}
		}
		return destroyedResult;
	}

	public Block.DestroyedResult OnBlockDestroyedByExplosion(Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		Block.DestroyedResult destroyedResult = Block.DestroyedResult.None;
		for (int i = 0; i < modulesCustomOrder.Length; i++)
		{
			Block.DestroyedResult destroyedResult2 = modulesCustomOrder[i].OnBlockDestroyedByExplosion(_blockPos, _blockValue, _playerThatStartedExpl);
			if (destroyedResult == Block.DestroyedResult.None)
			{
				destroyedResult = destroyedResult2;
			}
		}
		return destroyedResult;
	}

	public bool OnBlockActivated(BlockActivationCommand[] _commands, string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!SplitFullCommandName(_commandName, out var _moduleName, out var _commandName2))
		{
			return false;
		}
		return GetFeature(_moduleName)?.OnBlockActivated(_commandName2.Span, _world, _blockPos, _blockValue, _player) ?? false;
	}

	public ITileEntityFeature GetFeature(ReadOnlyMemory<char> _featureName)
	{
		int featureIndex = teData.GetFeatureIndex(_featureName);
		if (featureIndex < 0)
		{
			return null;
		}
		return modulesInternalOrder[featureIndex];
	}

	public T GetFeature<T>() where T : class
	{
		int featureIndex = teData.GetFeatureIndex<T>();
		if (featureIndex < 0)
		{
			return null;
		}
		return (T)modulesInternalOrder[featureIndex];
	}

	public static bool SplitFullCommandName(string _fullCommandName, out ReadOnlyMemory<char> _moduleName, out ReadOnlyMemory<char> _commandName)
	{
		int num = _fullCommandName.IndexOf(':');
		if (num < 0)
		{
			_moduleName = default(ReadOnlyMemory<char>);
			_commandName = default(ReadOnlyMemory<char>);
			return false;
		}
		_moduleName = _fullCommandName.AsMemory(0, num);
		_commandName = _fullCommandName.AsMemory(num + 1);
		return true;
	}

	public override string ToString()
	{
		return string.Format("[TEC] " + GetTileEntityType().ToStringCached() + "/" + ToWorldPos().ToString() + "/" + teData.Block.GetBlockName());
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		read(_br, _eStreamMode, null);
	}

	public void read(PooledBinaryReader _br, StreamModeRead _eStreamMode, int[] _blockIdMapping)
	{
		base.read(_br, _eStreamMode);
		int num = ((_eStreamMode != StreamModeRead.Persistency) ? 18 : ((!UseLocalVersioning()) ? GetLegacyForkVersion() : _br.ReadUInt16()));
		PooledBinaryReader.StreamReadSizeMarker _sizeMarker = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
		int num2 = _br.ReadInt32();
		int num3 = ((_blockIdMapping == null) ? num2 : _blockIdMapping[num2]);
		uint _bytesReceived;
		if (!(Block.list[num3] is BlockCompositeTileEntity blockCompositeTileEntity))
		{
			Log.Error("TileEntityComposite.read: Failed reading data, block " + Block.list[num3]?.GetBlockName() + " is not a BlockCompositeTileEntity");
			_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
			return;
		}
		try
		{
			Owner = PlatformUserIdentifierAbs.FromStream(_br);
			if (modulesInternalOrder == null || modulesCustomOrder == null || teData == null)
			{
				InitModulesFromBlock(blockCompositeTileEntity);
			}
			byte b = _br.ReadByte();
			if (num < 17)
			{
				if (b != modulesInternalOrder.Length)
				{
					Log.Warning(string.Format("{0}.{1}: Legacy composite TE (ver {2}) for block \"{3}\" ", "TileEntityComposite", "read", num, blockCompositeTileEntity.GetBlockName()) + $"has {b} features in stream but current definition has {modulesInternalOrder.Length}. Skipping TE payload.");
					_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
					return;
				}
				for (int i = 0; i < b; i++)
				{
					ITileEntityFeature tileEntityFeature = modulesInternalOrder[i];
					int num4 = _br.ReadInt32();
					if (tileEntityFeature.FeatureData.NameHash != num4)
					{
						Log.Warning(string.Format("{0}.{1}: Legacy composite TE (ver {2}) for block \"{3}\" ", "TileEntityComposite", "read", num, blockCompositeTileEntity.GetBlockName()) + $"hash mismatch at index {i}: stream 0x{num4:X8} != expected 0x{tileEntityFeature.FeatureData.NameHash:X8} ({tileEntityFeature.FeatureData.Name}). Skipping TE payload.");
						_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
						return;
					}
					tileEntityFeature.Read(_br, _eStreamMode);
				}
				if (!_br.ValidateSizeMarker(ref _sizeMarker, out var _bytesReceived2))
				{
					Log.Error("TileEntityComposite.read: Legacy composite TE for block \"" + blockCompositeTileEntity.GetBlockName() + "\" failed size validation: " + $"expected {_sizeMarker.ExpectedSize} B, read {_bytesReceived2} B");
				}
				return;
			}
			HashSet<int> hashSet = new HashSet<int>(b);
			for (int j = 0; j < b; j++)
			{
				int num5 = _br.ReadInt32();
				hashSet.Add(num5);
				PooledBinaryReader.StreamReadSizeMarker _sizeMarker2 = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
				int featureIndex = teData.GetFeatureIndex(num5);
				if (featureIndex < 0)
				{
					Log.Warning(string.Format("{0}.{1}: Block \"{2}\" no longer defines feature hash 0x{3:X8}. Skipping payload.", "TileEntityComposite", "read", blockCompositeTileEntity.GetBlockName(), num5));
					_br.ValidateSizeMarker(ref _sizeMarker2, out _bytesReceived);
					continue;
				}
				ITileEntityFeature tileEntityFeature2 = modulesInternalOrder[featureIndex];
				try
				{
					tileEntityFeature2.Read(_br, _eStreamMode);
				}
				catch (Exception e)
				{
					Log.Error(string.Format("{0}.{1}: Module read failed for feature \"{2}\" (hash 0x{3:X8}) ", "TileEntityComposite", "read", tileEntityFeature2.FeatureData.Name, num5) + "on block \"" + blockCompositeTileEntity.GetBlockName() + "\": Caught exception:");
					Log.Exception(e);
				}
				if (!_br.ValidateSizeMarker(ref _sizeMarker2, out var _bytesReceived3))
				{
					Log.Error(string.Format("{0}.{1}: Module \"{2}\" failed size validation (hash 0x{3:X8}) ", "TileEntityComposite", "read", tileEntityFeature2.FeatureData.Name, num5) + $"on block \"{blockCompositeTileEntity.GetBlockName()}\": expected {_sizeMarker2.ExpectedSize} B, read {_bytesReceived3} B");
				}
			}
			int num6 = 0;
			for (int k = 0; k < teData.Features.Count; k++)
			{
				int nameHash = teData.Features[k].NameHash;
				if (!hashSet.Contains(nameHash))
				{
					num6++;
				}
			}
			if (num6 > 0)
			{
				Log.Warning(string.Format("{0}.{1}: Block \"{2}\" has {3} feature(s) in the current definition ", "TileEntityComposite", "read", blockCompositeTileEntity.GetBlockName(), num6) + "that were not present in the saved data. They will use default values.");
			}
		}
		catch (Exception e2)
		{
			Log.Error("TileEntityComposite.read: Failed reading data for block " + Block.list[num3].GetBlockName() + ": Caught exception:");
			Log.Exception(e2);
			_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
			return;
		}
		if (!_br.ValidateSizeMarker(ref _sizeMarker, out var _bytesReceived4))
		{
			Log.Error("TileEntityComposite.read: Failed reading data for block \"" + Block.list[num3].GetBlockName() + "\": " + $"Data received {_sizeMarker.ExpectedSize} B, read {_bytesReceived4} B");
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
		PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
		_bw.Write(teData.Block.blockID);
		(GameManager.Instance.IsEditMode() ? null : Owner).ToStream(_bw);
		_bw.Write((byte)modulesInternalOrder.Length);
		for (int i = 0; i < modulesInternalOrder.Length; i++)
		{
			ITileEntityFeature tileEntityFeature = modulesInternalOrder[i];
			_bw.Write(tileEntityFeature.FeatureData.NameHash);
			PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker2 = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
			tileEntityFeature.Write(_bw, _eStreamMode);
			_bw.FinalizeSizeMarker(ref _sizeMarker2);
		}
		_bw.FinalizeSizeMarker(ref _sizeMarker);
	}
}

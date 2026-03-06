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

	public TileEntityCompositeData TeData => teData;

	public bool PlayerPlaced => Owner != null;

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

	public TileEntityComposite(Chunk _chunk, BlockCompositeTileEntity _block)
		: base(_chunk)
	{
		initModules(_block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityComposite(TileEntityComposite _original)
		: base(null)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initModules(BlockCompositeTileEntity _block)
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
		initModules(tileEntityComposite.TeData.Block);
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

	public void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		if (_ea != null && _ea.entityType == EntityType.Player)
		{
			Owner = PlatformManager.InternalLocalUserIdentifier;
			SetModified();
		}
		ITileEntityFeature[] array = modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].PlaceBlock(_world, _result, _ea);
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
		bool flag = false;
		for (int i = 0; i < _commands.Length; i++)
		{
			BlockActivationCommand _command = _commands[i];
			if (SplitFullCommandName(_command.text, out var _moduleName, out var _commandName))
			{
				ITileEntityFeature feature = GetFeature(_moduleName);
				if (feature != null)
				{
					feature.UpdateBlockActivationCommands(ref _command, _commandName.Span, _world, _blockPos, _blockValue, _entityFocusing);
					flag |= _command.enabled;
					_commands[i] = _command;
				}
			}
		}
		lastUpdateFrameOfBlockActivationCommands = frameCount;
		lastUpdateHadEnabledCommands = flag;
		return flag;
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
		return string.Format("[TEC] " + GetTileEntityType().ToStringCached() + "/" + ToWorldPos().ToString() + "/" + entityId + " / " + teData.Block.GetBlockName());
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		PooledBinaryReader.StreamReadSizeMarker _sizeMarker = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
		int num = _br.ReadInt32();
		uint _bytesReceived;
		if (!(Block.list[num] is BlockCompositeTileEntity blockCompositeTileEntity))
		{
			Log.Error("TileEntityComposite.read: Failed reading data, block " + Block.list[num]?.GetBlockName() + " is not a BlockCompositeTileEntity");
			_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
			return;
		}
		try
		{
			Owner = PlatformUserIdentifierAbs.FromStream(_br);
			if (modulesCustomOrder == null)
			{
				initModules(blockCompositeTileEntity);
			}
			byte b = _br.ReadByte();
			if (b != modulesCustomOrder.Length)
			{
				Log.Error(string.Format("{0}.{1}: Failed reading data for block {2}: Received {3} features, expected {4}", "TileEntityComposite", "read", Block.list[num].GetBlockName(), b, modulesCustomOrder.Length));
				_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
				return;
			}
			for (int i = 0; i < b; i++)
			{
				ITileEntityFeature tileEntityFeature = modulesInternalOrder[i];
				int num2 = _br.ReadInt32();
				if (tileEntityFeature.FeatureData.NameHash != num2)
				{
					Log.Error(string.Format("{0}.{1}: Failed reading data for block {2}: Received hash {3:X8} does not equal expected hash {4:X8} for feature {5}", "TileEntityComposite", "read", Block.list[num].GetBlockName(), num2, tileEntityFeature.FeatureData.NameHash, tileEntityFeature.FeatureData.Name));
					_br.ValidateSizeMarker(ref _sizeMarker, out _bytesReceived);
					return;
				}
				tileEntityFeature.Read(_br, _eStreamMode, readVersion);
			}
		}
		catch (Exception e)
		{
			Log.Error("TileEntityComposite.read: Failed reading data for block " + Block.list[num].GetBlockName() + ": Caught exception:");
			Log.Exception(e);
		}
		if (!_br.ValidateSizeMarker(ref _sizeMarker, out var _bytesReceived2))
		{
			Log.Error(string.Format("{0}.{1}: Failed reading data for block {2}: Data received {3} B, read {4} B", "TileEntityComposite", "read", Block.list[num].GetBlockName(), _sizeMarker.ExpectedSize, _bytesReceived2));
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
		_bw.Write(teData.Block.blockID);
		Owner.ToStream(_bw);
		_bw.Write((byte)modulesCustomOrder.Length);
		for (int i = 0; i < modulesInternalOrder.Length; i++)
		{
			ITileEntityFeature tileEntityFeature = modulesInternalOrder[i];
			_bw.Write(tileEntityFeature.FeatureData.NameHash);
			tileEntityFeature.Write(_bw, _eStreamMode);
		}
		_bw.FinalizeSizeMarker(ref _sizeMarker);
	}
}

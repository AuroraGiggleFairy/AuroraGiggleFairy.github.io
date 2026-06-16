using System;

public interface ITileEntityFeature
{
	TileEntityFeatureData FeatureData { get; }

	TileEntityComposite Parent { get; }

	void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData);

	void CopyFrom(TileEntityComposite _other);

	void OnRemove(World _world);

	void OnLoad();

	void OnUnload(World _world);

	void OnDestroy();

	void OnAdded(Vector3i _blockPos, BlockValue _blockValue);

	void OnBlockValueChanged(Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue);

	void OnBlockReset(Vector3i _blockPos, BlockValue _blockValue);

	void OnBlockStartsToFall(Vector3i _blockPos, BlockValue _blockValue);

	Block.DestroyedResult OnBlockDestroyedBy(Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool);

	Block.DestroyedResult OnBlockDestroyedByExplosion(Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl);

	void SetBlockEntityData(BlockEntityData _blockEntityData);

	void UpgradeDowngradeFrom(TileEntityComposite _other);

	void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew);

	void Reset(FastTags<TagGroup.Global> _questTags);

	void UpdateTick(World _world);

	string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName);

	void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback);

	bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing);

	bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player);

	void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode);

	void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode);
}

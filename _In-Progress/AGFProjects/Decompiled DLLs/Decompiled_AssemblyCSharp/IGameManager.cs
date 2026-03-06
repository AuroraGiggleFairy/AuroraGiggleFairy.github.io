using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
	void SetBlocksRPC(List<BlockChangeInfo> _changes, PlatformUserIdentifierAbs _persistentPlayerId = null);

	void SimpleRPC(int _entityId, SimpleRPCType _rpcType, bool _bExeLocal, bool _bOnlyLocal);

	void SpawnBlockParticleEffect(Vector3i _blockPos, ParticleEffect _pe);

	bool HasBlockParticleEffect(Vector3i _blockPos);

	Transform GetBlockParticleEffect(Vector3i _blockPos);

	void RemoveBlockParticleEffect(Vector3i _blockPos);

	ChunkManager.ChunkObserver AddChunkObserver(Vector3 _initialPosition, bool _bBuildVisualMeshAround, int _viewDim, int _entityIdToSendChunksTo);

	void RemoveChunkObserver(ChunkManager.ChunkObserver _observer);

	PersistentPlayerList GetPersistentPlayerList();

	PersistentPlayerData GetPersistentLocalPlayer();

	void HandlePersistentPlayerDisconnected(int _entityId);

	void ItemActionEffectsServer(int _entityId, int _slotIdx, int _itemActionIdx, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0);

	void ItemActionEffectsClient(int _entityId, int _slotIdx, int _itemActionIdx, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0);

	void SpawnParticleEffectServer(ParticleEffect _pe, int _entityIdThatCausedIt, bool _forceCreation = false, bool _worldSpawn = false);

	void SpawnParticleEffectClient(ParticleEffect _pe, int _entityIdThatCausedIt, bool _forceCreation = false, bool _worldSpawn = false);

	Transform SpawnParticleEffectClientForceCreation(ParticleEffect _pe, int _entityIdThatCausedIt, bool worldSpawn);

	void ExplosionServer(int _clrIdx, Vector3 _worldPos, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _playerIdx, float _delay, bool _bRemoveBlockAtExplPosition, ItemValue _itemValueExplosive = null);

	GameObject ExplosionClient(int _clrIdx, Vector3 _center, Quaternion _rotation, int _index, int _blastPower, float _blastRadius, float _blockDamage, int _entityId, List<BlockChangeInfo> _explosionChanges);

	void ItemReloadServer(int _entityId);

	void ItemReloadClient(int _entityId);

	void ItemDropServer(ItemStack _itemStack, Vector3 _dropPos, Vector3 _randomPosAdd, int _entityId = -1, float _lifetime = 60f, bool _bDropPosIsRelativeToHead = false);

	void ItemDropServer(ItemStack _itemStack, Vector3 _dropPos, Vector3 _randomPosAdd, Vector3 _initialMotion, int _entityId = -1, float _lifetime = 60f, bool _bDropPosIsRelativeToHead = false, int _clientInstanceId = 0);

	void RequestToSpawnEntityServer(EntityCreationData _ecd);

	void DropContentOfLootContainerServer(BlockValue _bvOld, Vector3i _worldPos, int _entityId, ITileEntityLootable _teOld = null);

	List<EntityLootContainer> DropContentInLootContainerServer(int _droppedByID, string _containerEntity, Vector3 _pos, ItemStack[] items, bool _skipIfEmpty = false, Vector3? increment = null);

	void CollectEntityServer(int _entityId, int _playerId);

	void CollectEntityClient(int _entityId, int _playerId);

	void PickupBlockServer(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerId, PlatformUserIdentifierAbs persistentPlayerId = null);

	void PickupBlockClient(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerId);

	void PlaySoundAtPositionServer(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance);

	void PlaySoundAtPositionServer(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, int _entityId);

	void PlaySoundAtPositionClient(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance);

	void SetWorldTime(ulong _worldTime);

	void AddVelocityToEntityServer(int _entityId, Vector3 _velToAdd);

	void AddExpServer(int _entityId, string _skill, int _experience);

	void AddScoreServer(int _entityId, int _zombieKills, int _playerKills, int _otherTeamnumber, int _conditions);

	IEnumerator ResetWindowsAndLocksByPlayer(int _playerId);

	IEnumerator ResetWindowsAndLocksByChunks(HashSetLong chunks);

	void TELockServer(int _clrIdx, Vector3i _blockPos, int _lootEntityId, int _entityIdThatOpenedIt, string _customUi = null);

	void TEUnlockServer(int _clrIdx, Vector3i _blockPos, int _lootEntityId, bool allowContainerDestroy = true);

	void TEAccessClient(int _clrIdx, Vector3i _blockPos, int _lootEntityId, int _entityIdThatOpenedIt, string _customUi = null);

	void TEDeniedAccessClient(int _clrIdx, Vector3i _blockPos, int _lootEntityId, int _entityIdThatOpenedIt);
}

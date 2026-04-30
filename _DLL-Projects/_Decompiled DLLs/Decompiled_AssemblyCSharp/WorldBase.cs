using System.Collections.Generic;
using Audio;
using UnityEngine;

public abstract class WorldBase : IBlockAccess
{
	public ChunkClusterList ChunkClusters = new ChunkClusterList();

	public ChunkCluster ChunkCache;

	public abstract IChunk GetChunkSync(Vector3i chunkPos);

	public abstract IChunk GetChunkFromWorldPos(int x, int y, int z);

	public abstract IChunk GetChunkFromWorldPos(Vector3i _blockPos);

	public abstract void GetChunkFromWorldPos(int _blockX, int _blockZ, ref IChunk _chunk);

	public abstract bool GetChunkFromWorldPos(Vector3i _blockPos, ref IChunk _chunk);

	public abstract void AddFallingBlocks(IList<Vector3i> _list);

	public abstract byte GetStability(int worldX, int worldY, int worldZ);

	public abstract byte GetStability(Vector3i _pos);

	public abstract void SetStability(int worldX, int worldY, int worldZ, byte stab);

	public abstract void SetStability(Vector3i _pos, byte stab);

	public abstract byte GetHeight(int worldX, int worldZ);

	public abstract bool IsWater(int _x, int _y, int _z);

	public abstract bool IsWater(Vector3i _pos);

	public abstract bool IsWater(Vector3 _pos);

	public abstract bool IsAir(int _x, int _y, int _z);

	public abstract IGameManager GetGameManager();

	public abstract Manager GetAudioManager();

	public abstract AIDirector GetAIDirector();

	public abstract void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue);

	public abstract void SetBlockRPC(Vector3i _blockPos, BlockValue _blockValue);

	public abstract void SetBlockRPC(Vector3i _blockPos, BlockValue _blockValue, sbyte _density);

	public abstract void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density);

	public abstract void SetBlockRPC(Vector3i _blockPos, sbyte _density);

	public abstract void SetBlocksRPC(List<BlockChangeInfo> _blockChangeInfo);

	public abstract void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density, int _changingEntityId);

	public abstract void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _changingEntityId);

	public abstract BlockValue GetBlock(int _x, int _y, int _z);

	public abstract BlockValue GetBlock(int _clrIdx, int _x, int _y, int _z);

	public abstract BlockValue GetBlock(Vector3i _pos);

	public abstract BlockValue GetBlock(int _clrIdx, Vector3i _pos);

	public abstract sbyte GetDensity(int _clrIdx, Vector3i _blockPos);

	public abstract sbyte GetDensity(int _clrIdx, int _x, int _y, int _z);

	public abstract Entity GetEntity(int _entityId);

	public abstract void ChangeClientEntityIdToServer(int _clientEntityId, int _serverEntityId);

	public abstract bool IsRemote();

	public abstract WorldBlockTicker GetWBT();

	public abstract bool IsOpenSkyAbove(int _clrIdx, int _x, int _y, int _z);

	public abstract int GetBlockLightValue(int _clrIdx, Vector3i blockPos);

	public abstract float GetLightBrightness(Vector3i blockPos);

	public abstract bool IsEditor();

	public abstract TileEntity GetTileEntity(Vector3i _blockPos);

	public abstract TileEntity GetTileEntity(int _clrIdx, Vector3i _blockPos);

	public abstract List<EntityPlayer> GetPlayers();

	public abstract EntityPlayerLocal GetPrimaryPlayer();

	public abstract void AddLocalPlayer(EntityPlayerLocal _localPlayer);

	public abstract void RemoveLocalPlayer(EntityPlayerLocal _localPlayer);

	public abstract List<EntityPlayerLocal> GetLocalPlayers();

	public abstract bool IsLocalPlayer(int _playerId);

	public abstract EntityPlayerLocal GetLocalPlayerFromID(int _playerId);

	public abstract EntityPlayerLocal GetClosestLocalPlayer(Vector3 _position);

	public abstract Vector3 GetVectorToClosestLocalPlayer(Vector3 _position);

	public abstract float GetSquaredDistanceToClosestLocalPlayer(Vector3 _position);

	public abstract float GetDistanceToClosestLocalPlayer(Vector3 _position);

	public abstract bool CanPlaceLandProtectionBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative);

	public abstract bool CanPlaceBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative, bool traderAllowed = false);

	public abstract bool CanPickupBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative);

	public abstract float GetLandProtectionHardnessModifier(Vector3i blockPos, EntityAlive lpRelative, PersistentPlayerData ppData);

	public abstract bool IsMyLandProtectedBlock(Vector3i worldBlockPos, PersistentPlayerData lpRelative, bool traderAllowed = false);

	public abstract EnumLandClaimOwner GetLandClaimOwner(Vector3i worldBlockPos, PersistentPlayerData lpRelative);

	public abstract ulong GetWorldTime();

	public abstract WorldCreationData GetWorldCreationData();

	public abstract void UnloadEntities(List<Entity> _entityList, bool _forceUnload = false);

	public abstract Entity RemoveEntity(int _entityId, EnumRemoveEntityReason _reason);

	public abstract int AddSleeperVolume(SleeperVolume _volume);

	public abstract int FindSleeperVolume(Vector3i mins, Vector3i maxs);

	public abstract void ResetSleeperVolumes(long chunkKey);

	public abstract int GetSleeperVolumeCount();

	public abstract SleeperVolume GetSleeperVolume(int index);

	public abstract int AddTriggerVolume(TriggerVolume _volume);

	public abstract int FindTriggerVolume(Vector3i mins, Vector3i maxs);

	public abstract void ResetTriggerVolumes(long chunkKey);

	public abstract int GetTriggerVolumeCount();

	public abstract TriggerVolume GetTriggerVolume(int index);

	public abstract int AddWallVolume(WallVolume _volume);

	public abstract int FindWallVolume(Vector3i mins, Vector3i maxs);

	public abstract int GetWallVolumeCount();

	public abstract List<WallVolume> GetAllWallVolumes();

	public abstract WallVolume GetWallVolume(int index);

	public abstract GameRandom GetGameRandom();

	public abstract void AddPendingDowngradeBlock(Vector3i _blockPos);

	public abstract bool TryRetrieveAndRemovePendingDowngradeBlock(Vector3i _blockPos);

	[PublicizedFrom(EAccessModifier.Protected)]
	public WorldBase()
	{
	}
}

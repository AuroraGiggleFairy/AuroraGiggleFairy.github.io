using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public abstract class WorldBase : IBlockAccess, IChunkAccess
{
	public ChunkCluster ChunkCache;

	public abstract IChunk GetChunkSync(int chunkX, int chunkZ);

	public abstract IChunk GetChunkSync(long chunkKey);

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkX, chunkY, chunkZ);
	}

	public IChunk GetChunkSync(Vector2i chunkPos)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkPos);
	}

	public IChunk GetChunkSync(PropRef propRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, propRef);
	}

	public IChunk GetChunkSync(BlockValueRef bvRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, bvRef);
	}

	public IChunk GetChunkFromWorldPos(int x, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, y, z);
	}

	public IChunk GetChunkFromWorldPos(Vector3i blockPos)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos);
	}

	public bool GetChunkFromWorldPos(int x, int z, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z, ref chunk);
	}

	public bool GetChunkFromWorldPos(Vector3i blockPos, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos, ref chunk);
	}

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

	public abstract void SetBlocksRPC(List<BlockChangeInfo> _blockChangeInfo);

	public void SetBlockRPC(BlockChangeInfo _info)
	{
		SetBlocksRPC(new List<BlockChangeInfo> { _info });
	}

	public void SetBlockRPC(BlockValueRef _bvRef, BlockValue _blockValue)
	{
		SetBlockRPC(new BlockChangeInfo(_bvRef, _blockValue, _updateLight: true));
	}

	public void SetBlockRPC(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density)
	{
		SetBlockRPC(new BlockChangeInfo(_bvRef, _blockValue, _density));
	}

	public void SetBlockRPC(BlockValueRef _bvRef, sbyte _density)
	{
		SetBlockRPC(new BlockChangeInfo(_bvRef, _density));
	}

	public void SetBlockRPC(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density, int _changingEntityId)
	{
		SetBlockRPC(new BlockChangeInfo(_bvRef, _blockValue, _density, _changingEntityId));
	}

	public void SetBlockRPC(BlockValueRef _bvRef, BlockValue _blockValue, int _changingEntityId)
	{
		SetBlockRPC(new BlockChangeInfo(_bvRef, _blockValue, _updateLight: true, _changingEntityId));
	}

	public abstract void SetPropsRPC(List<PropChangeInfo> _propChangeInfo);

	public void SetPropRPC(PropChangeInfo _change)
	{
		SetPropsRPC(new List<PropChangeInfo> { _change });
	}

	public abstract BlockValue GetBlock(int _x, int _y, int _z);

	public BlockValue GetBlock(Vector3i pos)
	{
		return IBlockAccess.DefaultGetBlock(this, pos);
	}

	public BlockValue GetBlock(BlockValueRef bvRef)
	{
		return IBlockAccess.DefaultGetBlock(this, bvRef);
	}

	public abstract sbyte GetDensity(int _x, int _y, int _z);

	public sbyte GetDensity(Vector3i _blockPos)
	{
		return GetDensity(_blockPos.x, _blockPos.y, _blockPos.z);
	}

	public sbyte GetDensity(BlockValueRef _bvRef)
	{
		return _bvRef.Type switch
		{
			BlockValueRefType.None => 0, 
			BlockValueRefType.Block => GetDensity(_bvRef.BlockPosition), 
			BlockValueRefType.Prop => 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public abstract PropValue GetProp(int chunkX, int chunkZ, int propId);

	public abstract PropValue GetProp(long chunkKey, int propId);

	public PropValue GetProp(Vector2i chunkPos, int propId)
	{
		return IBlockAccess.DefaultGetProp(this, chunkPos, propId);
	}

	public PropValue GetProp(PropRef propRef)
	{
		return IBlockAccess.DefaultGetProp(this, propRef);
	}

	public abstract Entity GetEntity(int _entityId);

	public abstract void ChangeClientEntityIdToServer(int _clientEntityId, int _serverEntityId);

	public abstract bool IsRemote();

	public abstract WorldBlockTicker GetWBT();

	public abstract bool IsOpenSkyAbove(int _x, int _y, int _z);

	public abstract int GetBlockLightValue(Vector3i blockPos);

	public abstract float GetLightBrightness(Vector3i blockPos);

	public abstract bool IsEditor();

	public abstract TileEntity GetTileEntity(Vector3i _blockPos);

	public TileEntity GetTileEntity(BlockValueRef _bvRef)
	{
		return _bvRef.Type switch
		{
			BlockValueRefType.None => null, 
			BlockValueRefType.Block => GetTileEntity(_bvRef.BlockPosition), 
			BlockValueRefType.Prop => null, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

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

	public abstract SleeperVolume GetSleeperVolume(int index);

	public abstract int AddTriggerVolume(TriggerVolume _volume);

	public abstract int FindTriggerVolume(Vector3i mins, Vector3i maxs);

	public abstract void ResetTriggerVolumes(long chunkKey);

	public abstract TriggerVolume GetTriggerVolume(int index);

	public abstract int AddWallVolume(WallVolume _volume);

	public abstract int FindWallVolume(Vector3i mins, Vector3i maxs);

	public abstract List<(int, WallVolume)> GetAllWallVolumes();

	public abstract WallVolume GetWallVolume(int index);

	public abstract bool HasWallVolumes(List<int> _wallVolumeIds);

	public abstract GameRandom GetGameRandom();

	public abstract void AddPendingDowngradeBlock(BlockValueRef _bvRef);

	public abstract bool TryRetrieveAndRemovePendingDowngradeBlock(BlockValueRef _bvRef);

	[PublicizedFrom(EAccessModifier.Protected)]
	public WorldBase()
	{
	}
}

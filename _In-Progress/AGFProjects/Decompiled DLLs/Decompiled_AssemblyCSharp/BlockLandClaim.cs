using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockLandClaim : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum LandClaimCommand
	{
		ShowBounds,
		HideBounds,
		Remove
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string activePrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inactivePrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[3]
	{
		new BlockActivationCommand("show_bounds", "frames", _enabled: false),
		new BlockActivationCommand("hide_bounds", "frames", _enabled: false),
		new BlockActivationCommand("remove", "x", _enabled: false)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRepairing;

	public BlockLandClaim()
	{
		base.IsNotifyOnLoadUnload = true;
		activePrompt = Localization.Get("activeBlockPrompt");
		inactivePrompt = Localization.Get("inactiveBlockPrompt");
	}

	public bool ServerCheckPrimary(Vector3i _blockPos)
	{
		return GameManager.Instance.persistentPlayers.GetLandProtectionBlockOwner(_blockPos) != null;
	}

	public static bool IsPrimary(BlockValue _blockValue)
	{
		return (_blockValue.meta & 2) == 0;
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool flag = ServerCheckPrimary(_blockPos);
			if (flag != IsPrimary(_blockValue))
			{
				_blockValue.meta = (byte)((_blockValue.meta & -3) | ((!flag) ? 2 : 0));
				if (!IsPrimary(_blockValue))
				{
					_blockValue.damage = MaxDamage - 1;
				}
				_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
			}
		}
		if (!IsPrimary(_blockValue))
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(_blockPos.ToVector3(), "land_claim");
			if (GameManager.Instance.persistentPlayers.m_lpBlockMap.ContainsKey(_blockPos))
			{
				PersistentPlayerData persistentPlayerData = GameManager.Instance.persistentPlayers.m_lpBlockMap[_blockPos];
				GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_blockPos);
				persistentPlayerData.LPBlocks.Remove(_blockPos);
			}
		}
		if (!(_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityLandClaim tileEntityLandClaim))
		{
			return;
		}
		if (!IsPrimary(_blockValue))
		{
			tileEntityLandClaim.ShowBounds = false;
		}
		if (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				tileEntityLandClaim.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(tileEntityLandClaim.ShowBounds);
			}
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (IsPrimary(_newBlockValue))
		{
			return;
		}
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntityLandClaim != null)
		{
			tileEntityLandClaim.ShowBounds = false;
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				tileEntityLandClaim.BoundsHelper = null;
				boundsHelper.gameObject.SetActive(value: false);
				LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool flag = ServerCheckPrimary(_blockPos);
			if (flag != IsPrimary(_blockValue))
			{
				_blockValue.meta = (byte)((_blockValue.meta & -3) | ((!flag) ? 2 : 0));
				if (!IsPrimary(_blockValue))
				{
					_blockValue.damage = MaxDamage - 1;
				}
				_world.SetBlockRPC(0, _blockPos, _blockValue);
			}
		}
		if (_ebcd == null)
		{
			return;
		}
		Chunk chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityLandClaim == null)
		{
			tileEntityLandClaim = new TileEntityLandClaim(chunk);
			if (tileEntityLandClaim != null)
			{
				tileEntityLandClaim.localChunkPos = World.toBlock(_blockPos);
				chunk.AddTileEntity(tileEntityLandClaim);
			}
		}
		if (tileEntityLandClaim == null)
		{
			Log.Error("Tile Entity Land Claim was unable to be created!");
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		TileEntityLandClaim tileEntityLandClaim = _world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntityLandClaim;
		if (tileEntityLandClaim == null)
		{
			TileEntityLandClaim tileEntityLandClaim2 = new TileEntityLandClaim(_chunk);
			tileEntityLandClaim2.localChunkPos = World.toBlock(_blockPos);
			_chunk.AddTileEntity(tileEntityLandClaim2);
			tileEntityLandClaim = tileEntityLandClaim2;
		}
		if (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				tileEntityLandClaim.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(tileEntityLandClaim.ShowBounds);
			}
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityLandClaim)
		{
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
		}
	}

	public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		((TileEntityLandClaim)_world.GetTileEntity(_result.clrIdx, _result.blockPos))?.SetOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public override string GetCustomDescription(Vector3i _blockPos, BlockValue _bv)
	{
		if (IsPrimary(_bv))
		{
			return string.Format(activePrompt, GetLocalizedBlockName());
		}
		return string.Format(inactivePrompt, GetLocalizedBlockName());
	}

	public void HandleDeactivatingCurrentLandClaims(PersistentPlayerData ppData)
	{
		List<Vector3i> landProtectionBlocks = ppData.GetLandProtectionBlocks();
		World world = GameManager.Instance.World;
		int num = GameStats.GetInt(EnumGameStats.LandClaimCount);
		if (landProtectionBlocks.Count <= num)
		{
			return;
		}
		int num2 = landProtectionBlocks.Count - num;
		for (int i = 0; i < num2; i++)
		{
			Vector3i vector3i = landProtectionBlocks[0];
			BlockValue block = world.GetBlock(vector3i);
			if (!block.isair)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					block.meta = 2;
					block.damage = MaxDamage - 1;
					world.SetBlockRPC(0, vector3i, block);
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, vector3i.ToVector3()));
				}
				NavObjectManager.Instance.UnRegisterNavObjectByPosition(vector3i.ToVector3(), "land_claim");
				LandClaimBoundsHelper.RemoveBoundsHelper(vector3i.ToVector3());
			}
			GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(vector3i);
			ppData.LPBlocks.RemoveAt(0);
		}
	}

	public static void HandleDeactivateLandClaim(Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(_blockPos);
		if (!block.isair)
		{
			block.meta = 2;
			block.damage = block.Block.MaxDamage - 1;
			world.SetBlockRPC(0, _blockPos, block);
			GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_blockPos);
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(_blockPos.ToVector3(), "land_claim");
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
		}
	}

	public override bool CanRepair(BlockValue _blockValue)
	{
		if (base.CanRepair(_blockValue))
		{
			return IsPrimary(_blockValue);
		}
		return false;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (IsPrimary(_blockValue))
		{
			TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
			string text = "";
			if (tileEntityLandClaim != null)
			{
				text = (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier) ? ("\n" + Localization.Get("useWorkstation")) : "");
			}
			return GetCustomDescription(_blockPos, _blockValue) + text;
		}
		return GetCustomDescription(_blockPos, _blockValue);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityLandClaim tileEntityLandClaim))
		{
			return false;
		}
		switch (_commandName)
		{
		case "show_bounds":
		case "hide_bounds":
			tileEntityLandClaim.ShowBounds = !tileEntityLandClaim.ShowBounds;
			updateViewBounds(_world, _cIdx, _blockPos, _blockValue, tileEntityLandClaim.ShowBounds);
			return true;
		case "repair":
			if (!isRepairing)
			{
				isRepairing = true;
				GameManager.Instance.StartCoroutine(repair(_blockPos));
			}
			return true;
		case "remove":
			_world.SetBlockRPC(_blockPos, BlockValue.Air);
			return true;
		default:
			return false;
		}
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return OnBlockActivated(_world, _cIdx, parentPos, block, _player);
		}
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityLandClaim == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityLandClaim.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityLandClaim.entityId, _player.entityId);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntityLandClaim == null)
		{
			return false;
		}
		if (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
		{
			return IsPrimary(_blockValue);
		}
		return false;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntityLandClaim == null)
		{
			return BlockActivationCommand.Empty;
		}
		bool flag = tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier) && IsPrimary(_blockValue);
		if (!flag)
		{
			return BlockActivationCommand.Empty;
		}
		cmds[0].enabled = flag && !tileEntityLandClaim.ShowBounds;
		cmds[1].enabled = flag && tileEntityLandClaim.ShowBounds;
		cmds[2].enabled = flag;
		return cmds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateViewBounds(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _enableState)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				tileEntityLandClaim.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(_enableState);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repair(Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		if (world.GetTileEntity(_blockPos) is TileEntityLandClaim tileEntityLandClaim && tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
		{
			Vector2i blocksMin = new Vector2i(_blockPos.x - 22, _blockPos.z - 22);
			Vector2i blocksMax = new Vector2i(blocksMin.x + 44, blocksMin.y + 44);
			Vector2i v = new Vector2i(blocksMin);
			Vector2i v2 = new Vector2i(blocksMax);
			Vector2i startChunk = World.toChunkXZ(v);
			Vector2i endChunk = World.toChunkXZ(v2);
			for (int cx = startChunk.x; cx <= endChunk.x; cx++)
			{
				yield return repairChunkCheckBounds(cx, startChunk.y, world, blocksMin, blocksMax);
				yield return repairChunkCheckBounds(cx, endChunk.y, world, blocksMin, blocksMax);
			}
			for (int cz = startChunk.y; cz <= endChunk.y - 1; cz++)
			{
				yield return repairChunkCheckBounds(startChunk.x, cz, world, blocksMin, blocksMax);
				yield return repairChunkCheckBounds(endChunk.x, cz, world, blocksMin, blocksMax);
			}
			for (int cz = startChunk.y + 1; cz <= endChunk.y - 1; cz++)
			{
				for (int cx = startChunk.x + 1; cx <= endChunk.x - 1; cx++)
				{
					yield return repairChunk(cx, cz, world);
				}
			}
		}
		isRepairing = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repairChunkCheckBounds(int cx, int cz, World world, Vector2i blocksMin, Vector2i blocksMax)
	{
		Chunk chunk = (Chunk)world.GetChunkSync(cx, cz);
		if (chunk == null)
		{
			yield break;
		}
		yield return chunk.LoopOverAllBlocksCoroutine([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
		{
			if (!bv.isair && bv.damage > 0 && x >= blocksMin.x && z >= blocksMin.y && x < blocksMax.x && z < blocksMax.y)
			{
				Vector3 worldPos = chunk.worldPosIMin + Vector3.one * 0.5f;
				repairBlock(worldPos, bv);
			}
		});
		Debug.Log($"Edge Chunk {chunk.X},{chunk.Z} processed");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repairChunk(int cx, int cz, World world)
	{
		Chunk chunk = (Chunk)world.GetChunkSync(cx, cz);
		if (chunk != null)
		{
			yield return chunk.LoopOverAllBlocksCoroutine([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
			{
				if (!bv.isair && bv.damage > 0)
				{
					Vector3 vector = chunk.worldPosIMin + Vector3.one * 0.5f;
					repairBlock(vector + new Vector3(x, y, z), bv);
				}
			});
		}
		Debug.Log($"Chunk {chunk.X},{chunk.Z} processed");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void repairBlock(Vector3 worldPos, BlockValue bv)
	{
		Debug.Log($"{worldPos.x},{worldPos.y},{worldPos.z}");
	}
}

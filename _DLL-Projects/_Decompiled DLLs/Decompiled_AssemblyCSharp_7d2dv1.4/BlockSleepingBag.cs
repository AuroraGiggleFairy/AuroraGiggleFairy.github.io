using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class BlockSleepingBag : BlockSiblingRemove
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i rotationToAddVector(int _rotation)
	{
		return _rotation switch
		{
			0 => new Vector3i(0, 0, 1), 
			1 => new Vector3i(1, 0, 0), 
			2 => new Vector3i(0, 0, -1), 
			3 => new Vector3i(-1, 0, 0), 
			_ => Vector3i.zero, 
		};
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (!base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		Vector3i vector3i = _blockPos + rotationToAddVector(_blockValue.rotation);
		Block block = _blockValue.Block;
		if (block.isMultiBlock)
		{
			vector3i = _blockPos + block.multiBlockPos.Get(0, _blockValue.type, _blockValue.rotation);
		}
		BlockValue block2 = _world.GetBlock(_clrIdx, vector3i);
		if (!block2.isair && !block2.Block.CanBlocksReplaceOrGroundCover())
		{
			return false;
		}
		BlockValue block3 = _world.GetBlock(_clrIdx, _blockPos - Vector3i.up);
		BlockValue block4 = _world.GetBlock(_clrIdx, vector3i - Vector3i.up);
		if (block3.isair || block4.isair)
		{
			return false;
		}
		if (!block3.Block.blockMaterial.StabilitySupport || !block4.Block.blockMaterial.StabilitySupport)
		{
			return false;
		}
		return true;
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _bpResult, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _bpResult, _ea);
		if (_ea is EntityPlayerLocal entityPlayerLocal)
		{
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.BedrollPlaced, 1);
			entityPlayerLocal.selectedSpawnPointKey = _ea.entityId;
			if (!SiblingBlock.isair && !isMultiBlock)
			{
				BlockValue siblingBlock = SiblingBlock;
				siblingBlock.rotation = _bpResult.blockValue.rotation;
				_world.SetBlockRPC(_bpResult.clrIdx, _bpResult.blockPos + rotationToAddVector(_bpResult.blockValue.rotation), siblingBlock);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs GetOwningPlayer(Vector3i _blockPos, out bool _ownedByOtherPlayer)
	{
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in GameManager.Instance.GetPersistentPlayerList().Players)
		{
			if (player.Value.BedrollPos.Equals(_blockPos))
			{
				_ownedByOtherPlayer = !player.Key.Equals(PlatformManager.InternalLocalUserIdentifier);
				return player.Key;
			}
		}
		_ownedByOtherPlayer = false;
		return null;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool _ownedByOtherPlayer;
		PlatformUserIdentifierAbs owningPlayer = GetOwningPlayer(_blockPos, out _ownedByOtherPlayer);
		if (!_ownedByOtherPlayer)
		{
			return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
		}
		return string.Format(Localization.Get("sleepingBagOwnership"), GetLocalizedBlockName(), GameUtils.SafeStringFormat(GameManager.Instance.GetPersistentPlayerList().GetPlayerData(owningPlayer).PlayerName.DisplayName));
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		GetOwningPlayer(_blockPos, out var _ownedByOtherPlayer);
		if (!_ownedByOtherPlayer)
		{
			return base.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		return false;
	}
}

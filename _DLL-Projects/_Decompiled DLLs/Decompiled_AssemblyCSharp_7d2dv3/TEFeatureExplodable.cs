using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureExplodable : TEFeatureAbs, IFeatureTriggerCapability
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosionData;

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		DynamicProperties props = _featureData.Props;
		explosionData = new ExplosionData(props);
		if (explosionData.ParticleIndex == 0)
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " has feature TEFeatureExplodable but no property ParticleIndex");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		Explode(_blockPos, _blockValue);
	}

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		_blockValue.rotation = (byte)(GameManager.Instance.World.GetGameRandom().RandomFloat * 4f);
	}

	public override void OnBlockStartsToFall(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockStartsToFall(_blockPos, _blockValue);
		Explode(_blockPos, _blockValue);
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		base.OnBlockDestroyedBy(_blockPos, _blockValue, _entityId, _bUseHarvestTool);
		if (!_bUseHarvestTool)
		{
			Explode(_blockPos, _blockValue);
			return Block.DestroyedResult.Remove;
		}
		return Block.DestroyedResult.None;
	}

	public override Block.DestroyedResult OnBlockDestroyedByExplosion(Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		base.OnBlockDestroyedByExplosion(_blockPos, _blockValue, _playerThatStartedExpl);
		Explode(_blockPos, _blockValue);
		return Block.DestroyedResult.Remove;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			_br.ReadUInt16();
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Explode(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (_blockValue.Block != null)
		{
			Vector3 worldPos = _blockPos.ToVector3();
			Quaternion rotation = Quaternion.identity;
			BlockEntityData blockEntity = GameManager.Instance.World.ChunkCache.GetBlockEntity(_blockPos);
			if (blockEntity != null && (bool)blockEntity.transform)
			{
				worldPos = blockEntity.transform.position + Origin.position;
				rotation = blockEntity.transform.rotation;
			}
			GameManager.Instance.ExplosionServer(worldPos, _blockPos, rotation, explosionData, -1, 0.1f, _bRemoveBlockAtExplPosition: false);
		}
	}
}

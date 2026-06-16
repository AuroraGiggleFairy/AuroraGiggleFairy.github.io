using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureAreaRepair : TEFeatureAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureStorage storageFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRepairing;

	public bool IsRepairing
	{
		get
		{
			return isRepairing;
		}
		set
		{
			isRepairing = value;
		}
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		storageFeature = base.Parent.GetFeature<TEFeatureStorage>();
		lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (_module is TEFeatureStorage)
		{
			return false;
		}
		if (!Equals(_module))
		{
			return true;
		}
		if (lockFeature != null && !GameManager.Instance.IsEditMode() && lockFeature.IsLocked() && !lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return false;
		}
		return base.Parent.LocalPlayerIsOwner;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.Parent.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.Parent.GetLegacyForkVersion();
			}
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)18);
		}
	}

	public void RepairAll(World _world, Vector3i _blockPos, int _requesterId)
	{
		GameManager.Instance.StartCoroutine(repair(_world, _blockPos, _requesterId));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repair(World _world, Vector3i _blockPos, int _requesterId)
	{
		Vector2i blocksMin = new Vector2i(_blockPos.x - 22, _blockPos.z - 22);
		Vector2i blocksMax = new Vector2i(blocksMin.x + 44, blocksMin.y + 44);
		Vector2i v = new Vector2i(blocksMin);
		Vector2i v2 = new Vector2i(blocksMax);
		Vector2i startChunk = World.toChunkXZ(v);
		Vector2i endChunk = World.toChunkXZ(v2);
		for (int cx = startChunk.x; cx <= endChunk.x; cx++)
		{
			yield return repairChunkCheckBounds(_world, cx, startChunk.y, _world, blocksMin, blocksMax);
			yield return repairChunkCheckBounds(_world, cx, endChunk.y, _world, blocksMin, blocksMax);
		}
		for (int cz = startChunk.y; cz <= endChunk.y - 1; cz++)
		{
			yield return repairChunkCheckBounds(_world, startChunk.x, cz, _world, blocksMin, blocksMax);
			yield return repairChunkCheckBounds(_world, endChunk.x, cz, _world, blocksMin, blocksMax);
		}
		for (int cz = startChunk.y + 1; cz <= endChunk.y - 1; cz++)
		{
			for (int cx = startChunk.x + 1; cx <= endChunk.x - 1; cx++)
			{
				yield return repairChunk(_world, cx, cz, _world);
			}
		}
		EntityPlayer primaryPlayer = _world.GetPrimaryPlayer();
		if (primaryPlayer != null && _requesterId == primaryPlayer.entityId)
		{
			IsRepairing = false;
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageLandClaimRepair>().Setup(_blockPos, _beginRepair: false), _onlyClientsAttachedToAnEntity: false, _requesterId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repairChunkCheckBounds(World _world, int cx, int cz, World world, Vector2i blocksMin, Vector2i blocksMax)
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
				repairBlock(_world, chunk.worldPosIMin + new Vector3i(x, y, z), bv);
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repairChunk(World _world, int cx, int cz, World world)
	{
		Chunk chunk = (Chunk)world.GetChunkSync(cx, cz);
		if (chunk == null)
		{
			yield break;
		}
		yield return chunk.LoopOverAllBlocksCoroutine([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
		{
			if (!bv.isair && bv.damage > 0)
			{
				repairBlock(_world, chunk.worldPosIMin + new Vector3i(x, y, z), bv);
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void repairBlock(World _world, Vector3i worldPos, BlockValue bv)
	{
		if (storageFeature == null)
		{
			return;
		}
		Block block = bv.Block;
		int num = bv.damage;
		float num2 = (float)num / (float)block.MaxDamage;
		List<Block.SItemNameCount> list = block.RepairItems;
		if (block.RepairItemsMeshDamage != null && block.shape.UseRepairDamageState(bv))
		{
			num = 1;
			num2 = 1f;
			list = block.RepairItemsMeshDamage;
		}
		if (list == null)
		{
			return;
		}
		float resourceScale = block.ResourceScale;
		bool flag = true;
		List<ItemStack> list2 = new List<ItemStack>();
		for (int i = 0; i < list.Count; i++)
		{
			string itemName = list[i].ItemName;
			ItemStack itemStack = new ItemStack(_count: Utils.FastMax((int)((float)list[i].Count * num2 * resourceScale), 1), _itemValue: ItemClass.GetItem(itemName));
			if (storageFeature.CountItem(itemStack.itemValue.ItemClass) >= itemStack.count)
			{
				list2.Add(itemStack);
				continue;
			}
			flag = false;
			break;
		}
		if (flag)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				ItemStack itemStack2 = list2[j];
				storageFeature.RemoveItems(itemStack2.itemValue, itemStack2.count);
			}
			bv.Block.DamageBlock(_world, worldPos, bv, -num, -1);
		}
	}
}

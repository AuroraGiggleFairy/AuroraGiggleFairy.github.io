using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLandClaimRepair : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool beginRepair;

	public NetPackageLandClaimRepair Setup(Vector3i _blockPosition, bool _beginRepair)
	{
		blockPosition = _blockPosition;
		beginRepair = _beginRepair;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		long num = _br.ReadInt64();
		long num2 = _br.ReadInt64();
		long num3 = _br.ReadInt64();
		blockPosition = new Vector3i(num, num2, num3);
		beginRepair = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((long)blockPosition.x);
		_bw.Write((long)blockPosition.y);
		_bw.Write((long)blockPosition.z);
		_bw.Write(beginRepair);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		TEFeatureAreaRepair selfOrFeature = _world.GetTileEntity(blockPosition).GetSelfOrFeature<TEFeatureAreaRepair>();
		if (selfOrFeature == null)
		{
			return;
		}
		if (beginRepair)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				selfOrFeature.RepairAll(_world, blockPosition, base.Sender.entityId);
			}
		}
		else if (selfOrFeature.Parent.Owner.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			selfOrFeature.IsRepairing = false;
		}
	}

	public override int GetLength()
	{
		return 25;
	}
}

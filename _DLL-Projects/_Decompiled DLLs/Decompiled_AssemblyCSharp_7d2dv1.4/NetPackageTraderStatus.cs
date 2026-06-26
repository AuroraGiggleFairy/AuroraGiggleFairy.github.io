using UnityEngine.Scripting;

[Preserve]
public class NetPackageTraderStatus : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int traderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	public NetPackageTraderStatus Setup(int _traderId, bool _isOpen = false)
	{
		traderId = _traderId;
		isOpen = _isOpen;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		traderId = _br.ReadInt32();
		isOpen = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(traderId);
		_bw.Write(isOpen);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityTrader entityTrader = GameManager.Instance.World.GetEntity(traderId) as EntityTrader;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageTraderStatus package = NetPackageManager.GetPackage<NetPackageTraderStatus>();
			package.Setup(traderId, entityTrader.traderArea == null || !entityTrader.traderArea.IsClosed);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, base.Sender.entityId);
		}
		else
		{
			entityTrader.ActivateTrader(isOpen);
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}

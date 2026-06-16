using UnityEngine.Scripting;

[Preserve]
public class NetPackageTraderData : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i tePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public TraderData traderData;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageTraderData Setup(ITrader _trader)
	{
		if (_trader is EntityTrader entityTrader)
		{
			return Setup(entityTrader);
		}
		if (_trader is TileEntityVendingMachine vendingMachine)
		{
			return Setup(vendingMachine);
		}
		entityId = -1;
		tePosition = Vector3i.zero;
		traderData = null;
		return this;
	}

	public NetPackageTraderData Setup(EntityTrader _entityTrader)
	{
		entityId = _entityTrader.entityId;
		tePosition = Vector3i.zero;
		traderData = _entityTrader.TraderData?.Clone();
		return this;
	}

	public NetPackageTraderData Setup(TileEntityVendingMachine _vendingMachine)
	{
		entityId = -1;
		tePosition = _vendingMachine.ToWorldPos();
		traderData = _vendingMachine.TraderData?.Clone();
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		bool num = _br.ReadBoolean();
		entityId = -1;
		tePosition = Vector3i.zero;
		if (num)
		{
			entityId = _br.ReadInt32();
		}
		else
		{
			tePosition = StreamUtils.ReadVector3i(_br);
		}
		if (_br.ReadBoolean())
		{
			if (traderData == null)
			{
				traderData = new TraderData();
			}
			traderData.Read(_br);
		}
		else
		{
			traderData = null;
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		bool flag = entityId != -1;
		_bw.Write(flag);
		if (flag)
		{
			_bw.Write(entityId);
		}
		else
		{
			StreamUtils.Write(_bw, tePosition);
		}
		_bw.Write(traderData != null);
		if (traderData != null)
		{
			traderData.Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || traderData == null || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (entityId != -1)
		{
			EntityTrader entityTrader = _world.GetEntity(entityId) as EntityTrader;
			if (!(entityTrader == null))
			{
				entityTrader.TraderData.CopyFrom(traderData);
			}
		}
		else if (_world.GetTileEntity(tePosition) is TileEntityVendingMachine tileEntityVendingMachine)
		{
			tileEntityVendingMachine.TraderData.CopyFrom(traderData);
			tileEntityVendingMachine.NotifyListeners();
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}

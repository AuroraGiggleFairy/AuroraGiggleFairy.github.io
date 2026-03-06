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
		if (!(GameManager.Instance.World.GetEntity(traderId) is EntityTrader entityTrader))
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (QuestEventManager.Current.GetQuestList(_world, traderId, base.Sender.entityId) == null && GameManager.Instance.World.GetEntity(base.Sender.entityId) is EntityPlayer player)
			{
				entityTrader.SetupActiveQuestsForPlayer(player);
			}
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

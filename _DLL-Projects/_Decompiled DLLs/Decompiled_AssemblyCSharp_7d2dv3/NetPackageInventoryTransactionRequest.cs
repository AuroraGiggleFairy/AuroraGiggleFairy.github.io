using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryTransactionRequest : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public InventoryTransaction tx;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageInventoryTransactionRequest Setup(InventoryTransaction _tx)
	{
		tx = _tx;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		tx = InventoryTransaction.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		InventoryTransaction.Write(_bw, tx);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		InventoryManager.Instance.TransactionRequestServer(tx, base.Sender.entityId);
	}

	public override int GetLength()
	{
		return 0;
	}
}

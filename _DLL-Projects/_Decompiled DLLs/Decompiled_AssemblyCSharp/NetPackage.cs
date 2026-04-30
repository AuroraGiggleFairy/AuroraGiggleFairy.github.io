using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public abstract class NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int inSendQueuesCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string classnameCached;

	public virtual int Channel => 0;

	public virtual bool Compress => false;

	public virtual bool FlushQueue => false;

	public virtual NetPackageDirection PackageDirection => NetPackageDirection.Both;

	public virtual bool AllowedBeforeAuth => false;

	public int PackageId => NetPackageManager.GetPackageId(GetType());

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ClientInfo Sender { get; set; }

	public virtual bool ReliableDelivery => true;

	public abstract void read(PooledBinaryReader _reader);

	public virtual void write(PooledBinaryWriter _writer)
	{
		_writer.Write((ushort)PackageId);
	}

	public abstract void ProcessPackage(World _world, GameManager _callbacks);

	public abstract int GetLength();

	public override string ToString()
	{
		return classnameCached ?? (classnameCached = GetType().Name);
	}

	public void RegisterSendQueue()
	{
		Interlocked.Increment(ref inSendQueuesCount);
	}

	public void SendQueueHandled()
	{
		if (Interlocked.Decrement(ref inSendQueuesCount) == 0)
		{
			NetPackageManager.FreePackage(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ValidEntityIdForSender(int _entityId, bool _allowAttachedToEntity = false)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return true;
		}
		if (_entityId == Sender.entityId)
		{
			return true;
		}
		if (_allowAttachedToEntity)
		{
			EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(Sender.entityId) as EntityPlayer;
			if (entityPlayer != null && entityPlayer.AttachedToEntity != null && entityPlayer.AttachedToEntity.entityId == _entityId)
			{
				return true;
			}
		}
		Log.Warning($"Received {ToString()} with invalid entityId {_entityId} from {Sender}");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ValidUserIdForSender(PlatformUserIdentifierAbs _userId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return true;
		}
		if (object.Equals(_userId, Sender.PlatformId) || object.Equals(_userId, Sender.CrossplatformId))
		{
			return true;
		}
		Log.Warning($"Received {ToString()} with invalid userId {_userId} from {Sender}");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public NetPackage()
	{
	}
}

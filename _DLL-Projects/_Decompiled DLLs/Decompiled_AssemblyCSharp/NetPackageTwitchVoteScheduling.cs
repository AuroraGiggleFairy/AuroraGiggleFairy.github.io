using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTwitchVoteScheduling : NetPackage
{
	public NetPackageTwitchVoteScheduling Setup()
	{
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				TwitchVoteScheduler.Current.AddParticipant(base.Sender.entityId);
			}
			else
			{
				TwitchManager.Current.VotingManager.RequestApprovedToStart();
			}
		}
	}

	public override int GetLength()
	{
		return 30;
	}
}

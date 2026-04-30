using System.Collections.Generic;

namespace Twitch;

public class TwitchVoteScheduler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static TwitchVoteScheduler instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> votingParticipants = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextVoteTime;

	public static TwitchVoteScheduler Current
	{
		get
		{
			if (instance == null)
			{
				instance = new TwitchVoteScheduler();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVoteScheduler()
	{
	}

	public void Cleanup()
	{
		ClearParticipants();
		instance = null;
	}

	public void AddParticipant(int entityID)
	{
		if (!votingParticipants.Contains(entityID))
		{
			votingParticipants.Add(entityID);
		}
	}

	public void ClearParticipants()
	{
		votingParticipants.Clear();
	}

	public void Init()
	{
	}

	public void Update(float deltaTime)
	{
		if (GameManager.Instance.World == null || GameManager.Instance.World.Players == null || GameManager.Instance.World.Players.Count == 0)
		{
			return;
		}
		if (nextVoteTime > 0f)
		{
			nextVoteTime -= deltaTime;
		}
		if (votingParticipants.Count != 0 && nextVoteTime <= 0f)
		{
			if (GameManager.Instance.World.GetPrimaryPlayerId() == votingParticipants[0])
			{
				TwitchManager.Current.VotingManager.RequestApprovedToStart();
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTwitchVoteScheduling>().Setup(), _onlyClientsAttachedToAnEntity: false, votingParticipants[0]);
			}
			votingParticipants.RemoveAt(0);
			nextVoteTime = 3f;
		}
	}
}

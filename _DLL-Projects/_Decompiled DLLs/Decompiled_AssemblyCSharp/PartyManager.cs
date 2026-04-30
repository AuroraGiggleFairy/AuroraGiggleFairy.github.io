using System.Collections.Generic;

public class PartyManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PartyManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PartyVoice voice;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextPartyID;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Party> partyList = new List<Party>();

	public static PartyManager Current => instance ?? (instance = new PartyManager());

	public static bool HasInstance => instance != null;

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyManager()
	{
		voice = PartyVoice.Instance;
	}

	public Party CreateParty()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return null;
		}
		Party party = new Party
		{
			PartyID = ++nextPartyID
		};
		partyList.Add(party);
		return party;
	}

	public Party CreateClientParty(World world, int partyID, int leaderIndex, int[] partyMembers, string voiceLobbyId)
	{
		Party party = new Party
		{
			PartyID = partyID,
			LeaderIndex = leaderIndex,
			VoiceLobbyId = voiceLobbyId
		};
		partyList.Add(party);
		party.UpdateMemberList(world, partyMembers);
		return party;
	}

	public void RemoveParty(Party party)
	{
		if (partyList.Contains(party))
		{
			partyList.Remove(party);
		}
	}

	public Party GetParty(int partyID)
	{
		for (int i = 0; i < partyList.Count; i++)
		{
			if (partyList[i].PartyID == partyID)
			{
				return partyList[i];
			}
		}
		return null;
	}

	public void Cleanup()
	{
		partyList.Clear();
		nextPartyID = 0;
	}

	public void Update()
	{
		voice.Update();
	}
}

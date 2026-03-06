using System.Collections.Generic;
using UnityEngine;

namespace Twitch;

public class TwitchVoteGroup
{
	public string Name = "";

	public List<TwitchVoteType> VoteTypes = new List<TwitchVoteType>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	public bool SkippedThisVote;

	public TwitchVoteGroup(string name)
	{
		Name = name;
	}

	public TwitchVoteType GetNextVoteType()
	{
		index++;
		if (index >= VoteTypes.Count)
		{
			index = 0;
		}
		return VoteTypes[index];
	}

	public void ShuffleVoteTypes()
	{
		for (int i = 0; i <= VoteTypes.Count * VoteTypes.Count; i++)
		{
			int num = Random.Range(0, VoteTypes.Count);
			int num2 = Random.Range(0, VoteTypes.Count);
			if (num != num2)
			{
				TwitchVoteType value = VoteTypes[num];
				VoteTypes[num] = VoteTypes[num2];
				VoteTypes[num2] = value;
			}
		}
	}
}

using System.Collections.Generic;

namespace Challenges;

public class ChallengeGroupEntry
{
	public ChallengeGroup ChallengeGroup;

	public int LastUpdateDay = -1;

	public ChallengeGroupEntry(ChallengeGroup group)
	{
		ChallengeGroup = group;
	}

	public void CreateChallenges(EntityPlayer player)
	{
		ResetChallenges(player);
		if (ChallengeGroup.DayReset != -1)
		{
			LastUpdateDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) + ChallengeGroup.DayReset;
		}
	}

	public void Update(int day, EntityPlayer player)
	{
		if (ChallengeGroup.DayReset != -1 && LastUpdateDay <= day)
		{
			ResetChallenges(player);
			LastUpdateDay = day + ChallengeGroup.DayReset;
		}
	}

	public void AddAnyMissingChallenges(EntityPlayer player)
	{
		ChallengeJournal challengeJournal = player.challengeJournal;
		if (ChallengeGroup.IsRandom)
		{
			return;
		}
		int activeChallengeCount = ChallengeGroup.ActiveChallengeCount;
		for (int i = 0; i < ChallengeGroup.ChallengeClasses.Count && i < activeChallengeCount; i++)
		{
			if (!challengeJournal.ChallengeDictionary.ContainsKey(ChallengeGroup.ChallengeClasses[i].Name))
			{
				Challenge challenge = ChallengeGroup.ChallengeClasses[i].CreateChallenge(challengeJournal);
				challenge.ChallengeGroup = ChallengeGroup;
				if (challengeJournal.Challenges.Count == 0)
				{
					challenge.IsTracked = true;
				}
				challengeJournal.AddChallenge(challenge);
			}
		}
	}

	public void ResetChallenges(EntityPlayer player)
	{
		ChallengeJournal challengeJournal = player.challengeJournal;
		if (ChallengeGroup.IsRandom)
		{
			challengeJournal.RemoveChallengesForGroup(ChallengeGroup);
			int activeChallengeCount = ChallengeGroup.ActiveChallengeCount;
			List<ChallengeClass> challengeClassesForCreate = ChallengeGroup.GetChallengeClassesForCreate();
			for (int i = 0; i < challengeClassesForCreate.Count && i < activeChallengeCount; i++)
			{
				Challenge challenge = challengeClassesForCreate[i].CreateChallenge(challengeJournal);
				challenge.ChallengeGroup = ChallengeGroup;
				challengeJournal.AddChallenge(challenge);
				challenge.StartChallenge();
				if (challenge.IsTracked)
				{
					LocalPlayerUI.GetUIForPrimaryPlayer().xui.QuestTracker.TrackedChallenge = challenge;
				}
			}
			return;
		}
		int activeChallengeCount2 = ChallengeGroup.ActiveChallengeCount;
		for (int j = 0; j < ChallengeGroup.ChallengeClasses.Count && j < activeChallengeCount2; j++)
		{
			Challenge challenge2 = ChallengeGroup.ChallengeClasses[j].CreateChallenge(challengeJournal);
			challenge2.ChallengeGroup = ChallengeGroup;
			if (challengeJournal.Challenges.Count == 0)
			{
				challenge2.IsTracked = true;
			}
			challengeJournal.AddChallenge(challenge2);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Audio;
using Challenges;
using UniLinq;
using UnityEngine;

public class ChallengeJournal
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCurrentSaveVersion = 2;

	public List<ChallengeGroupEntry> ChallengeGroups = new List<ChallengeGroupEntry>();

	public Dictionary<string, Challenge> ChallengeDictionary = new Dictionary<string, Challenge>();

	public List<Challenge> Challenges = new List<Challenge>();

	public List<Challenge> CompleteChallengesForMinEvents = new List<Challenge>();

	public List<ChallengeGroup> CompleteChallengeGroupsForMinEvents = new List<ChallengeGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ChallengeClass> eventList = new Dictionary<string, ChallengeClass>();

	public EntityPlayerLocal Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdateTime;

	public void Read(BinaryReader _br)
	{
		SetupData();
		if (_br.ReadByte() == 1)
		{
			return;
		}
		int num = _br.ReadInt32();
		Challenges.Clear();
		ChallengeDictionary.Clear();
		CompleteChallengesForMinEvents.Clear();
		for (int i = 0; i < num; i++)
		{
			Challenge challenge = new Challenge();
			challenge.Owner = this;
			challenge.Read(_br);
			if (challenge.ResetToChallengeClass())
			{
				if (challenge.ChallengeState == Challenge.ChallengeStates.Redeemed && eventList.ContainsKey(challenge.ChallengeClass.Name))
				{
					CompleteChallengesForMinEvents.Add(challenge);
				}
				ChallengeDictionary.Add(challenge.ChallengeClass.Name, challenge);
			}
		}
		Challenges = ChallengeDictionary.Values.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (Challenge c) => c.ChallengeClass.OrderIndex).ToList();
		if (ChallengeGroups.Count == 0 && !GameManager.Instance.World.IsEditor() && !GameUtils.IsWorldEditor() && !GameUtils.IsPlaytesting())
		{
			foreach (ChallengeGroup value in ChallengeGroup.s_ChallengeGroups.Values)
			{
				ChallengeGroupEntry item = new ChallengeGroupEntry(value);
				ChallengeGroups.Add(item);
			}
		}
		num = _br.ReadInt32();
		for (int num2 = 0; num2 < num; num2++)
		{
			string text = _br.ReadString();
			int lastUpdateDay = _br.ReadInt32();
			for (int num3 = 0; num3 < ChallengeGroups.Count; num3++)
			{
				if (ChallengeGroups[num3].ChallengeGroup.Name == text)
				{
					ChallengeGroups[num3].LastUpdateDay = lastUpdateDay;
				}
			}
		}
		string text2 = _br.ReadString();
		if (text2 != "" && ChallengeDictionary.ContainsKey(text2))
		{
			ChallengeDictionary[text2].IsTracked = true;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		string value = "";
		_bw.Write((byte)2);
		_bw.Write(Challenges.Count);
		for (int i = 0; i < Challenges.Count; i++)
		{
			Challenge challenge = Challenges[i];
			challenge.Write(_bw);
			if (challenge.IsTracked)
			{
				value = challenge.ChallengeClass.Name;
			}
		}
		int num = 0;
		for (int j = 0; j < ChallengeGroups.Count; j++)
		{
			if (ChallengeGroups[j].LastUpdateDay != -1)
			{
				num++;
			}
		}
		_bw.Write(num);
		for (int k = 0; k < ChallengeGroups.Count; k++)
		{
			if (ChallengeGroups[k].LastUpdateDay != -1)
			{
				_bw.Write(ChallengeGroups[k].ChallengeGroup.Name);
				_bw.Write(ChallengeGroups[k].LastUpdateDay);
			}
		}
		_bw.Write(value);
	}

	public ChallengeJournal Clone()
	{
		ChallengeJournal challengeJournal = new ChallengeJournal();
		challengeJournal.Player = Player;
		for (int i = 0; i < ChallengeGroups.Count; i++)
		{
			challengeJournal.ChallengeGroups.Add(ChallengeGroups[i]);
		}
		for (int j = 0; j < Challenges.Count; j++)
		{
			Challenge challenge = Challenges[j].Clone();
			challengeJournal.ChallengeDictionary.Add(challenge.ChallengeClass.Name, challenge);
			challengeJournal.Challenges.Add(challenge);
		}
		return challengeJournal;
	}

	public void Update(World world)
	{
		int num = GameUtils.WorldTimeToDays(world.worldTime);
		if (lastDay < num)
		{
			for (int i = 0; i < ChallengeGroups.Count; i++)
			{
				ChallengeGroups[i].Update(num, Player);
			}
			lastDay = num;
		}
		if (Time.time - lastUpdateTime >= 1f)
		{
			FireEvent(MinEventTypes.onSelfChallengeCompleteUpdate, Player.MinEventContext);
			lastUpdateTime = Time.time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupData()
	{
		eventList.Clear();
		foreach (ChallengeClass value in ChallengeClass.s_Challenges.Values)
		{
			if (value.HasEventsOrPassives())
			{
				eventList.Add(value.Name, value);
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (eventList != null)
		{
			for (int i = 0; i < CompleteChallengesForMinEvents.Count; i++)
			{
				Challenge challenge = CompleteChallengesForMinEvents[i];
				ChallengeClass challengeClass = challenge.ChallengeClass;
				_params.Challenge = challenge;
				challengeClass.FireEvent(_eventType, _params);
			}
		}
	}

	public void ModifyValue(PassiveEffects _effect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> _tags)
	{
		for (int i = 0; i < CompleteChallengesForMinEvents.Count; i++)
		{
			Challenge challenge = CompleteChallengesForMinEvents[i];
			if (challenge == null)
			{
				continue;
			}
			ChallengeClass challengeClass = challenge.ChallengeClass;
			if (challengeClass == null)
			{
				continue;
			}
			MinEffectController effects = challengeClass.Effects;
			if (effects != null)
			{
				HashSet<PassiveEffects> passivesIndex = effects.PassivesIndex;
				if (passivesIndex != null && passivesIndex.Contains(_effect))
				{
					challengeClass.ModifyValue(Player, _effect, ref _base_val, ref _perc_val, _tags);
				}
			}
		}
		for (int j = 0; j < CompleteChallengeGroupsForMinEvents.Count; j++)
		{
			ChallengeGroup challengeGroup = CompleteChallengeGroupsForMinEvents[j];
			if (challengeGroup == null)
			{
				continue;
			}
			MinEffectController effects2 = challengeGroup.Effects;
			if (effects2 != null)
			{
				HashSet<PassiveEffects> passivesIndex2 = effects2.PassivesIndex;
				if (passivesIndex2 != null && passivesIndex2.Contains(_effect))
				{
					challengeGroup.ModifyValue(Player, _effect, ref _base_val, ref _perc_val, _tags);
				}
			}
		}
	}

	public void StartChallenges(EntityPlayerLocal player)
	{
		if (Player == null)
		{
			Player = player;
		}
		if (Player == null)
		{
			return;
		}
		if (ChallengeGroups.Count == 0)
		{
			foreach (ChallengeGroup value in ChallengeGroup.s_ChallengeGroups.Values)
			{
				ChallengeGroupEntry challengeGroupEntry = new ChallengeGroupEntry(value);
				ChallengeGroups.Add(challengeGroupEntry);
				value.IsComplete = true;
				challengeGroupEntry.CreateChallenges(Player);
			}
		}
		else
		{
			int num = 0;
			foreach (ChallengeGroup value2 in ChallengeGroup.s_ChallengeGroups.Values)
			{
				value2.IsComplete = true;
				if (num < ChallengeGroups.Count)
				{
					ChallengeGroupEntry challengeGroupEntry2 = ChallengeGroups[num];
					if (challengeGroupEntry2.ChallengeGroup == value2)
					{
						challengeGroupEntry2.AddAnyMissingChallenges(Player);
					}
				}
				num++;
			}
		}
		for (int i = 0; i < Challenges.Count; i++)
		{
			Challenge challenge = Challenges[i];
			challenge.StartChallenge();
			if (challenge.ChallengeState != Challenge.ChallengeStates.Redeemed)
			{
				challenge.ChallengeGroup.IsComplete = false;
			}
			if (challenge.IsTracked)
			{
				LocalPlayerUI.GetUIForPrimaryPlayer().xui.QuestTracker.TrackedChallenge = challenge;
			}
		}
		foreach (ChallengeGroup value3 in ChallengeGroup.s_ChallengeGroups.Values)
		{
			if (value3.IsComplete)
			{
				CompleteChallengeGroupsForMinEvents.Add(value3);
			}
		}
	}

	public void EndChallenges()
	{
		for (int i = 0; i < Challenges.Count; i++)
		{
			Challenges[i].EndChallenge(isCompleted: false);
		}
	}

	public void AddChallenge(Challenge challenge)
	{
		ChallengeDictionary.Add(challenge.ChallengeClass.Name, challenge);
		Challenges.Add(challenge);
	}

	public void RemoveChallengesForGroup(ChallengeGroup challengeGroup)
	{
		for (int num = Challenges.Count - 1; num >= 0; num--)
		{
			Challenge challenge = Challenges[num];
			if (challenge.ChallengeGroup == challengeGroup)
			{
				challenge.EndChallenge(isCompleted: false);
				ChallengeDictionary.Remove(challenge.ChallengeClass.Name);
				Challenges.RemoveAt(num);
			}
		}
	}

	public void ResetChallenges()
	{
		if (!GameManager.Instance.World.IsEditor() && !GameUtils.IsWorldEditor() && !GameUtils.IsPlaytesting())
		{
			EndChallenges();
			ChallengeDictionary.Clear();
			Challenges.Clear();
			ChallengeGroups.Clear();
			CompleteChallengesForMinEvents.Clear();
			CompleteChallengeGroupsForMinEvents.Clear();
			StartChallenges(Player);
		}
	}

	public void HandleChallengeRedeemed(Challenge challenge)
	{
		if (eventList.ContainsKey(challenge.ChallengeClass.Name))
		{
			CompleteChallengesForMinEvents.Add(challenge);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void HandleChallengeGroupComplete(ChallengeGroup group)
	{
		for (int i = 0; i < Challenges.Count; i++)
		{
			Challenge challenge = Challenges[i];
			if (challenge.ChallengeGroup == group && challenge.ChallengeState != Challenge.ChallengeStates.Redeemed)
			{
				group.IsComplete = false;
				Manager.PlayInsidePlayerHead("ui_challenge_redeem");
				return;
			}
		}
		if (group.RewardEvent != null)
		{
			GameEventManager.Current.HandleAction(group.RewardEvent, null, Player, twitchActivated: false);
		}
		Manager.PlayInsidePlayerHead("ui_challenge_complete_row");
		group.IsComplete = true;
		if (!CompleteChallengeGroupsForMinEvents.Contains(group))
		{
			CompleteChallengeGroupsForMinEvents.Add(group);
		}
		GameManager.Instance.StartCoroutine(unhideRowLater(group));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator unhideRowLater(ChallengeGroup group)
	{
		yield return new WaitForSeconds(1f);
		bool flag = false;
		foreach (ChallengeGroup value in ChallengeGroup.s_ChallengeGroups.Values)
		{
			if (value.HiddenBy.EqualsCaseInsensitive(group.Name))
			{
				value.UIDirty = true;
				flag = true;
			}
		}
		if (flag)
		{
			Manager.PlayInsidePlayerHead("ui_challenge_unhide_row");
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public Challenge GetNextChallenge(Challenge challenge)
	{
		Challenge challenge2 = null;
		string name = challenge.ChallengeGroup.ChallengeClasses[0].Name;
		if (name != "" && ChallengeDictionary.ContainsKey(name))
		{
			challenge2 = ChallengeDictionary[name];
		}
		while (challenge2 != null && !challenge2.IsActive)
		{
			name = challenge2.ChallengeClass.GetNextChallengeName();
			challenge2 = ((!(name != "") || !ChallengeDictionary.ContainsKey(name)) ? null : ChallengeDictionary[name]);
		}
		return challenge2;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public Challenge GetNextRedeemableChallenge(Challenge challenge)
	{
		Challenge challenge2 = null;
		string name = challenge.ChallengeGroup.ChallengeClasses[0].Name;
		if (name != "" && ChallengeDictionary.ContainsKey(name))
		{
			challenge2 = ChallengeDictionary[name];
		}
		while (challenge2 != null && !challenge2.ReadyToComplete)
		{
			name = challenge2.ChallengeClass.GetNextChallengeName();
			challenge2 = ((!(name != "") || !ChallengeDictionary.ContainsKey(name)) ? null : ChallengeDictionary[name]);
		}
		return challenge2;
	}

	public bool HasCompletedChallenges()
	{
		for (int i = 0; i < Challenges.Count; i++)
		{
			if (Challenges[i].ReadyToComplete)
			{
				return true;
			}
		}
		return false;
	}
}

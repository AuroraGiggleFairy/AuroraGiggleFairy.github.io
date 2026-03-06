using System;
using System.Collections.Generic;
using Audio;
using GameEvent.SequenceActions;
using UnityEngine;

namespace GameEvent.GameEventHelpers;

public class HomerunData
{
	public class ScoreDisplay
	{
		public int Score;

		public NavObject NavObject;

		public float TimeRemaining = 3f;

		public HomerunData Owner;

		public ScoreDisplay(int score, Vector3 position, Color color)
		{
			NavObject = NavObjectManager.Instance.RegisterNavObject("twitch_score", position);
			NavObject.IsActive = true;
			NavObject.name = score.ToString();
			NavObject.UseOverrideFontColor = true;
			NavObject.OverrideColor = color;
		}

		public bool Update(float deltaTime)
		{
			TimeRemaining -= deltaTime;
			if (TimeRemaining <= 0f)
			{
				RemoveNavObject();
				return false;
			}
			return true;
		}

		public void Cleanup()
		{
			if (NavObject != null)
			{
				RemoveNavObject();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RemoveNavObject()
		{
			Owner.RemoveScoreDisplay(NavObject.TrackedPosition);
			NavObjectManager.Instance.UnRegisterNavObject(NavObject);
			NavObject = null;
		}
	}

	public List<EntityHomerunGoal> GoalControllers = new List<EntityHomerunGoal>();

	public EntityPlayer Player;

	public List<EntityPlayer> BuffedPlayers;

	public HomerunManager Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> rewardLevels;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> rewardEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> entityIDs = new List<int>();

	public float timeRemaining = 120f;

	public int ExpectedCount = 3;

	public Action CompleteCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentScoreIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int score;

	public List<ScoreDisplay> ScoreDisplays = new List<ScoreDisplay>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public float createTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom gr;

	public int Score
	{
		get
		{
			return score;
		}
		set
		{
			score = value;
			currentScoreIndex = GetRewardIndex(currentScoreIndex, value);
		}
	}

	public HomerunData(EntityPlayer player, float gameTime, string goalEntityNames, List<int> rewardLevels, List<string> rewardEvents, HomerunManager manager, Action completeCallback)
	{
		Player = player;
		Owner = manager;
		this.rewardLevels = rewardLevels;
		this.rewardEvents = rewardEvents;
		CompleteCallback = completeCallback;
		if (player.IsInParty())
		{
			BuffedPlayers = new List<EntityPlayer>();
			for (int i = 0; i < player.Party.MemberList.Count; i++)
			{
				EntityPlayer entityPlayer = player.Party.MemberList[i];
				if (!entityPlayer.Buffs.HasBuff("twitch_buffHomeRun"))
				{
					entityPlayer.Buffs.AddBuff("twitch_buffHomeRun");
				}
				if (player != entityPlayer)
				{
					BuffedPlayers.Add(entityPlayer);
				}
			}
		}
		else if (!player.Buffs.HasBuff("twitch_buffHomeRun"))
		{
			player.Buffs.AddBuff("twitch_buffHomeRun");
		}
		gr = GameEventManager.Current.Random;
		timeRemaining = gameTime;
		SetupEntityIDs(goalEntityNames);
		world = GameManager.Instance.World;
	}

	public void SetupEntityIDs(string entityNames)
	{
		string[] array = entityNames.Split(',');
		entityIDs.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			foreach (KeyValuePair<int, EntityClass> item in EntityClass.list.Dict)
			{
				if (item.Value.entityClassName == array[i])
				{
					entityIDs.Add(item.Key);
					if (entityIDs.Count == array.Length)
					{
						break;
					}
				}
			}
		}
	}

	public bool Update(float deltaTime)
	{
		for (int num = ScoreDisplays.Count - 1; num >= 0; num--)
		{
			if (!ScoreDisplays[num].Update(deltaTime))
			{
				ScoreDisplays.RemoveAt(num);
			}
		}
		if (Player.IsDead())
		{
			return false;
		}
		if (BuffedPlayers != null)
		{
			for (int num2 = BuffedPlayers.Count - 1; num2 >= 0; num2--)
			{
				if (BuffedPlayers[num2].IsDead())
				{
					BuffedPlayers.RemoveAt(num2);
				}
			}
		}
		if (timeRemaining > 10f && timeRemaining - deltaTime < 10f)
		{
			if (!Player.Buffs.HasBuff("twitch_buffHomeRunEnding"))
			{
				Player.Buffs.AddBuff("twitch_buffHomeRunEnding");
			}
			if (BuffedPlayers != null)
			{
				for (int i = 0; i < BuffedPlayers.Count; i++)
				{
					if (!BuffedPlayers[i].Buffs.HasBuff("twitch_buffHomeRunEnding"))
					{
						BuffedPlayers[i].Buffs.AddBuff("twitch_buffHomeRunEnding");
					}
				}
			}
		}
		timeRemaining -= deltaTime;
		if (timeRemaining > 0f)
		{
			if (GoalControllers.Count < ExpectedCount)
			{
				createTime -= deltaTime;
				if (createTime <= 0f)
				{
					Vector3 newPoint = Vector3.zero;
					if (ActionBaseSpawn.FindValidPosition(out newPoint, Player, 6f, 12f, spawnInSafe: true, 1f, spawnInAir: true))
					{
						EntityHomerunGoal entityHomerunGoal = EntityFactory.CreateEntity(entityIDs[gr.RandomRange(entityIDs.Count)], newPoint, Vector3.zero, Player.entityId, "") as EntityHomerunGoal;
						entityHomerunGoal.SetSpawnerSource(EnumSpawnerSource.Dynamic);
						GameManager.Instance.World.SpawnEntityInWorld(entityHomerunGoal);
						entityHomerunGoal.StartPosition = newPoint;
						entityHomerunGoal.position = newPoint;
						entityHomerunGoal.direction = (EntityHomerunGoal.Direction)gr.RandomRange(5);
						Manager.BroadcastPlayByLocalPlayer(entityHomerunGoal.position, "twitch_balloon_spawn");
						entityHomerunGoal.Owner = this;
						GoalControllers.Add(entityHomerunGoal);
						createTime = 1f;
					}
				}
			}
			for (int num3 = GoalControllers.Count - 1; num3 >= 0; num3--)
			{
				EntityHomerunGoal entityHomerunGoal2 = GoalControllers[num3];
				if (GoalControllers[num3].ReadyForDelete)
				{
					Manager.BroadcastPlayByLocalPlayer(entityHomerunGoal2.position, "twitch_balloon_despawn");
					world.RemoveEntity(entityHomerunGoal2.entityId, EnumRemoveEntityReason.Killed);
					GoalControllers.RemoveAt(num3);
				}
			}
			return true;
		}
		int num4 = -1;
		for (int num5 = rewardLevels.Count - 1; num5 >= 0; num5--)
		{
			if (Score > rewardLevels[num5])
			{
				num4 = num5;
				break;
			}
		}
		if (num4 >= 0)
		{
			string text = string.Format(Localization.Get("ttTwitchHomerunScore"), Utils.ColorToHex(QualityInfo.GetTierColor(currentScoreIndex)), Score);
			GameManager.ShowTooltipMP(Player, text);
			GameEventManager.Current.HandleAction(rewardEvents[num4], Player, Player, twitchActivated: false);
			if (BuffedPlayers != null)
			{
				for (int j = 0; j < BuffedPlayers.Count; j++)
				{
					GameManager.ShowTooltipMP(BuffedPlayers[j], text);
					GameEventManager.Current.HandleAction(rewardEvents[num4], Player, BuffedPlayers[j], twitchActivated: false);
				}
			}
		}
		else
		{
			string text2 = Localization.Get("ttTwitchHomerunFailed");
			GameManager.ShowTooltipMP(Player, text2);
			if (BuffedPlayers != null)
			{
				for (int k = 0; k < BuffedPlayers.Count; k++)
				{
					GameManager.ShowTooltipMP(BuffedPlayers[k], text2);
				}
			}
		}
		return false;
	}

	public void Cleanup()
	{
		for (int num = ScoreDisplays.Count - 1; num >= 0; num--)
		{
			ScoreDisplays[num].Cleanup();
		}
		ScoreDisplays.Clear();
		for (int i = 0; i < GoalControllers.Count; i++)
		{
			if (GoalControllers[i] != null)
			{
				world.RemoveEntity(GoalControllers[i].entityId, EnumRemoveEntityReason.Killed);
			}
		}
		if (Player != null)
		{
			Player.Buffs.RemoveBuff("twitch_buffHomeRun");
		}
		if (BuffedPlayers != null)
		{
			for (int j = 0; j < BuffedPlayers.Count; j++)
			{
				BuffedPlayers[j].Buffs.RemoveBuff("twitch_buffHomeRun");
			}
		}
		GoalControllers.Clear();
	}

	public void AddScoreDisplay(Vector3 position)
	{
		Color tierColor = QualityInfo.GetTierColor(currentScoreIndex);
		if (Player.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", Score.ToString(), position, _isAdd: true, tierColor, _usingLocalizationId: false), _onlyClientsAttachedToAnEntity: false, Player.entityId);
		}
		if (BuffedPlayers != null)
		{
			for (int i = 0; i < BuffedPlayers.Count; i++)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", Score.ToString(), position, _isAdd: true, tierColor, _usingLocalizationId: false), _onlyClientsAttachedToAnEntity: false, BuffedPlayers[i].entityId);
			}
		}
		ScoreDisplays.Add(new ScoreDisplay(Score, position, tierColor)
		{
			Owner = this
		});
	}

	public void RemoveScoreDisplay(Vector3 position)
	{
		if (Player.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", "", position, _isAdd: false, _usingLocalizationId: false), _onlyClientsAttachedToAnEntity: false, Player.entityId);
		}
		if (BuffedPlayers != null)
		{
			for (int i = 0; i < BuffedPlayers.Count; i++)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup("twitch_score", "", position, _isAdd: false, _usingLocalizationId: false), _onlyClientsAttachedToAnEntity: false, BuffedPlayers[i].entityId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetRewardIndex(int currentIndex, int newScore)
	{
		for (int i = currentIndex + 1; i < rewardLevels.Count && newScore >= rewardLevels[i - 1]; i++)
		{
			currentIndex = i;
		}
		return currentIndex;
	}
}

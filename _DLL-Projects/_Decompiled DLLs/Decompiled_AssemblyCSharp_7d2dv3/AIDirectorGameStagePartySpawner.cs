using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorGameStagePartySpawner
{
	public ReadOnlyCollection<EntityPlayer> partyMembers;

	public float gsScaling;

	public int groupIndex;

	public int partyLevel;

	public int stageSpawnMax;

	public int bonusLootEvery;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameStageDefinition def;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameStageDefinition.Stage stage;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameStageDefinition.SpawnGroup spawnGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public int spawnCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public double interval;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextStageTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> memberIDs = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> members = new List<EntityPlayer>();

	public bool IsDone
	{
		get
		{
			if (groupIndex > 0)
			{
				return spawnGroup == null;
			}
			return false;
		}
	}

	public bool canSpawn
	{
		get
		{
			if (spawnGroup != null)
			{
				return spawnCount < numToSpawn;
			}
			return false;
		}
	}

	public int maxAlive
	{
		get
		{
			if (spawnGroup == null)
			{
				return 0;
			}
			return spawnGroup.maxAlive;
		}
	}

	public string spawnGroupName
	{
		get
		{
			if (spawnGroup == null)
			{
				return null;
			}
			return spawnGroup.groupName;
		}
	}

	public AIDirectorGameStagePartySpawner(World _world, string _gameStageName)
	{
		world = _world;
		def = GameStageDefinition.GetGameStage(_gameStageName);
		partyMembers = new ReadOnlyCollection<EntityPlayer>(members);
		partyLevel = -1;
		gsScaling = 1f;
	}

	public void SetScaling(float _scaling)
	{
		gsScaling = Utils.FastLerp(1f, 2.5f, (_scaling - 1f) / 3f);
	}

	public void ResetPartyLevel(int mod = 0)
	{
		int num = CalcPartyLevel();
		if (mod != 0)
		{
			num %= mod;
		}
		SetPartyLevel(num);
	}

	public int CalcPartyLevel()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < members.Count; i++)
		{
			EntityPlayer entityPlayer = members[i];
			list.Add(entityPlayer.gameStage);
		}
		return GameStageDefinition.CalcPartyLevel(list);
	}

	public void SetPartyLevel(int _partyLevel)
	{
		partyLevel = _partyLevel;
		partyLevel = (int)((float)partyLevel * gsScaling);
		stageSpawnMax = 0;
		groupIndex = 0;
		spawnCount = 0;
		if (def != null)
		{
			stage = def.GetStage(_partyLevel);
			if (stage != null)
			{
				stageSpawnMax = CalcStageSpawnMax();
				SetupGroup();
			}
		}
		bonusLootEvery = Utils.FastMax(stageSpawnMax / GameStageDefinition.LootBonusMaxCount, GameStageDefinition.LootBonusEvery);
		Log.Out("Party of {0}, GS {1} ({2}), scaling {3}, enemy max {4}, bonus every {5}", members.Count, partyLevel, _partyLevel, gsScaling, stageSpawnMax, bonusLootEvery);
		Log.Out("Party members: ");
		for (int i = 0; i < members.Count; i++)
		{
			EntityPlayer entityPlayer = members[i];
			Log.Out("Player id {0}, gameStage {1}", entityPlayer.entityId, entityPlayer.gameStage);
		}
	}

	public bool Tick(double _deltaTime)
	{
		if (spawnGroup != null)
		{
			bool flag = false;
			if (nextStageTime != 0 && world.worldTime >= nextStageTime)
			{
				flag = true;
			}
			else if (spawnCount >= numToSpawn)
			{
				interval -= _deltaTime;
				flag = interval <= 0.0;
			}
			if (flag)
			{
				groupIndex++;
				SetupGroup();
			}
		}
		return spawnGroup != null;
	}

	public void AddMember(EntityPlayer _player)
	{
		if (!memberIDs.Contains(_player.entityId))
		{
			memberIDs.Add(_player.entityId);
		}
		if (!members.Contains(_player))
		{
			members.Add(_player);
		}
	}

	public bool IsMemberOfParty(int _entityID)
	{
		return memberIDs.Contains(_entityID);
	}

	public void RemoveMember(EntityPlayer _player, bool removeID)
	{
		members.Remove(_player);
		if (removeID)
		{
			memberIDs.Remove(_player.entityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CalcStageSpawnMax()
	{
		int num = 0;
		int count = stage.Count;
		for (int i = 0; i < count; i++)
		{
			spawnGroup = stage.GetSpawnGroup(i);
			num += spawnGroup.spawnCount;
		}
		return num;
	}

	public void ClearMembers()
	{
		members.Clear();
		memberIDs.Clear();
	}

	public void IncSpawnCount()
	{
		spawnCount++;
	}

	public void DecSpawnCount(int dec)
	{
		if (dec > spawnCount)
		{
			spawnCount = 0;
		}
		else
		{
			spawnCount -= dec;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupGroup()
	{
		spawnGroup = stage.GetSpawnGroup(groupIndex);
		if (spawnGroup != null)
		{
			interval = (int)spawnGroup.interval;
			nextStageTime = ((spawnGroup.duration > 0) ? (world.worldTime + (uint)(spawnGroup.duration * 1000)) : 0);
			numToSpawn = EntitySpawner.ModifySpawnCountByGameDifficulty(spawnGroup.spawnCount);
			spawnCount = 0;
		}
		else
		{
			Log.Out("AIDirectorGameStagePartySpawner: groups done ({0})", groupIndex);
		}
	}

	public override string ToString()
	{
		return $"{groupIndex} {spawnGroupName} (count {spawnCount}, numToSpawn {numToSpawn}, maxAlive {maxAlive})";
	}
}

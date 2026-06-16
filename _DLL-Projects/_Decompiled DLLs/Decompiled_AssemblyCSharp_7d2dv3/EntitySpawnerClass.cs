using System;
using UnityEngine;

public class EntitySpawnerClass
{
	public static string PropStartSound = "StartSound";

	public static string PropStartText = "StartText";

	public static string PropEntityGroupName = "EntityGroupName";

	public static string PropTime = "Time";

	public static string PropDelayBetweenSpawns = "DelayBetweenSpawns";

	public static string PropTotalAlive = "TotalAlive";

	public static string PropTotalPerWave = "TotalPerWave";

	public static string PropDelayToNextWave = "DelayToNextWave";

	public static string PropAttackPlayerAtOnce = "AttackPlayerAtOnce";

	public static string PropNumberOfWaves = "NumberOfWaves";

	public static string PropTerritorial = "Territorial";

	public static string PropTerritorialRange = "TerritorialRange";

	public static string PropSpawnOnGround = "SpawnOnGround";

	public static string PropIgnoreTrigger = "IgnoreTrigger";

	public static string PropResetToday = "ResetToday";

	public static string PropDaysToRespawnIfPlayerLeft = "DaysToRespawnIfPlayerLeft";

	public static DictionarySave<string, EntitySpawnerClassForDay> list = new DictionarySave<string, EntitySpawnerClassForDay>();

	public static EntitySpawnerClass DefaultClassName;

	public DynamicProperties Properties = new DynamicProperties();

	public string name;

	public string entityGroupName;

	public EDaytime spawnAtTimeOfDay;

	public float delayBetweenSpawns;

	public int totalAlive;

	public float delayToNextWave;

	public int totalPerWaveMin;

	public int totalPerWaveMax;

	public int numberOfWaves;

	public bool bAttackPlayerImmediately;

	public bool bSpawnOnGround;

	public bool bIgnoreTrigger;

	public bool bTerritorial;

	public int territorialRange;

	public bool bPropResetToday;

	public int daysToRespawnIfPlayerLeft;

	public string startSound;

	public string startText;

	public void Init()
	{
		if (!Properties.Values.ContainsKey(PropEntityGroupName))
		{
			throw new Exception("Mandatory property '" + PropEntityGroupName + "' missing in entityspawnerclass '" + name + "'");
		}
		entityGroupName = Properties.Values[PropEntityGroupName];
		if (!EntityGroups.list.ContainsKey(entityGroupName))
		{
			throw new Exception("Entity spawner '" + name + "' contains invalid group " + entityGroupName);
		}
		if (Properties.Values.ContainsKey(PropStartSound))
		{
			startSound = Properties.Values[PropStartSound];
		}
		if (Properties.Values.ContainsKey(PropStartText))
		{
			startText = Properties.Values[PropStartText];
		}
		spawnAtTimeOfDay = EDaytime.Any;
		if (Properties.Values.ContainsKey(PropTime))
		{
			spawnAtTimeOfDay = EnumUtils.Parse<EDaytime>(Properties.Values[PropTime]);
		}
		delayBetweenSpawns = 0f;
		if (Properties.Values.ContainsKey(PropDelayBetweenSpawns))
		{
			delayBetweenSpawns = StringParsers.ParseFloat(Properties.Values[PropDelayBetweenSpawns]);
		}
		totalAlive = 1;
		if (Properties.Values.ContainsKey(PropTotalAlive))
		{
			totalAlive = int.Parse(Properties.Values[PropTotalAlive]);
		}
		totalPerWaveMin = 1;
		totalPerWaveMax = 1;
		if (Properties.Values.ContainsKey(PropTotalPerWave))
		{
			StringParsers.ParseMinMaxCount(Properties.Values[PropTotalPerWave], out totalPerWaveMin, out totalPerWaveMax);
		}
		delayToNextWave = 1f;
		if (Properties.Values.ContainsKey(PropDelayToNextWave))
		{
			delayToNextWave = StringParsers.ParseFloat(Properties.Values[PropDelayToNextWave]);
		}
		bAttackPlayerImmediately = false;
		if (Properties.Values.ContainsKey(PropAttackPlayerAtOnce))
		{
			bAttackPlayerImmediately = StringParsers.ParseBool(Properties.Values[PropAttackPlayerAtOnce]);
		}
		if (Properties.Values.ContainsKey(PropNumberOfWaves))
		{
			numberOfWaves = int.Parse(Properties.Values[PropNumberOfWaves]);
		}
		bTerritorial = false;
		if (Properties.Values.ContainsKey(PropTerritorial))
		{
			bTerritorial = StringParsers.ParseBool(Properties.Values[PropTerritorial]);
		}
		territorialRange = 10;
		if (Properties.Values.ContainsKey(PropTerritorialRange))
		{
			territorialRange = int.Parse(Properties.Values[PropTerritorialRange]);
		}
		bSpawnOnGround = true;
		if (Properties.Values.ContainsKey(PropSpawnOnGround))
		{
			bSpawnOnGround = StringParsers.ParseBool(Properties.Values[PropSpawnOnGround]);
		}
		bIgnoreTrigger = false;
		if (Properties.Values.ContainsKey(PropIgnoreTrigger))
		{
			bIgnoreTrigger = StringParsers.ParseBool(Properties.Values[PropIgnoreTrigger]);
		}
		bPropResetToday = true;
		if (Properties.Values.ContainsKey(PropResetToday))
		{
			bPropResetToday = StringParsers.ParseBool(Properties.Values[PropResetToday]);
		}
		daysToRespawnIfPlayerLeft = 0;
		if (Properties.Values.ContainsKey(PropDaysToRespawnIfPlayerLeft))
		{
			daysToRespawnIfPlayerLeft = Mathf.RoundToInt(StringParsers.ParseFloat(Properties.Values[PropDaysToRespawnIfPlayerLeft]));
		}
		if (DefaultClassName == null)
		{
			DefaultClassName = this;
		}
	}

	public static void Cleanup()
	{
		list.Clear();
	}
}

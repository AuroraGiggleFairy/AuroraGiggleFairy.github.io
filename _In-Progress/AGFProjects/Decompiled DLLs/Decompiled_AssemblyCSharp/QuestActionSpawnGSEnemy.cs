using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionSpawnGSEnemy : BaseQuestAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;

	public static string PropGameStageID = "gamestage_list";

	public static string PropCount = "count";

	public override void PerformAction(Quest ownerQuest)
	{
		if (GameStats.GetBool(EnumGameStats.EnemySpawnMode))
		{
			HandleSpawnEnemies(ownerQuest);
		}
	}

	public void HandleSpawnEnemies(Quest ownerQuest)
	{
		if (Value != null && Value != "" && !int.TryParse(Value, out count) && Value.Contains("-"))
		{
			string[] array = Value.Split('-');
			int min = Convert.ToInt32(array[0]);
			int maxExclusive = Convert.ToInt32(array[1]);
			World world = GameManager.Instance.World;
			count = world.GetGameRandom().RandomRange(min, maxExclusive);
		}
		GameManager.Instance.StartCoroutine(SpawnEnemies(ownerQuest));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SpawnEnemies(Quest ownerQuest)
	{
		EntityPlayerLocal player = ownerQuest.OwnerJournal.OwnerPlayer;
		int lastClassId = 0;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_ = GameManager.Instance.World;
			GameStageDefinition def = GameStageDefinition.GetGameStage(ID);
			for (int i = 0; i < count; i++)
			{
				yield return new WaitForSeconds(0.5f);
				int randomFromGroup = EntityGroups.GetRandomFromGroup(def.GetStage(player.PartyGameStage).GetSpawnGroup(0).groupName, ref lastClassId);
				if (randomFromGroup != 0)
				{
					SpawnQuestEntity(randomFromGroup, -1, player);
				}
			}
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				yield return new WaitForSeconds(0.5f);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEntitySpawn>().Setup(ID, player.entityId));
			}
		}
	}

	public static void SpawnQuestEntity(int spawnedEntityID, int entityIDQuestHolder, EntityPlayer player = null)
	{
		World world = GameManager.Instance.World;
		if (player == null)
		{
			player = world.GetEntity(entityIDQuestHolder) as EntityPlayer;
		}
		Vector3 vector = new Vector3(world.GetGameRandom().RandomFloat * 2f + -1f, 0f, world.GetGameRandom().RandomFloat * 2f + -1f);
		vector.Normalize();
		float num = world.GetGameRandom().RandomFloat * 12f + 12f;
		Vector3 transformPos = player.position + vector * num;
		Vector3 rotation = new Vector3(0f, player.transform.eulerAngles.y + 180f, 0f);
		float num2 = (int)GameManager.Instance.World.GetHeight((int)transformPos.x, (int)transformPos.z);
		float num3 = (int)GameManager.Instance.World.GetTerrainHeight((int)transformPos.x, (int)transformPos.z);
		transformPos.y = (num2 + num3) / 2f + 1.5f;
		Entity entity = EntityFactory.CreateEntity(spawnedEntityID, transformPos, rotation);
		entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		GameManager.Instance.World.SpawnEntityInWorld(entity);
		(entity as EntityAlive).SetAttackTarget(player, 200);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionSpawnGSEnemy questActionSpawnGSEnemy = new QuestActionSpawnGSEnemy();
		CopyValues(questActionSpawnGSEnemy);
		return questActionSpawnGSEnemy;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropGameStageID))
		{
			ID = properties.Values[PropGameStageID];
		}
		if (properties.Values.ContainsKey(PropCount))
		{
			Value = properties.Values[PropCount];
		}
	}
}

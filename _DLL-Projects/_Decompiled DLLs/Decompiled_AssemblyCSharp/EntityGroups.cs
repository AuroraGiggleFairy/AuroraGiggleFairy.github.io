using System.Collections.Generic;

public class EntityGroups
{
	public static string DefaultGroupName;

	public static DictionarySave<string, List<SEntityClassAndProb>> list = new DictionarySave<string, List<SEntityClassAndProb>>();

	public static int GetRandomFromGroup(string _sEntityGroupName, ref int lastClassId, GameRandom random = null)
	{
		List<SEntityClassAndProb> grpList = list[_sEntityGroupName];
		if (random == null)
		{
			random = GameManager.Instance.World.GetGameRandom();
		}
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			num = GetRandomFromGroupList(grpList, random);
			if (num != lastClassId)
			{
				lastClassId = num;
				break;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetRandomFromGroupList(List<SEntityClassAndProb> grpList, GameRandom random)
	{
		float randomFloat = random.RandomFloat;
		float num = 0f;
		for (int i = 0; i < grpList.Count; i++)
		{
			SEntityClassAndProb sEntityClassAndProb = grpList[i];
			num += sEntityClassAndProb.prob;
			if (randomFloat <= num && sEntityClassAndProb.prob > 0f)
			{
				return sEntityClassAndProb.entityClassId;
			}
		}
		return -1;
	}

	public static bool IsEnemyGroup(string _sEntityGroupName)
	{
		List<SEntityClassAndProb> list = EntityGroups.list[_sEntityGroupName];
		if (list == null || list.Count < 1)
		{
			return false;
		}
		return EntityClass.list[list[0].entityClassId].bIsEnemyEntity;
	}

	public static void Normalize(string _sEntityGroupName, float totalp)
	{
		List<SEntityClassAndProb> list = EntityGroups.list[_sEntityGroupName];
		for (int i = 0; i < list.Count; i++)
		{
			SEntityClassAndProb value = list[i];
			value.prob /= totalp;
			list[i] = value;
		}
	}

	public static void Cleanup()
	{
		if (list != null)
		{
			list.Clear();
		}
	}
}

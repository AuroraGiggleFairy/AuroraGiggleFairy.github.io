using System.Collections.Generic;
using UnityEngine;

public class SleeperEventData
{
	public List<SleeperVolume> SleeperVolumes = new List<SleeperVolume>();

	public List<int> EntityList = new List<int>();

	public bool hasRefreshed;

	public int ShowQuestClearCount = 1;

	public Vector3 position;

	public void SetupData(Vector3 _position)
	{
		position = _position;
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)position.x, (int)position.z);
		ShowQuestClearCount = prefabFromWorldPos.prefab.ShowQuestClearCount;
		if (prefabFromWorldPos == null)
		{
			return;
		}
		for (int i = 0; i < prefabFromWorldPos.prefab.SleeperVolumes.Count; i++)
		{
			Vector3i startPos = prefabFromWorldPos.prefab.SleeperVolumes[i].startPos;
			Vector3i size = prefabFromWorldPos.prefab.SleeperVolumes[i].size;
			int num = GameManager.Instance.World.FindSleeperVolume(prefabFromWorldPos.boundingBoxPosition + startPos, prefabFromWorldPos.boundingBoxPosition + startPos + size);
			if (num != -1)
			{
				SleeperVolume sleeperVolume = GameManager.Instance.World.GetSleeperVolume(num);
				if (!sleeperVolume.isQuestExclude && !SleeperVolumes.Contains(sleeperVolume))
				{
					SleeperVolumes.Add(sleeperVolume);
				}
			}
		}
	}

	public bool Update()
	{
		if (!hasRefreshed)
		{
			World world = GameManager.Instance.World;
			for (int i = 0; i < SleeperVolumes.Count; i++)
			{
				SleeperVolumes[i].DespawnAndReset(world);
			}
			hasRefreshed = true;
		}
		if (SleeperVolumes.Count > 0)
		{
			bool flag = false;
			bool flag2 = false;
			for (int num = SleeperVolumes.Count - 1; num >= 0; num--)
			{
				SleeperVolume sleeperVolume = SleeperVolumes[num];
				if (sleeperVolume.wasCleared)
				{
					for (int j = 0; j < EntityList.Count; j++)
					{
						if ((GameManager.Instance.World.GetEntity(EntityList[j]) as EntityPlayer) is EntityPlayerLocal)
						{
							QuestEventManager.Current.SleeperVolumePositionRemoved(sleeperVolume.Center);
						}
						else
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.HideSleeperVolume, EntityList[j], sleeperVolume.Center));
						}
					}
					SleeperVolumes.RemoveAt(num);
					flag = true;
				}
				else
				{
					flag2 = true;
				}
			}
			if (flag)
			{
				if (SleeperVolumes.Count <= ShowQuestClearCount)
				{
					for (int k = 0; k < EntityList.Count; k++)
					{
						EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(EntityList[k]) as EntityPlayer;
						for (int l = 0; l < SleeperVolumes.Count; l++)
						{
							if (entityPlayer is EntityPlayerLocal)
							{
								QuestEventManager.Current.SleeperVolumePositionAdded(SleeperVolumes[l].Center);
							}
							else
							{
								SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ShowSleeperVolume, EntityList[k], SleeperVolumes[l].Center), _onlyClientsAttachedToAnEntity: false, EntityList[k]);
							}
						}
					}
				}
				flag = false;
			}
			if (flag2)
			{
				return false;
			}
			bool flag3 = false;
			for (int m = 0; m < EntityList.Count; m++)
			{
				if ((GameManager.Instance.World.GetEntity(EntityList[m]) as EntityPlayer) is EntityPlayerLocal)
				{
					flag3 = true;
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ClearSleeper, EntityList[m], position));
				}
			}
			if (flag3)
			{
				QuestEventManager.Current.ClearedSleepers(position);
			}
			return true;
		}
		return false;
	}
}

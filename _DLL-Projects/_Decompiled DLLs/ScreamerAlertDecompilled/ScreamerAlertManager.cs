using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreamerAlertManager : MonoBehaviour
{
	public static ScreamerAlertManager Instance;

	public Dictionary<Vector3, List<int>> targetScreamerIds = new Dictionary<Vector3, List<int>>();

	public static Dictionary<Vector3, List<int>> ClientTargetScreamerIds = new Dictionary<Vector3, List<int>>();

	private float syncTimer = 0f;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		DictionaryList<int, Entity> worldEntities = GameManager.Instance.World?.Entities;
		if (worldEntities == null)
		{
			return;
		}
		foreach (Vector3 targetPos in new List<Vector3>(targetScreamerIds.Keys))
		{
			List<int> list = targetScreamerIds[targetPos];
			bool flag = false;
			foreach (Entity item in worldEntities.list)
			{
				if (item != null && item.GetType().Name == "EntityZombie" && item.EntityClass != null && item.EntityClass.entityClassName.ToLower().Contains("screamer"))
				{
					float num = Vector3.Distance(item.position, targetPos);
					if (num <= 120f && !list.Contains(item.entityId) && !item.IsDead())
					{
						list.Add(item.entityId);
					}
					if (num <= 120f && !item.IsDead())
					{
						flag = true;
					}
				}
			}
			foreach (int item2 in list.ToList())
			{
				if (item2 != -1 && worldEntities.dict.TryGetValue(item2, out var value))
				{
					_ = value != null;
				}
			}
			if (list.Contains(-1) && flag)
			{
				list.Remove(-1);
			}
			list.RemoveAll((int id) => id != -1 && (!worldEntities.dict.TryGetValue(id, out var value2) || value2 == null || value2.IsDead() || Vector3.Distance(value2.position, targetPos) > 120f));
			if (list.Count == 0)
			{
				targetScreamerIds.Remove(targetPos);
			}
		}
		if (ScreamerAlertsController.Instance != null)
		{
			ScreamerAlertsController.Instance.UpdateAlertMessage();
		}
		if (!GameManager.IsDedicatedServer || !(ScreamerAlertsController.Instance != null))
		{
			return;
		}
		syncTimer += Time.deltaTime;
		if (!(syncTimer >= 0.5f))
		{
			return;
		}
		syncTimer = 0f;
		if (ScreamerAlertsController.Instance.hordeAlertEndTime > 0f && Time.time > ScreamerAlertsController.Instance.hordeAlertEndTime)
		{
			ScreamerAlertsController.Instance.hordeAlertPosition = Vector3.zero;
			ScreamerAlertsController.Instance.hordeAlertEndTime = 0f;
		}
		NetPackageScreamerAlertSync package = new NetPackageScreamerAlertSync(targetScreamerIds, ScreamerAlertsController.Instance.screamerAlertMessage, ScreamerAlertsController.Instance.hordeAlertPosition, ScreamerAlertsController.Instance.hordeAlertEndTime);
		foreach (ClientInfo item3 in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
		{
			item3.SendPackage(package);
		}
	}

	public void RemoveScoutScreamer(int entityId, Vector3 position)
	{
		foreach (KeyValuePair<Vector3, List<int>> item in targetScreamerIds.ToList())
		{
			Vector3 key = item.Key;
			List<int> value = item.Value;
			if (Vector3.Distance(key, position) <= 120f && value.Contains(entityId))
			{
				value.Remove(entityId);
				if (value.Count == 0)
				{
					targetScreamerIds.Remove(key);
				}
				if ((object)ScreamerAlertsController.Instance != null)
				{
					ScreamerAlertsController.Instance.UpdateAlertMessage();
				}
			}
		}
	}

	public void AddScoutScreamer(int entityId, Vector3 position)
	{
		if (!targetScreamerIds.ContainsKey(position))
		{
			targetScreamerIds[position] = new List<int>();
		}
		if (!targetScreamerIds[position].Contains(entityId))
		{
			targetScreamerIds[position].Add(entityId);
			if (targetScreamerIds[position].Contains(-1))
			{
				targetScreamerIds[position].Remove(-1);
			}
			if ((object)ScreamerAlertsController.Instance != null)
			{
				ScreamerAlertsController.Instance.UpdateAlertMessage();
			}
		}
	}

	public void AddScreamerTarget(Vector3 position)
	{
		if (!targetScreamerIds.ContainsKey(position))
		{
			targetScreamerIds[position] = new List<int> { -1 };
			if ((object)ScreamerAlertsController.Instance != null)
			{
				ScreamerAlertsController.Instance.UpdateAlertMessage();
			}
			return;
		}
		List<int> list = targetScreamerIds[position];
		if (!list.Any((int id) => id != -1) && !list.Contains(-1))
		{
			list.Add(-1);
			if ((object)ScreamerAlertsController.Instance != null)
			{
				ScreamerAlertsController.Instance.UpdateAlertMessage();
			}
		}
	}
}

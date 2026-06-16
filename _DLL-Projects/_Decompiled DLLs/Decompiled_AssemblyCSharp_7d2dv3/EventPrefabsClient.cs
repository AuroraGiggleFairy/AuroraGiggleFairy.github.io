public class EventPrefabsClient
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabCache prefabCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator dpd;

	public EventPrefabsClient(PrefabCache prefabCache, DynamicPrefabDecorator dynamicPrefabDecorator)
	{
		this.prefabCache = prefabCache;
		dpd = dynamicPrefabDecorator;
	}

	public void TryAdd(int id, string prefabName, byte rotation, Vector3i position)
	{
		if (dpd.GetPrefab(id) != null)
		{
			Log.Error(string.Format("[{0}] Cannot add prefab, prefab already exists with id {1}", "EventPrefabsClient", id));
			return;
		}
		Prefab prefabRotated = prefabCache.GetPrefabRotated(prefabName, rotation);
		if (prefabRotated == null)
		{
			Log.Error("[EventPrefabsClient] Could not load prefab '" + prefabName + "'");
			return;
		}
		Log.Out($"EventPrefabsClient Add {id} {prefabName}");
		PrefabInstance prefabInstance = new PrefabInstance(id, prefabRotated.location, position, rotation, prefabRotated, 0);
		dpd.AddEventPrefab(prefabInstance);
		DecoManager.Instance.ClearDecoObjectsInArea(prefabInstance.boundingBoxPosition, prefabInstance.boundingBoxSize);
	}

	public void Remove(int id, string prefabName, byte rotation, Vector3i position)
	{
		PrefabInstance prefab = dpd.GetPrefab(id);
		if (prefab == null)
		{
			Log.Error(string.Format("[{0}] Could not find prefab with id {1} to remove", "EventPrefabsClient", id));
		}
		else if (prefab.prefab.PrefabName != prefabName)
		{
			Log.Error(string.Format("[{0}] trying to remove prefab with id {1} but it has a different name. Looking for: {2}, found: {3}", "EventPrefabsClient", id, prefabName, prefab.prefab.PrefabName));
		}
		else if (prefab.boundingBoxPosition != position)
		{
			Log.Error(string.Format("[{0}] trying to remove prefab with id {1} but it has a different position. Looking for: {2}, found: {3}", "EventPrefabsClient", id, position, prefab.boundingBoxPosition));
		}
		else if (prefab.rotation != rotation)
		{
			Log.Error(string.Format("[{0}] trying to remove prefab with id {1} but it has a different rotation. Looking for: {2}, found: {3}", "EventPrefabsClient", id, rotation, prefab.rotation));
		}
		else
		{
			Log.Out($"EventPrefabsClient Remove {id} {prefabName}");
			dpd.RemoveEventPrefab(prefab);
		}
	}
}

public class EntityBedrollPositionList
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive theEntity;

	public int Count
	{
		get
		{
			if (GetPos().y == int.MaxValue)
			{
				return 0;
			}
			return 1;
		}
	}

	public Vector3i this[int _idx] => GetPos();

	public EntityBedrollPositionList(EntityAlive _e)
	{
		theEntity = _e;
	}

	public Vector3i GetPos()
	{
		return GetData()?.BedrollPos ?? new Vector3i(0, int.MaxValue, 0);
	}

	public void Set(Vector3i _pos)
	{
		PersistentPlayerData data = GetData();
		if (data != null)
		{
			data.BedrollPos = _pos;
			data.ShowBedrollOnMap();
		}
	}

	public void Clear()
	{
		GetData()?.ClearBedroll();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData GetData()
	{
		return GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(theEntity.entityId);
	}
}

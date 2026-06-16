using System.Collections.Generic;

public class QuestLockInstance
{
	public bool IsLocked = true;

	public List<int> LockedByEntities = new List<int>();

	public ulong LockedOutUntil;

	public QuestLockInstance(int lockedByEntityID)
	{
		AddQuester(lockedByEntityID);
		LockedOutUntil = 0uL;
		IsLocked = true;
	}

	public void AddQuester(int entityID)
	{
		if (!LockedByEntities.Contains(entityID))
		{
			LockedByEntities.Add(entityID);
		}
	}

	public void AddQuesters(int[] entityIDs)
	{
		for (int i = 0; i < entityIDs.Length; i++)
		{
			if (!LockedByEntities.Contains(entityIDs[i]))
			{
				LockedByEntities.Add(entityIDs[i]);
			}
		}
	}

	public void RemoveQuester(int entityID)
	{
		if (LockedByEntities.Contains(entityID))
		{
			LockedByEntities.Remove(entityID);
		}
		if (LockedByEntities.Count == 0)
		{
			SetUnlocked();
		}
	}

	public void SetUnlocked()
	{
		if (IsLocked)
		{
			IsLocked = false;
			if (!GameUtils.IsPlaytesting())
			{
				LockedOutUntil = GameManager.Instance.World.GetWorldTime() + 2000;
			}
		}
	}

	public bool CheckQuestLock()
	{
		if (!IsLocked)
		{
			return GameManager.Instance.World.GetWorldTime() > LockedOutUntil;
		}
		return false;
	}
}

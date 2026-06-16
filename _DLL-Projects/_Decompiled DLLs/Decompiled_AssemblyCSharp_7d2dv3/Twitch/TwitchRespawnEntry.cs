using System.Collections.Generic;

namespace Twitch;

public class TwitchRespawnEntry
{
	public string UserName;

	public EntityPlayer Target;

	public TwitchAction Action;

	public int RespawnsLeft;

	public bool NeedsRespawn;

	public bool ReadyForRemove;

	public List<int> SpawnedEntities = new List<int>();

	public List<Vector3i> SpawnedBlocks = new List<Vector3i>();

	public TwitchRespawnEntry(string username, int respawnsLeft, EntityPlayer target, TwitchAction action)
	{
		UserName = username;
		Target = target;
		Action = action;
		RespawnsLeft = respawnsLeft;
	}

	public bool CheckRespawn(string username, EntityPlayer target, TwitchAction action)
	{
		if (UserName == username && Target == target)
		{
			return Action == action;
		}
		return false;
	}

	public bool RemoveSpawnedEntry(int entityID, bool checkForRemove)
	{
		bool result = false;
		for (int num = SpawnedEntities.Count - 1; num >= 0; num--)
		{
			if (SpawnedEntities[num] == entityID)
			{
				result = true;
				SpawnedEntities.RemoveAt(num);
			}
		}
		if (checkForRemove)
		{
			CheckReadyForRemove();
		}
		return result;
	}

	public bool RemoveSpawnedBlock(Vector3i pos, bool checkForRemove)
	{
		bool result = false;
		for (int num = SpawnedBlocks.Count - 1; num >= 0; num--)
		{
			if (SpawnedBlocks[num] == pos)
			{
				result = true;
				SpawnedBlocks.RemoveAt(num);
			}
		}
		if (checkForRemove)
		{
			CheckReadyForRemove();
		}
		return result;
	}

	public bool RemoveAllSpawnedBlock(bool checkForRemove)
	{
		bool result = false;
		if (SpawnedBlocks.Count > 0)
		{
			result = true;
			SpawnedBlocks.Clear();
		}
		if (checkForRemove)
		{
			CheckReadyForRemove();
		}
		return result;
	}

	public void CheckReadyForRemove()
	{
		switch (Action.RespawnCountType)
		{
		case TwitchAction.RespawnCountTypes.SpawnsOnly:
			ReadyForRemove = SpawnedEntities.Count == 0;
			break;
		case TwitchAction.RespawnCountTypes.BlocksOnly:
			ReadyForRemove = SpawnedBlocks.Count == 0;
			break;
		default:
			ReadyForRemove = SpawnedEntities.Count == 0 && SpawnedBlocks.Count == 0;
			break;
		}
	}

	public TwitchActionEntry RespawnAction()
	{
		TwitchActionEntry twitchActionEntry = Action.SetupActionEntry();
		twitchActionEntry.UserName = UserName;
		twitchActionEntry.Target = Target;
		twitchActionEntry.Action = Action;
		twitchActionEntry.IsRespawn = true;
		twitchActionEntry.IsBitAction = Action.PointType == TwitchAction.PointTypes.Bits;
		RespawnsLeft--;
		NeedsRespawn = false;
		if (RespawnsLeft <= 0)
		{
			ReadyForRemove = true;
		}
		return twitchActionEntry;
	}

	public bool CanRespawn(TwitchManager tm)
	{
		if (NeedsRespawn && tm.CheckCanRespawnEvent(Target))
		{
			return true;
		}
		return false;
	}
}

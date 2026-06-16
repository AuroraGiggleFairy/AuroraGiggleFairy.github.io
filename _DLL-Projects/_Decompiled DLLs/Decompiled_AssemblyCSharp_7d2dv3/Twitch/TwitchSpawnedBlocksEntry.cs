using System.Collections.Generic;

namespace Twitch;

public class TwitchSpawnedBlocksEntry
{
	public List<Vector3i> blocks;

	public List<Vector3i> recentlyRemoved;

	public TwitchActionEntry Action;

	public TwitchEventActionEntry Event;

	public TwitchVoteEntry Vote;

	public int BlockGroupID = -1;

	public float TimeRemaining = -1f;

	public TwitchRespawnEntry RespawnEntry;

	public bool CheckPos(Vector3i pos)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			if (blocks[i] == pos)
			{
				return true;
			}
		}
		if (recentlyRemoved != null)
		{
			for (int j = 0; j < recentlyRemoved.Count; j++)
			{
				if (recentlyRemoved[j] == pos)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool RemoveBlock(Vector3i blockRemoved)
	{
		for (int num = blocks.Count - 1; num >= 0; num--)
		{
			if (blocks[num] == blockRemoved)
			{
				if (recentlyRemoved == null)
				{
					recentlyRemoved = new List<Vector3i>();
				}
				recentlyRemoved.Add(blocks[num]);
				blocks.RemoveAt(num);
			}
		}
		return blocks.Count == 0;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool RemoveBlocks(List<Vector3i> blocksRemoved)
	{
		for (int num = blocks.Count - 1; num >= 0; num--)
		{
			for (int i = 0; i < blocksRemoved.Count; i++)
			{
				if (blocks[num] == blocksRemoved[i])
				{
					blocks.RemoveAt(num);
					break;
				}
			}
		}
		return blocks.Count == 0;
	}
}

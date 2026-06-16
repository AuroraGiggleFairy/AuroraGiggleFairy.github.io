using System.Collections.Generic;

public class QuestTierReward
{
	public int Tier;

	public List<BaseReward> Rewards = new List<BaseReward>();

	public void GiveRewards(EntityPlayer player)
	{
		for (int i = 0; i < Rewards.Count; i++)
		{
			Rewards[i].GiveReward(player);
		}
	}
}

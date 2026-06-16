using System;
using System.Collections.Generic;

namespace GameEvent.GameEventHelpers;

public class HomerunManager
{
	public DictionaryList<EntityPlayer, HomerunData> HomerunDataList = new DictionaryList<EntityPlayer, HomerunData>();

	public void Cleanup()
	{
		for (int i = 0; i < HomerunDataList.Count; i++)
		{
			HomerunDataList.list[i].Cleanup();
		}
		HomerunDataList.Clear();
	}

	public void Update(float deltaTime)
	{
		for (int num = HomerunDataList.Count - 1; num >= 0; num--)
		{
			if (!HomerunDataList.list[num].Update(deltaTime))
			{
				HomerunData homerunData = HomerunDataList.list[num];
				homerunData.CompleteCallback();
				HomerunDataList.Remove(homerunData.Player);
				homerunData.Cleanup();
			}
		}
	}

	public void AddPlayerToHomerun(EntityPlayer player, List<int> rewardLevels, List<string> rewardEvents, float gameTime, Action completeCallback)
	{
		if (!HomerunDataList.dict.ContainsKey(player))
		{
			HomerunDataList.Add(player, new HomerunData(player, gameTime, "twitch_homerungoal_red,twitch_homerungoal_blue,twitch_homerungoal_green", rewardLevels, rewardEvents, this, completeCallback));
		}
	}

	public bool HasHomerunActive(EntityPlayer player)
	{
		return HomerunDataList.dict.ContainsKey(player);
	}
}

using System.Collections.Generic;

namespace Twitch;

public class TwitchActionManager
{
	public class ActionCategory
	{
		public string Name;

		public string DisplayName;

		public string Icon;

		public bool ShowInCommandList = true;

		public bool AlwaysShowInMenu;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TwitchActionManager instance = null;

	public List<ActionCategory> CategoryList = new List<ActionCategory>();

	public static Dictionary<string, TwitchAction> TwitchActions = new Dictionary<string, TwitchAction>();

	public static Dictionary<string, TwitchVote> TwitchVotes = new Dictionary<string, TwitchVote>();

	public static TwitchActionManager Current
	{
		get
		{
			if (instance == null)
			{
				instance = new TwitchActionManager();
			}
			return instance;
		}
	}

	public static bool HasInstance => instance != null;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchActionManager()
	{
	}

	public void Cleanup()
	{
		if (TwitchActions != null)
		{
			TwitchActions.Clear();
		}
		if (TwitchVotes != null)
		{
			TwitchVotes.Clear();
		}
		CategoryList.Clear();
		if (TwitchManager.HasInstance)
		{
			TwitchManager.Current.CleanupData();
		}
	}

	public void AddAction(TwitchAction action)
	{
		if (!TwitchActions.ContainsKey(action.Name))
		{
			TwitchActions.Add(action.Name, action);
		}
	}

	public void AddVoteClass(TwitchVote vote)
	{
		TwitchVotes.Add(vote.VoteName, vote);
	}

	public int GetCategoryIndex(string categoryName)
	{
		for (int i = 0; i < CategoryList.Count; i++)
		{
			if (categoryName.StartsWith(CategoryList[i].Name))
			{
				return i;
			}
		}
		return 9999;
	}

	public ActionCategory GetCategory(string categoryName)
	{
		for (int i = 0; i < CategoryList.Count; i++)
		{
			if (CategoryList[i].Name == categoryName)
			{
				return CategoryList[i];
			}
		}
		return null;
	}
}

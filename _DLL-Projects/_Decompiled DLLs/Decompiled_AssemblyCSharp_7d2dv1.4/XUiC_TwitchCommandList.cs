using System.Collections.Generic;
using Twitch;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchCommandList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchCommandEntry> commandEntries = new List<XUiC_TwitchCommandEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	public string completeIconName = "";

	public string incompleteIconName = "";

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFB400";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

	public string completeColor = "0,255,0,255";

	public string incompleteColor = "255, 180, 0, 255";

	public string warningColor = "255,255,0,255";

	public Dictionary<string, List<TwitchAction>> commandLists = new Dictionary<string, List<TwitchAction>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchActionGroup> commandGroupList = new List<TwitchActionGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int commandListIndex = -1;

	public string CurrentKey = "";

	public string CurrentTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public float secondRotation = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager twitchManager;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchWindow Owner { get; set; }

	public float GetHeight()
	{
		if (!twitchManager.IsReady || twitchManager.VotingManager.VotingIsActive)
		{
			return 0f;
		}
		if (commandLists.ContainsKey(CurrentKey))
		{
			return commandLists[CurrentKey].Count * 30;
		}
		return 0f;
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchCommandEntry[] childrenByType = GetChildrenByType<XUiC_TwitchCommandEntry>();
		for (int i = 0; i < childrenByType.Length; i++)
		{
			if (childrenByType[i] != null)
			{
				commandEntries.Add(childrenByType[i]);
			}
		}
		twitchManager = TwitchManager.Current;
		twitchManager.CommandsChanged -= TwitchManager_CommandsChanged;
		twitchManager.CommandsChanged += TwitchManager_CommandsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_CommandsChanged()
	{
		SetupCommandList();
		lastUpdate = 0f;
		commandListIndex = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetPrevCategory()
	{
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			commandListIndex--;
			if (commandListIndex < 0)
			{
				commandListIndex = commandGroupList.Count - 1;
			}
			if (commandLists.ContainsKey(commandGroupList[commandListIndex].groupName))
			{
				flag = true;
			}
			num++;
			if (num > commandGroupList.Count)
			{
				break;
			}
		}
		ResetKey();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetNextCategory()
	{
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			commandListIndex++;
			if (commandListIndex >= commandGroupList.Count)
			{
				commandListIndex = 0;
			}
			if (commandGroupList.Count == 0 || commandLists.ContainsKey(commandGroupList[commandListIndex].groupName))
			{
				flag = true;
			}
			num++;
			if (num > commandGroupList.Count)
			{
				break;
			}
		}
		ResetKey();
	}

	public override void Update(float _dt)
	{
		if (!twitchManager.IsReady)
		{
			return;
		}
		if (Time.time - lastUpdate >= secondRotation)
		{
			isDirty = true;
			if (commandLists.Count > 0)
			{
				GetNextCategory();
			}
			lastUpdate = Time.time;
		}
		if (isDirty)
		{
			if (commandLists.Count == 0)
			{
				SetupCommandList();
			}
			if (commandLists.Count != 0 && commandLists.ContainsKey(CurrentKey))
			{
				TwitchAction[] array = (from a in commandLists[CurrentKey]
					orderby a.Command
					orderby a.PointType
					select a).ToArray();
				int num = 0;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					if (num >= commandEntries.Count)
					{
						break;
					}
					if (commandEntries[num] != null)
					{
						commandEntries[num].Owner = Owner;
						commandEntries[num].Action = array[num2];
						num++;
					}
				}
				for (int num3 = num; num3 < commandEntries.Count; num3++)
				{
					commandEntries[num3].Action = null;
				}
				isDirty = false;
			}
		}
		base.Update(_dt);
	}

	public void MoveForward()
	{
		GetNextCategory();
		lastUpdate = Time.time - 2f;
		isDirty = true;
	}

	public void MoveBackward()
	{
		GetPrevCategory();
		lastUpdate = Time.time - 2f;
		isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchActionGroup AddCommandGroup(string groupName)
	{
		for (int i = 0; i < commandGroupList.Count; i++)
		{
			if (commandGroupList[i].groupName == groupName)
			{
				return commandGroupList[i];
			}
		}
		int categoryIndex = TwitchActionManager.Current.GetCategoryIndex(groupName);
		commandGroupList.Add(new TwitchActionGroup
		{
			ActionList = new List<TwitchAction>(),
			groupName = groupName,
			displayName = TwitchActionManager.Current.CategoryList[categoryIndex].DisplayName,
			index = categoryIndex
		});
		return commandGroupList[commandGroupList.Count - 1];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveCommandGroup(string groupName)
	{
		for (int i = 0; i < commandGroupList.Count; i++)
		{
			if (commandGroupList[i].groupName == groupName)
			{
				commandGroupList.RemoveAt(i);
				break;
			}
		}
	}

	public void SetupCommandList()
	{
		commandLists.Clear();
		string name = TwitchActionManager.Current.CategoryList[0].Name;
		int num = 0;
		foreach (string key in twitchManager.AvailableCommands.Keys)
		{
			TwitchAction twitchAction = twitchManager.AvailableCommands[key];
			if (!twitchAction.OnCooldown && twitchAction.OnlyUsableByType == TwitchAction.OnlyUsableTypes.Everyone)
			{
				num++;
			}
			if (num > 10)
			{
				break;
			}
		}
		bool flag = num <= 10;
		if (flag)
		{
			commandLists.Add(name, new List<TwitchAction>());
		}
		foreach (string key2 in twitchManager.AvailableCommands.Keys)
		{
			TwitchAction twitchAction2 = twitchManager.AvailableCommands[key2];
			if (twitchAction2.PointType == TwitchAction.PointTypes.Bits && twitchManager.BroadcasterType == "")
			{
				continue;
			}
			if (flag)
			{
				if (!twitchAction2.OnCooldown && twitchAction2.OnlyUsableByType == TwitchAction.OnlyUsableTypes.Everyone)
				{
					twitchAction2.groupIndex = 0;
					commandLists[name].Add(twitchAction2);
					AddCommandGroup(name).ActionList.Add(twitchAction2);
				}
			}
			else if (twitchAction2.HasExtraConditions())
			{
				string text = twitchAction2.CategoryNames[0];
				if (!commandLists.ContainsKey(text))
				{
					commandLists.Add(text, new List<TwitchAction>());
				}
				AddCommandGroup(text).ActionList.Add(twitchAction2);
				twitchAction2.groupIndex = 0;
				commandLists[text].Add(twitchAction2);
			}
		}
		commandListIndex = 0;
		lastUpdate = Time.time;
		if (!flag)
		{
			bool flag2 = true;
			while (flag2)
			{
				flag2 = false;
				foreach (string key3 in commandLists.Keys)
				{
					bool flag3 = false;
					if (commandLists[key3].Count <= 10)
					{
						continue;
					}
					List<TwitchAction> list = commandLists[key3];
					commandLists.Remove(key3);
					RemoveCommandGroup(key3);
					for (int i = 0; i < list.Count; i++)
					{
						TwitchAction twitchAction3 = list[i];
						string text2 = twitchAction3.CategoryNames[twitchAction3.groupIndex];
						if (twitchAction3.CategoryNames.Count > twitchAction3.groupIndex + 1)
						{
							twitchAction3.groupIndex++;
							text2 = twitchAction3.CategoryNames[twitchAction3.groupIndex];
							flag3 = true;
						}
						if (!commandLists.ContainsKey(text2))
						{
							commandLists.Add(text2, new List<TwitchAction>());
						}
						AddCommandGroup(text2).ActionList.Add(twitchAction3);
						commandLists[text2].Add(twitchAction3);
					}
					if (flag3)
					{
						flag2 = true;
						break;
					}
					commandLists.Remove(key3);
					RemoveCommandGroup(key3);
					for (int j = 0; j < list.Count; j++)
					{
						TwitchAction twitchAction4 = list[j];
						int num2 = j / 10 + 1;
						string text3 = twitchAction4.CategoryNames[twitchAction4.groupIndex] + num2;
						if (!commandLists.ContainsKey(text3))
						{
							commandLists.Add(text3, new List<TwitchAction>());
						}
						AddCommandGroup(text3).ActionList.Add(twitchAction4);
						commandLists[text3].Add(twitchAction4);
					}
					flag2 = true;
					break;
				}
			}
		}
		commandGroupList = (from x in commandGroupList
			orderby x.index, x.groupName
			select x).ToList();
		ResetKey();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetKey()
	{
		if (commandLists.Count <= 1)
		{
			CurrentTitle = (CurrentKey = Localization.Get("TwitchActionCategory_Commands"));
			return;
		}
		CurrentKey = commandGroupList[commandListIndex].groupName;
		CurrentTitle = commandGroupList[commandListIndex].displayName;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
		twitchManager = TwitchManager.Current;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "complete_icon":
			completeIconName = value;
			return true;
		case "incomplete_icon":
			incompleteIconName = value;
			return true;
		case "complete_color":
		{
			Color32 color2 = StringParsers.ParseColor(value);
			completeColor = $"{color2.r},{color2.g},{color2.b},{color2.a}";
			completeHexColor = Utils.ColorToHex(color2);
			return true;
		}
		case "incomplete_color":
		{
			Color32 color = StringParsers.ParseColor(value);
			incompleteColor = $"{color.r},{color.g},{color.b},{color.a}";
			incompleteHexColor = Utils.ColorToHex(color);
			return true;
		}
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}

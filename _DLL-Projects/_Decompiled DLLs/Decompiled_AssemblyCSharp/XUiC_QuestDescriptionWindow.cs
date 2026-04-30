using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestDescriptionWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestEntry entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiQuestDescriptionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string, string> questtitleFormatter = new CachedStringFormatter<string, string, string>([PublicizedFrom(EAccessModifier.Internal)] (string _s, string _s1, string _s2) => $"{_s} : {_s1} {_s2}");

	public Quest CurrentQuest
	{
		get
		{
			return currentQuest;
		}
		set
		{
			currentQuest = value;
			questClass = ((value != null) ? QuestClass.GetQuest(currentQuest.ID) : null);
			RefreshBindings(_forceAll: true);
		}
	}

	public override void Init()
	{
		base.Init();
		xuiQuestDescriptionLabel = Localization.Get("xuiDescriptionLabel");
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "questdescription":
			value = ((currentQuest != null) ? currentQuest.GetParsedText(questClass.Description) : "");
			return true;
		case "questcategory":
			value = ((currentQuest != null) ? questClass.Category : "");
			return true;
		case "questsubtitle":
			value = ((currentQuest != null) ? questClass.SubTitle : "");
			return true;
		case "questtitle":
			value = ((currentQuest != null) ? questtitleFormatter.Format(questClass.Category, questClass.SubTitle, (currentQuest.GetSharedWithCount() == 0) ? "" : ("(" + currentQuest.GetSharedWithCount() + ")")) : xuiQuestDescriptionLabel);
			return true;
		case "sharedbyname":
			if (currentQuest == null)
			{
				value = "";
			}
			else
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(currentQuest.SharedOwnerID);
				if (playerDataFromEntityID != null)
				{
					value = GameUtils.SafeStringFormat(playerDataFromEntityID.PlayerName.DisplayName);
				}
				else
				{
					value = "";
				}
			}
			return true;
		case "showempty":
			value = (currentQuest == null).ToString();
			return true;
		default:
			return false;
		}
	}

	public void SetQuest(XUiC_QuestEntry questEntry)
	{
		entry = questEntry;
		if (entry != null)
		{
			CurrentQuest = entry.Quest;
		}
		else
		{
			CurrentQuest = null;
		}
	}
}

using UnityEngine.Scripting;

[Preserve]
public class QuestActionShowMessageWindow : BaseQuestAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropMessage = "message";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropTitle = "title";

	[PublicizedFrom(EAccessModifier.Private)]
	public string message = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string title = "";

	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		XUiC_TipWindow.ShowTip(message, title, XUiM_Player.GetPlayer() as EntityPlayerLocal, null);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionShowMessageWindow questActionShowMessageWindow = new QuestActionShowMessageWindow();
		CopyValues(questActionShowMessageWindow);
		questActionShowMessageWindow.message = message;
		questActionShowMessageWindow.title = title;
		return questActionShowMessageWindow;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropMessage, ref message);
		properties.ParseString(PropTitle, ref title);
	}
}

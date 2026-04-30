using System.Collections.Generic;

public class Dialog
{
	public static Dictionary<string, Dialog> DialogList = new Dictionary<string, Dialog>();

	public string ID = "";

	public string StartStatementID = "";

	public string StartResponseID = "";

	public List<DialogPhase> Phases = new List<DialogPhase>();

	public List<DialogStatement> Statements = new List<DialogStatement>();

	public List<DialogResponse> Responses = new List<DialogResponse>();

	public EntityNPC CurrentOwner;

	public Dialog ChildDialog;

	public List<QuestEntry> QuestEntryList = new List<QuestEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DialogStatement currentStatement;

	public string currentReturnStatement = "";

	public DialogStatement CurrentStatement
	{
		get
		{
			if (ChildDialog != null)
			{
				return ChildDialog.CurrentStatement;
			}
			return currentStatement;
		}
		set
		{
			if (ChildDialog != null)
			{
				ChildDialog.CurrentStatement = value;
			}
			else
			{
				currentStatement = value;
			}
		}
	}

	public Dialog(string newID)
	{
		ID = newID;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DialogStatement GetStatement(string currentStatementID)
	{
		if (ChildDialog != null)
		{
			return ChildDialog.GetStatement(currentStatementID);
		}
		if (currentStatementID == "")
		{
			currentStatementID = StartStatementID;
		}
		for (int i = 0; i < Statements.Count; i++)
		{
			if (Statements[i].ID == currentStatementID)
			{
				return Statements[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DialogResponse GetResponse(string currentResponseID)
	{
		if (ChildDialog != null)
		{
			return ChildDialog.GetResponse(currentResponseID);
		}
		for (int i = 0; i < Responses.Count; i++)
		{
			if (Responses[i].ID == currentResponseID)
			{
				return Responses[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DialogStatement GetFirstStatment(EntityPlayer player)
	{
		string startStatementID = StartStatementID;
		for (int i = 0; i < Phases.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < Phases[i].RequirementList.Count; j++)
			{
				if (!Phases[i].RequirementList[j].CheckRequirement(player, CurrentOwner))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				startStatementID = Phases[i].StartStatementID;
				break;
			}
		}
		for (int k = 0; k < Statements.Count; k++)
		{
			if (Statements[k].ID == startStatementID)
			{
				return Statements[k];
			}
		}
		return null;
	}

	public void RestartDialog(EntityPlayer player)
	{
		CurrentStatement = GetFirstStatment(player);
		ChildDialog = null;
	}

	public void SelectResponse(DialogResponse response, EntityPlayer player)
	{
		if (ChildDialog != null)
		{
			ChildDialog.SelectResponse(response, player);
			return;
		}
		if (response.Actions.Count > 0)
		{
			for (int i = 0; i < response.Actions.Count; i++)
			{
				response.Actions[i].PerformAction(player);
			}
		}
		if (response is DialogResponseQuest)
		{
			DialogResponseQuest dialogResponseQuest = response as DialogResponseQuest;
			QuestClass questClass = dialogResponseQuest.Quest.QuestClass;
			CurrentStatement = new DialogStatement("");
			CurrentStatement.NextStatementID = dialogResponseQuest.NextStatementID;
			CurrentStatement.Text = dialogResponseQuest.Quest.GetParsedText(questClass.StatementText);
		}
		else
		{
			CurrentStatement = GetStatement(response.NextStatementID);
		}
	}

	public static void Cleanup()
	{
		DialogList.Clear();
	}

	public static void ReloadDialogs()
	{
		Cleanup();
		WorldStaticData.Reset("dialogs");
	}
}

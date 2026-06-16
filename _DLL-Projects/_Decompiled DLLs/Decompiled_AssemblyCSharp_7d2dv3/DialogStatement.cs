using System.Collections.Generic;

public class DialogStatement : BaseStatement
{
	public List<BaseResponseEntry> ResponseEntries = new List<BaseResponseEntry>();

	public DialogStatement(string newID)
	{
		ID = newID;
		HeaderName = $"Statement : {newID}";
	}

	public List<BaseResponseEntry> GetResponses()
	{
		List<BaseResponseEntry> list = new List<BaseResponseEntry>();
		if (ResponseEntries.Count > 0)
		{
			for (int i = 0; i < ResponseEntries.Count; i++)
			{
				switch (ResponseEntries[i].ResponseType)
				{
				case BaseResponseEntry.ResponseTypes.Response:
					ResponseEntries[i].Response = base.OwnerDialog.GetResponse(ResponseEntries[i].ID);
					list.Add(ResponseEntries[i]);
					break;
				case BaseResponseEntry.ResponseTypes.QuestAdd:
				{
					DialogQuestResponseEntry dialogQuestResponseEntry = ResponseEntries[i] as DialogQuestResponseEntry;
					DialogResponseQuest dialogResponseQuest = new DialogResponseQuest(dialogQuestResponseEntry.ID, dialogQuestResponseEntry.ReturnStatementID, ID, dialogQuestResponseEntry.questType, base.OwnerDialog, dialogQuestResponseEntry.ListIndex, dialogQuestResponseEntry.Tier);
					if (dialogResponseQuest.IsValid)
					{
						ResponseEntries[i].Response = dialogResponseQuest;
						list.Add(ResponseEntries[i]);
					}
					break;
				}
				}
			}
		}
		else if (base.NextStatementID != "")
		{
			DialogResponseEntry dialogResponseEntry = new DialogResponseEntry(base.NextStatementID);
			dialogResponseEntry.Response = DialogResponse.NextStatementEntry(base.NextStatementID);
			list.Add(dialogResponseEntry);
		}
		return list;
	}
}

using UnityEngine.Scripting;

[Preserve]
public class DialogQuestResponseEntry : BaseResponseEntry
{
	public int ListIndex = -1;

	public string ReturnStatementID = "";

	public string questType = "";

	public int Tier = -1;

	public DialogQuestResponseEntry(string _questID, string _type, string _returnStatementID, int _listIndex, int _tier)
	{
		base.ID = _questID;
		ListIndex = _listIndex;
		base.ResponseType = ResponseTypes.QuestAdd;
		questType = _type;
		Tier = _tier;
		ReturnStatementID = _returnStatementID;
	}
}

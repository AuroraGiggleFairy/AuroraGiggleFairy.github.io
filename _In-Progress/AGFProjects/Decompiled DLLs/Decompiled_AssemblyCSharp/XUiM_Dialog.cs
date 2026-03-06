public class XUiM_Dialog : XUiModel
{
	public EntityNPC Respondent;

	public XUiC_DialogWindowGroup DialogWindowGroup;

	public Quest QuestTurnIn;

	public string ReturnStatement = "";

	public bool keepZoomOnClose;

	public DialogStatement LastStatement;
}

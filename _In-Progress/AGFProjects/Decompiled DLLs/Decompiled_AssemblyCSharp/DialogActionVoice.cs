using UnityEngine.Scripting;

[Preserve]
public class DialogActionVoice : BaseDialogAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override ActionTypes ActionType => ActionTypes.Voice;

	public override void PerformAction(EntityPlayer player)
	{
		LocalPlayerUI.primaryUI.xui.Dialog.Respondent.PlayVoiceSetEntry(base.ID, player);
	}
}

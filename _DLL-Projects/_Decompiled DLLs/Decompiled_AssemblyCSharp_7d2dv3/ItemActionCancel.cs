using UnityEngine.Scripting;

[Preserve]
public class ItemActionCancel : ItemAction
{
	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (_bReleased)
		{
			int num = 1 - _actionData.indexInEntityOfAction;
			if (_actionData.invData.actionData[num] != null)
			{
				_actionData.invData.item.Actions[num].CancelAction(_actionData.invData.actionData[num]);
			}
		}
	}
}

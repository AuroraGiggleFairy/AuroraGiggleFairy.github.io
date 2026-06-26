using UnityEngine.Scripting;

[Preserve]
public class MinEventActionLogItemData : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string message;

	public override void Execute(MinEventParams _params)
	{
		if (_params.Self.inventory.holdingItem != null)
		{
			Log.Out("Debug Item: '{0}' Tags: {1}", _params.Self.inventory.holdingItem.GetItemName(), _params.Self.inventory.holdingItem.ItemTags.ToString());
		}
	}
}

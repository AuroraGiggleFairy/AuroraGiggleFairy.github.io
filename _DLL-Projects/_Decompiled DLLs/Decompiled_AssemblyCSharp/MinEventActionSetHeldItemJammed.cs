using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetHeldItemJammed : MinEventActionBase
{
	public override void Execute(MinEventParams _params)
	{
		ItemValue itemValue = _params.ItemValue;
		if (itemValue != null && !itemValue.IsEmpty())
		{
			itemValue.SetMetadata(ItemActionRanged.scGunIsJammed, 1);
		}
	}
}

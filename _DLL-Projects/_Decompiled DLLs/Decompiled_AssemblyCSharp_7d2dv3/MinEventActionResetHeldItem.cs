using UnityEngine.Scripting;

[Preserve]
public class MinEventActionResetHeldItem : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		_params.ItemActionData?.invData.item.OnHoldingReset(_params.ItemActionData.invData);
	}
}

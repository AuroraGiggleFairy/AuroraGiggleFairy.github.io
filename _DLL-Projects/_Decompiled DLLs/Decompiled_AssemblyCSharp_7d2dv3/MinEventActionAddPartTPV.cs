using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddPartTPV : MinEventActionAddPart
{
	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if ((_params.Self is EntityPlayerLocal entityPlayerLocal && !entityPlayerLocal.emodel.IsFPV) || _params.Self is EntityPlayer { isEntityRemote: not false })
		{
			return base.CanExecute(_eventType, _params);
		}
		return false;
	}
}

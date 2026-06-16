using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddPartFPV : MinEventActionAddPart
{
	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (_params.Self is EntityPlayerLocal entityPlayerLocal && entityPlayerLocal.emodel.IsFPV)
		{
			return base.CanExecute(_eventType, _params);
		}
		return false;
	}
}

using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddOrRemoveBuff : MinEventActionAddBuff
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdd;

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		isAdd = base.CanExecute(_eventType, _params);
		return true;
	}

	public override void Execute(MinEventParams _params)
	{
		if (isAdd)
		{
			base.Execute(_params);
		}
		else
		{
			Remove(_params);
		}
	}
}

using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemoveBuff : MinEventActionBuffModifierBase
{
	public override void Execute(MinEventParams _params)
	{
		Remove(_params);
	}
}

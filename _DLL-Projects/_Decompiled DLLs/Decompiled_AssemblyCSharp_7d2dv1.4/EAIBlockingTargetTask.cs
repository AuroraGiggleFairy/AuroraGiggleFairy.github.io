using UnityEngine.Scripting;

[Preserve]
public class EAIBlockingTargetTask : EAIBase
{
	public bool canExecute;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 1;
	}

	public override bool CanExecute()
	{
		return canExecute;
	}

	public override bool Continue()
	{
		return canExecute;
	}
}

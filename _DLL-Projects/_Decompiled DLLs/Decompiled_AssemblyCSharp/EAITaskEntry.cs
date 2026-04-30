using UnityEngine.Scripting;

[Preserve]
public class EAITaskEntry
{
	public EAIBase action;

	public int priority;

	public bool isExecuting;

	public float executeTime;

	public EAITaskEntry(int _priority, EAIBase _action)
	{
		priority = _priority;
		action = _action;
	}
}

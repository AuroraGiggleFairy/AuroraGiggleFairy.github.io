using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public struct ActionData
{
	public UAIAction Action;

	public object Target;

	public object Data;

	public int TaskIndex;

	public ulong TaskStartTimeStamp;

	public bool Initialized;

	public bool Started;

	public bool Executing;

	public bool Failed;

	public bool Finished;

	public UAITaskBase CurrentTask
	{
		get
		{
			if (Action == null || Action.GetTasks() == null || TaskIndex < 0 || TaskIndex >= Action.GetTasks().Count)
			{
				return null;
			}
			return Action.GetTasks()[TaskIndex];
		}
	}

	public void ClearData()
	{
		Data = null;
		TaskStartTimeStamp = 0uL;
		Initialized = false;
		Started = false;
		Executing = false;
		Failed = false;
		Finished = false;
	}
}

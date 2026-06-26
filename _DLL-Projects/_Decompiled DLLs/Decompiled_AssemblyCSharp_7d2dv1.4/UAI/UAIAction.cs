using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<UAITaskBase> tasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<UAIConsiderationBase> considerations;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float Weight
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public UAIAction(string _name, float _weight)
	{
		Name = _name;
		Weight = _weight;
		considerations = new List<UAIConsiderationBase>();
		tasks = new List<UAITaskBase>();
	}

	public float GetScore(Context _context, object _target, float min = 0f)
	{
		float num = 1f;
		if (considerations.Count == 0)
		{
			return num * Weight;
		}
		if (tasks.Count == 0)
		{
			return 0f;
		}
		for (int i = 0; i < considerations.Count; i++)
		{
			if (0f > num || num < min)
			{
				return 0f;
			}
			num *= considerations[i].ComputeResponseCurve(considerations[i].GetScore(_context, _target));
		}
		return (num + (1f - num) * (float)(1 - 1 / considerations.Count) * num) * Weight;
	}

	public void AddConsideration(UAIConsiderationBase _c)
	{
		considerations.Add(_c);
	}

	public void AddTask(UAITaskBase _t)
	{
		tasks.Add(_t);
	}

	public List<UAIConsiderationBase> GetConsiderations()
	{
		return considerations;
	}

	public List<UAITaskBase> GetTasks()
	{
		return tasks;
	}
}

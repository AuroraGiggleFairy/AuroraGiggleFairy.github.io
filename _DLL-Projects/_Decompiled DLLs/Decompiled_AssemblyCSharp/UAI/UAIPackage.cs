using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<UAIAction> actionList;

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

	public UAIPackage(string _name = "", float _weight = 1f)
	{
		Name = _name;
		Weight = _weight;
		actionList = new List<UAIAction>();
	}

	public float DecideAction(Context _context, out UAIAction _chosenAction, out object _chosenTarget)
	{
		float num = 0f;
		_chosenAction = null;
		_chosenTarget = null;
		for (int i = 0; i < actionList.Count; i++)
		{
			int num2 = 0;
			for (int j = 0; j < _context.ConsiderationData.EntityTargets.Count; j++)
			{
				if (num2 > UAIBase.MaxEntitiesToConsider)
				{
					break;
				}
				float score = actionList[i].GetScore(_context, _context.ConsiderationData.EntityTargets[j]);
				if (score > num)
				{
					num = score;
					_chosenAction = actionList[i];
					_chosenTarget = _context.ConsiderationData.EntityTargets[j];
				}
				num2++;
			}
			for (int k = 0; k < _context.ConsiderationData.WaypointTargets.Count && k <= UAIBase.MaxWaypointsToConsider; k++)
			{
				float score2 = actionList[i].GetScore(_context, _context.ConsiderationData.WaypointTargets[k]);
				if (score2 > num)
				{
					num = score2;
					_chosenAction = actionList[i];
					_chosenTarget = _context.ConsiderationData.WaypointTargets[k];
				}
			}
		}
		return num;
	}

	public List<UAIAction> GetActions()
	{
		return actionList;
	}

	public void AddAction(UAIAction _action)
	{
		actionList.Add(_action);
	}
}

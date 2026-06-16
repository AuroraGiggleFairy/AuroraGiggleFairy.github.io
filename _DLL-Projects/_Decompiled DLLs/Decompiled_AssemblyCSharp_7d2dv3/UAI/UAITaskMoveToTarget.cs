using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAITaskMoveToTarget : UAITaskBase
{
	public float distance;

	public bool run;

	public bool shouldBreakWalls;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initializeParameters()
	{
		base.initializeParameters();
		if (Parameters.ContainsKey("distance"))
		{
			distance = StringParsers.ParseFloat(Parameters["distance"]);
		}
		if (Parameters.ContainsKey("run"))
		{
			run = StringParsers.ParseBool(Parameters["run"]);
		}
		if (Parameters.ContainsKey("break_walls"))
		{
			shouldBreakWalls = StringParsers.ParseBool(Parameters["break_walls"]);
		}
	}

	public override void Start(Context _context)
	{
		base.Start(_context);
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
		if (entityAlive != null)
		{
			_context.Self.FindPath(RandomPositionGenerator.CalcNear(_context.Self, entityAlive.position, (int)distance, (int)distance), run ? _context.Self.GetMoveSpeedPanic() : (_context.Self.IsAlert ? _context.Self.GetMoveSpeedAggro() : _context.Self.GetMoveSpeed()), shouldBreakWalls, null);
		}
		else if (_context.ActionData.Target.GetType() == typeof(Vector3))
		{
			_context.Self.FindPath(RandomPositionGenerator.CalcNear(_context.Self, (Vector3)_context.ActionData.Target, (int)distance, (int)distance), run ? _context.Self.GetMoveSpeedPanic() : _context.Self.GetMoveSpeed(), shouldBreakWalls, null);
		}
		else
		{
			Stop(_context);
		}
	}

	public override void Update(Context _context)
	{
		base.Update(_context);
		if (_context.Self.getNavigator().noPathAndNotPlanningOne())
		{
			Stop(_context);
		}
	}
}

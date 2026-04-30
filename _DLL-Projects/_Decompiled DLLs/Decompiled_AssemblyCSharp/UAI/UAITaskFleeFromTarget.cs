using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAITaskFleeFromTarget : UAITaskBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxFleeDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initializeParameters()
	{
		base.initializeParameters();
		if (Parameters.ContainsKey("max_distance"))
		{
			maxFleeDistance = StringParsers.ParseFloat(Parameters["max_distance"]);
		}
	}

	public override void Start(Context _context)
	{
		base.Start(_context);
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
		if (entityAlive != null)
		{
			_context.Self.detachHome();
			_context.Self.FindPath(RandomPositionGenerator.CalcAway(_context.Self, 0, (int)maxFleeDistance, (int)maxFleeDistance, entityAlive.position), _context.Self.GetMoveSpeedPanic(), canBreak: false, null);
		}
		else
		{
			_context.ActionData.Failed = true;
		}
	}

	public override void Update(Context _context)
	{
		base.Update(_context);
		if (_context.Self.getNavigator().noPathAndNotPlanningOne())
		{
			_context.Self.setHomeArea(new Vector3i(_context.Self.position), 10);
			Stop(_context);
		}
	}
}

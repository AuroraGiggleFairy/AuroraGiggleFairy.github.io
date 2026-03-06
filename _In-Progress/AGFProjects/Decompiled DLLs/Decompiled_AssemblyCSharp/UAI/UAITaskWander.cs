using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAITaskWander : UAITaskBase
{
	public float maxWanderDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initializeParameters()
	{
		base.initializeParameters();
		if (Parameters.ContainsKey("max_distance"))
		{
			maxWanderDistance = StringParsers.ParseFloat(Parameters["max_distance"]);
		}
	}

	public override void Start(Context _context)
	{
		base.Start(_context);
		int num = 10;
		_context.Self.FindPath(RandomPositionGenerator.CalcAround(_context.Self, num, num), _context.Self.GetMoveSpeed(), canBreak: false, null);
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

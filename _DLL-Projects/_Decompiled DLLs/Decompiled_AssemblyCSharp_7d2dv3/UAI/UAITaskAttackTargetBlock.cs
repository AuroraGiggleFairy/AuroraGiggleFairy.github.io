using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAITaskAttackTargetBlock : UAITaskBase
{
	public int attackTimeout;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initializeParameters()
	{
		base.initializeParameters();
	}

	public override void Start(Context _context)
	{
		base.Start(_context);
		if (_context.ActionData.Target.GetType() == typeof(Vector3))
		{
			attackTimeout = _context.Self.GetAttackTimeoutTicks();
			Vector3 vector = (Vector3)_context.ActionData.Target;
			_context.Self.SetLookPosition(_context.Self.CanSee(vector) ? vector : Vector3.zero);
			if (_context.Self.bodyDamage.HasLimbs)
			{
				_context.Self.RotateTo(vector.x, vector.y, vector.z, 30f, 30f);
			}
		}
		else
		{
			Stop(_context);
		}
	}

	public override void Update(Context _context)
	{
		base.Update(_context);
		if (_context.ActionData.Target.GetType() == typeof(Vector3))
		{
			Vector3 lookPosition = (Vector3)_context.ActionData.Target;
			attackTimeout = Utils.FastMax(attackTimeout - 1, 0);
			if (attackTimeout <= 0)
			{
				_context.Self.SetLookPosition(lookPosition);
				if (_context.Self.bodyDamage.HasLimbs)
				{
					_context.Self.RotateTo(lookPosition.x, lookPosition.y, lookPosition.z, 30f, 30f);
				}
				if (_context.Self.Attack(_isReleased: false))
				{
					attackTimeout = _context.Self.GetAttackTimeoutTicks();
					_context.Self.Attack(_isReleased: true);
					Stop(_context);
				}
			}
		}
		else
		{
			Stop(_context);
		}
	}
}

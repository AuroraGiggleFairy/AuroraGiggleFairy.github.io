using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAITaskAttackTargetEntity : UAITaskBase
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
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
		if (entityAlive != null)
		{
			_context.Self.SetLookPosition(_context.Self.CanSee(entityAlive) ? entityAlive.getHeadPosition() : Vector3.zero);
			if (_context.Self.bodyDamage.HasLimbs)
			{
				_context.Self.RotateTo(entityAlive.position.x, entityAlive.position.y, entityAlive.position.z, 30f, 30f);
			}
			attackTimeout = _context.Self.GetAttackTimeoutTicks();
		}
		else
		{
			Stop(_context);
		}
	}

	public override void Update(Context _context)
	{
		base.Update(_context);
		EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
		if (entityAlive != null)
		{
			_context.Self.SetLookPosition(_context.Self.CanSee(entityAlive) ? entityAlive.getHeadPosition() : Vector3.zero);
			if (_context.Self.bodyDamage.HasLimbs)
			{
				_context.Self.RotateTo(entityAlive, 30f, 30f);
			}
			attackTimeout = Utils.FastMax(attackTimeout - 1, 0);
			if (attackTimeout <= 0 && _context.Self.Attack(_isReleased: false))
			{
				attackTimeout = _context.Self.GetAttackTimeoutTicks();
				_context.Self.Attack(_isReleased: true);
				Stop(_context);
			}
		}
		else
		{
			Stop(_context);
		}
	}
}

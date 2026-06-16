using UnityEngine.Scripting;

[Preserve]
public class ItemActionProjectile : ItemActionAttack
{
	public new ExplosionData Explosion;

	public new float Velocity;

	public new float FlyTime;

	public new float LifeTime;

	public float DeadTime;

	public float Gravity;

	public float collisionRadius;

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		Explosion = new ExplosionData(Properties, item.Effects);
		Properties.ParseFloat("FlyTime", ref FlyTime);
		Properties.ParseFloat("LifeTime", ref LifeTime);
		Properties.ParseFloat("DeadTime", ref DeadTime);
		Properties.ParseFloat("Velocity", ref Velocity);
		Gravity = -9.81f;
		Properties.ParseFloat("Gravity", ref Gravity);
		Properties.ParseFloat("CollisionRadius", ref collisionRadius);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
	}
}

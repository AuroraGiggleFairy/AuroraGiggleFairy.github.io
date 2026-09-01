namespace FullautoLauncher.Scripts.ProjectileManager;

public abstract class ParameterHolderAbs
{
	protected readonly ProjectileParams par;

	public ProjectileParams Params => par;

	protected ParameterHolderAbs(ProjectileParams par)
	{
		this.par = par;
	}

	public override int GetHashCode()
	{
		return par.ProjectileID;
	}

	public abstract void UpdatePosition();

	public virtual void Fire()
	{
	}
}

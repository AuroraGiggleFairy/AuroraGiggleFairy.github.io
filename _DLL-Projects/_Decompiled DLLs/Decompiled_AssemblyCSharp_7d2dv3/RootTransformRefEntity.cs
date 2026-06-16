using UnityEngine;

public class RootTransformRefEntity : RootTransformRef
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		if (RootTransform == null)
		{
			RootTransform = FindEntityUpwards(base.transform);
		}
	}

	public static RootTransformRefEntity AddIfEntity(Transform _t)
	{
		Transform transform = FindEntityUpwards(_t);
		if ((bool)transform)
		{
			RootTransformRefEntity rootTransformRefEntity = _t.gameObject.AddMissingComponent<RootTransformRefEntity>();
			rootTransformRefEntity.RootTransform = transform;
			return rootTransformRefEntity;
		}
		return null;
	}

	public static Transform FindEntityUpwards(Transform _t)
	{
		do
		{
			if ((bool)_t.GetComponent<Entity>())
			{
				return _t;
			}
			_t = _t.parent;
		}
		while ((bool)_t);
		return null;
	}

	public void GunOpen()
	{
	}

	public void GunClose()
	{
	}

	public void GunRemoveRound()
	{
	}

	public void GunLoadRound()
	{
	}

	public void GunCockBack()
	{
	}

	public void GunCockForward()
	{
	}
}

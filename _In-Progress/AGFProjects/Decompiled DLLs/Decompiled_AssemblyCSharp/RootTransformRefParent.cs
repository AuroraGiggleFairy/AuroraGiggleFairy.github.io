using UnityEngine;

public class RootTransformRefParent : RootTransformRef
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (!RootTransform)
		{
			RootTransform = FindTopTransform(base.transform);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform FindTopTransform(Transform _t)
	{
		Transform parent;
		while ((parent = _t.parent) != null)
		{
			_t = parent;
		}
		return _t;
	}

	public static Transform FindRoot(Transform _t)
	{
		Transform transform = _t;
		do
		{
			if (transform.TryGetComponent<RootTransformRefParent>(out var component))
			{
				return component.RootTransform;
			}
			transform = transform.parent;
		}
		while ((bool)transform);
		return _t;
	}
}

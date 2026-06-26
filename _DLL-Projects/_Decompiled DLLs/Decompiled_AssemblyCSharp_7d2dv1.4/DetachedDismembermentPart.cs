using UnityEngine;

public class DetachedDismembermentPart
{
	public float lifeTime = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float elapsedTime;

	public float overrideHeadSize = 1f;

	public float overrideHeadDismemberScaleTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startValuesSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public float startingHeadSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startingScale;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Transform detachT
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Transform pivotT
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ReadyForCleanup { get; set; }

	public void SetDetached(Transform _t)
	{
		detachT = _t;
	}

	public void SetPivot(Transform _t)
	{
		pivotT = _t;
	}

	public void CleanupDetached()
	{
		if ((bool)detachT)
		{
			Object.Destroy(detachT.gameObject);
			detachT = null;
		}
	}

	public void Update()
	{
		elapsedTime += Time.deltaTime;
		if ((bool)pivotT && overrideHeadSize != 1f)
		{
			if (!startValuesSet)
			{
				startingHeadSize = overrideHeadSize;
				startingScale = pivotT.localScale;
				startValuesSet = true;
			}
			float t = elapsedTime / overrideHeadDismemberScaleTime;
			overrideHeadSize = Mathf.Lerp(startingHeadSize, 1f, t);
			pivotT.localScale = Vector3.Lerp(startingScale, Vector3.one, t);
		}
		if (elapsedTime >= lifeTime)
		{
			ReadyForCleanup = true;
		}
	}
}

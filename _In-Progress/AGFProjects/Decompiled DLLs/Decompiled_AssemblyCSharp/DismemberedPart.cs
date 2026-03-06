using System.Collections.Generic;
using UnityEngine;

public class DismemberedPart
{
	public DismemberedPartData Data;

	public uint bodyDamageFlag;

	public EnumDamageTypes damageType;

	public Transform prefabT;

	public Transform detachT;

	public bool ReadyForCleanup;

	public float lifeTime = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float elapsedTime;

	public Transform targetT;

	public Transform pivotT;

	public float overrideHeadSize = 1f;

	public float overrideHeadDismemberScaleTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startValuesSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public float startingHeadSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startingScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer detachRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> detachMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cFadeTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fadeOutInit;

	public string prefabPath => Data.prefabPath;

	public void SetDetachedTransform(Transform _detach, Transform _group)
	{
		detachT = _detach;
		detachRenderer = _group.GetComponentInChildren<MeshRenderer>();
	}

	public void FadeDetached(float value)
	{
		if (!detachRenderer)
		{
			return;
		}
		if (!fadeOutInit)
		{
			detachMats.Clear();
			detachMats.AddRange(detachRenderer.materials);
			fadeOutInit = true;
		}
		for (int i = 0; i < detachMats.Count; i++)
		{
			Material material = detachMats[i];
			if (material.HasProperty("_Fade"))
			{
				material.SetFloat("_Fade", value);
			}
		}
		detachRenderer.SetMaterials(detachMats);
	}

	public DismemberedPart(DismemberedPartData _data, uint _bodyDamageFlag, EnumDamageTypes _damageType)
	{
		Data = _data;
		bodyDamageFlag = _bodyDamageFlag;
		damageType = _damageType;
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
		if (!ReadyForCleanup)
		{
			if (elapsedTime > lifeTime - 0.5f)
			{
				fadeTime += Time.deltaTime;
				FadeDetached(Mathf.Lerp(1f, 0f, fadeTime / 0.5f));
			}
			if (elapsedTime >= lifeTime)
			{
				ReadyForCleanup = true;
			}
		}
	}

	public void Hide()
	{
		if ((bool)prefabT)
		{
			prefabT.gameObject.SetActive(value: false);
		}
	}

	public void CleanupDetached()
	{
		if ((bool)detachT)
		{
			Object.Destroy(detachT.gameObject);
			detachT = null;
		}
	}
}

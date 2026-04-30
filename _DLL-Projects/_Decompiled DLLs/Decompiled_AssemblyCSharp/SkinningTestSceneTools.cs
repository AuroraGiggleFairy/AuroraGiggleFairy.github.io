using System;
using UnityEngine;

public class SkinningTestSceneTools : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator anim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int layerIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int maxLayers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float turnRate = 600f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int totalModels;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBodyParts;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int randomMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetLayerWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float endLayerWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentModel;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		anim = GetComponent<Animator>();
		if (anim != null)
		{
			maxLayers = anim.layerCount;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Space) && anim != null)
		{
			layerIndex++;
			if (layerIndex == maxLayers)
			{
				layerIndex = 0;
				for (int i = 1; i < maxLayers - 1; i++)
				{
					anim.SetLayerWeight(i, 0f);
				}
			}
			targetLayerWeight = 0f;
			endLayerWeight = 1f;
		}
		if (layerIndex == 0)
		{
			endLayerWeight = Mathf.Lerp(endLayerWeight, 0f, 0.01f);
			anim.SetLayerWeight(maxLayers - 1, endLayerWeight);
		}
		else
		{
			targetLayerWeight = Mathf.Lerp(targetLayerWeight, 1f, 0.01f);
			anim.SetLayerWeight(layerIndex, targetLayerWeight);
		}
		if (Input.GetKey(KeyCode.A))
		{
			base.transform.Rotate(0f, turnRate * Time.deltaTime * -1f, 0f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.Rotate(0f, turnRate * Time.deltaTime, 0f);
		}
	}
}

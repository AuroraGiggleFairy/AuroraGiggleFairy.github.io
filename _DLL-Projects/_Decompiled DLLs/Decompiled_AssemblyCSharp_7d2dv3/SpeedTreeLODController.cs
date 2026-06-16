using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTreeLODController : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Tree> tempTrees = new List<Tree>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		GetComponentsInChildren(tempTrees);
		foreach (Tree tempTree in tempTrees)
		{
			if (tempTree.hasSpeedTreeWind && tempTree.TryGetComponent<Renderer>(out var component) && component.motionVectorGenerationMode == MotionVectorGenerationMode.Object)
			{
				tempTree.gameObject.AddMissingComponent<SpeedTreeMotionVectorHelper>().Init(component);
			}
		}
		tempTrees.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		LODGroup component = GetComponent<LODGroup>();
		int lodCount = component.lodCount;
		if (lodCount <= 0)
		{
			return;
		}
		LOD[] lODs = component.GetLODs();
		float screenRelativeTransitionHeight = Utils.FastLerp(0.02f, 0.03f, component.size / 5f);
		lODs[lodCount - 1].screenRelativeTransitionHeight = screenRelativeTransitionHeight;
		if (lodCount > 1)
		{
			float num = 0.17f;
			float num2 = (0.58f - num) / ((float)(lodCount - 2) + 0.001f);
			for (int num3 = lodCount - 2; num3 >= 0; num3--)
			{
				lODs[num3].screenRelativeTransitionHeight = num;
				num += num2;
			}
		}
		component.SetLODs(lODs);
	}
}

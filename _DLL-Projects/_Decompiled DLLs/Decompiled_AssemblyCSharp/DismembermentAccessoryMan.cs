using System.Collections.Generic;
using UnityEngine;

public class DismembermentAccessoryMan : MonoBehaviour
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftLowerArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftUpperArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftLowerLeg = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> LeftUpperLeg = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightLowerArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightUpperArm = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightLowerLeg = new List<GameObject>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> RightUpperLeg = new List<GameObject>();

	public void HidePart(EnumBodyPartHit bodyPart)
	{
		switch (bodyPart)
		{
		case EnumBodyPartHit.LeftLowerArm:
		{
			for (int l = 0; l < LeftLowerArm.Count; l++)
			{
				GameObject gameObject4 = LeftLowerArm[l];
				if ((bool)gameObject4)
				{
					gameObject4.SetActive(value: false);
				}
			}
			break;
		}
		case EnumBodyPartHit.LeftUpperArm:
		{
			for (int num = 0; num < LeftLowerArm.Count; num++)
			{
				GameObject gameObject7 = LeftLowerArm[num];
				if ((bool)gameObject7 && gameObject7.activeSelf)
				{
					gameObject7.SetActive(value: false);
				}
			}
			for (int num2 = 0; num2 < LeftUpperArm.Count; num2++)
			{
				GameObject gameObject8 = LeftUpperArm[num2];
				if ((bool)gameObject8)
				{
					gameObject8.SetActive(value: false);
				}
			}
			break;
		}
		case EnumBodyPartHit.LeftLowerLeg:
		{
			for (int k = 0; k < LeftLowerLeg.Count; k++)
			{
				GameObject gameObject3 = LeftLowerLeg[k];
				if ((bool)gameObject3)
				{
					gameObject3.SetActive(value: false);
				}
			}
			break;
		}
		case EnumBodyPartHit.LeftUpperLeg:
		{
			for (int num5 = 0; num5 < LeftLowerLeg.Count; num5++)
			{
				GameObject gameObject11 = LeftLowerLeg[num5];
				if ((bool)gameObject11 && gameObject11.activeSelf)
				{
					gameObject11.SetActive(value: false);
				}
			}
			for (int num6 = 0; num6 < LeftUpperLeg.Count; num6++)
			{
				GameObject gameObject12 = LeftUpperLeg[num6];
				if ((bool)gameObject12)
				{
					gameObject12.SetActive(value: false);
				}
			}
			break;
		}
		case EnumBodyPartHit.RightLowerArm:
		{
			for (int m = 0; m < RightLowerArm.Count; m++)
			{
				GameObject gameObject5 = RightLowerArm[m];
				if ((bool)gameObject5)
				{
					gameObject5.SetActive(value: false);
				}
			}
			break;
		}
		case EnumBodyPartHit.RightUpperArm:
		{
			for (int num3 = 0; num3 < RightLowerArm.Count; num3++)
			{
				GameObject gameObject9 = RightLowerArm[num3];
				if ((bool)gameObject9 && gameObject9.activeSelf)
				{
					gameObject9.SetActive(value: false);
				}
			}
			for (int num4 = 0; num4 < RightUpperArm.Count; num4++)
			{
				GameObject gameObject10 = RightUpperArm[num4];
				if ((bool)gameObject10)
				{
					gameObject10.SetActive(value: false);
				}
			}
			break;
		}
		case EnumBodyPartHit.RightLowerLeg:
		{
			for (int n = 0; n < RightLowerLeg.Count; n++)
			{
				GameObject gameObject6 = RightLowerLeg[n];
				if ((bool)gameObject6)
				{
					gameObject6.SetActive(value: false);
				}
			}
			break;
		}
		case EnumBodyPartHit.RightUpperLeg:
		{
			for (int i = 0; i < RightLowerLeg.Count; i++)
			{
				GameObject gameObject = RightLowerLeg[i];
				if ((bool)gameObject && gameObject.activeSelf)
				{
					gameObject.SetActive(value: false);
				}
			}
			for (int j = 0; j < RightUpperLeg.Count; j++)
			{
				GameObject gameObject2 = RightUpperLeg[j];
				if ((bool)gameObject2)
				{
					gameObject2.SetActive(value: false);
				}
			}
			break;
		}
		}
	}
}

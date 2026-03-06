using System.Collections;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class CWeightList
{
	public Transform transform;

	public ArrayList weights;

	public CWeightList()
	{
		weights = new ArrayList();
	}
}

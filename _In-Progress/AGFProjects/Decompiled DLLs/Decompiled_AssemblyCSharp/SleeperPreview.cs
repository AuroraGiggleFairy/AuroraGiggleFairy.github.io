using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SleeperPreview : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator animator;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void SetPose(int pose)
	{
	}

	public void SetRotation(float rot)
	{
		base.transform.rotation = Quaternion.AngleAxis(rot, Vector3.up);
	}
}

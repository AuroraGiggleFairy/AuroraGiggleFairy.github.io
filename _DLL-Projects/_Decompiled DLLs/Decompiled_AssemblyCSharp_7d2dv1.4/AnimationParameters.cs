using System;
using UnityEngine;

public class AnimationParameters : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator anim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 currentEulerAngles;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion currentRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion previousRotation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentYaw;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastYaw;

	public float turnPlayRateMultiplier;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float turnPlayRate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaYawTarget;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaYaw;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float angle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 angularVelocity;

	public bool debugMode;

	public float deltaYawMin;

	public float deltaYawMax;

	public float deltaYawSmoothTime = 0.3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float turnVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		anim = GetComponent<Animator>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		currentRotation = base.transform.rotation;
		Quaternion quaternion = currentRotation * Quaternion.Inverse(previousRotation);
		previousRotation = currentRotation;
		quaternion.ToAngleAxis(out var num, out var axis);
		num *= MathF.PI / 180f;
		angularVelocity = 1f / Time.deltaTime * num * axis;
		deltaYaw = Mathf.SmoothDamp(deltaYaw, angularVelocity.y, ref turnVelocity, deltaYawSmoothTime);
		if (debugMode)
		{
			if (Mathf.Abs(deltaYaw) > 0.001f)
			{
				Debug.Log("DeltaYaw: " + deltaYaw);
			}
			if (deltaYaw < deltaYawMin)
			{
				deltaYawMin = deltaYaw;
			}
			if (deltaYaw > deltaYawMax)
			{
				deltaYawMax = deltaYaw;
			}
		}
		anim.SetFloat("deltaYaw", deltaYaw);
		anim.SetFloat("TurnPlayRate", deltaYaw);
		if (Mathf.Abs(angularVelocity.y) > 0.1f)
		{
			anim.SetBool("Turning", value: true);
		}
		else
		{
			anim.SetBool("Turning", value: false);
		}
	}
}

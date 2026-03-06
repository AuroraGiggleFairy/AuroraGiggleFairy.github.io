using System;
using UnityEngine;

public class EyeLidController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum BlinkState
	{
		Open,
		Closing,
		Opening
	}

	public Transform leftTopTransform;

	public Transform leftBottomTransform;

	public Transform rightTopTransform;

	public Transform rightBottomTransform;

	public Vector3 leftTopLocalPosition;

	public Vector3 leftBottomLocalPosition;

	public Quaternion leftTopRotation;

	public Quaternion leftBottomRotation;

	public Vector3 rightTopLocalPosition;

	public Vector3 rightBottomLocalPosition;

	public Quaternion rightTopRotation;

	public Quaternion rightBottomRotation;

	public bool autoRecordEyeBoneTransforms;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float nextBlinkTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float blinkProgress;

	public bool debug;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityAlive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom random;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 topOffset = new Vector3(0f, 0f, 0.007f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 topRotation = new Vector3(40f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 bottomRotation = new Vector3(-10f, 0f, -10f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlinkState blinkState;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		entityAlive = GetComponentInParent<EntityAlive>();
		random = GameRandomManager.Instance.CreateGameRandom();
		nextBlinkTime = Time.time + random.RandomRange(1f, 5f);
		if (autoRecordEyeBoneTransforms)
		{
			leftTopLocalPosition = leftTopTransform.localPosition;
			leftBottomLocalPosition = leftBottomTransform.localPosition;
			leftTopRotation = leftTopTransform.localRotation;
			leftBottomRotation = leftBottomTransform.localRotation;
			rightTopLocalPosition = rightTopTransform.localPosition;
			rightBottomLocalPosition = rightBottomTransform.localPosition;
			rightTopRotation = rightTopTransform.localRotation;
			rightBottomRotation = rightBottomTransform.localRotation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (debug || (entityAlive != null && entityAlive.IsDead()))
		{
			blinkProgress = 1f;
		}
		else
		{
			if (Time.time > nextBlinkTime)
			{
				nextBlinkTime = Time.time + random.RandomRange(1f, 5f);
				blinkState = BlinkState.Closing;
			}
			switch (blinkState)
			{
			case BlinkState.Closing:
				blinkProgress += 20f * Time.deltaTime;
				if (blinkProgress >= 1f)
				{
					blinkProgress = 1f;
					blinkState = BlinkState.Opening;
				}
				break;
			case BlinkState.Opening:
				blinkProgress -= 10f * Time.deltaTime;
				if (blinkProgress <= 0f)
				{
					blinkProgress = 0f;
					blinkState = BlinkState.Open;
				}
				break;
			}
		}
		leftTopTransform.localPosition = leftTopLocalPosition + topOffset * blinkProgress;
		leftTopTransform.localRotation = Quaternion.Euler(topRotation * blinkProgress) * leftTopRotation;
		leftBottomTransform.localPosition = leftBottomLocalPosition;
		leftBottomTransform.localRotation = Quaternion.Euler(bottomRotation * blinkProgress) * leftBottomRotation;
		rightTopTransform.localPosition = rightTopLocalPosition + topOffset * blinkProgress;
		rightTopTransform.localRotation = Quaternion.Euler(topRotation * blinkProgress) * rightTopRotation;
		rightBottomTransform.localPosition = rightBottomLocalPosition;
		rightBottomTransform.localRotation = Quaternion.Euler(bottomRotation * blinkProgress) * rightBottomRotation;
	}
}

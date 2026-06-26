using System;
using UnityEngine;

namespace Assets.DuckType.Jiggle;

[PublicizedFrom(EAccessModifier.Internal)]
public class ProceduralAnimation : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_RestPos;

	public bool MoveAlongX;

	public bool ForwardAndBackward;

	public bool UpAndDown;

	public bool SideToSide;

	public bool Bounce;

	public float TranslationMultiplier = 1f;

	public bool RotateX;

	public bool RotateY;

	public float RotationMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_RestPos = base.transform.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float num = (MoveAlongX ? (Time.time * TranslationMultiplier) : 0f);
		if (ForwardAndBackward)
		{
			num += GetSineValue(Bounce, TranslationMultiplier);
		}
		base.transform.position = m_RestPos + new Vector3(num, UpAndDown ? GetSineValue(Bounce, TranslationMultiplier) : 0f, SideToSide ? GetSineValue(Bounce, TranslationMultiplier) : 0f);
		base.transform.rotation = Quaternion.Euler(RotateX ? (Mathf.Sin(Time.time * 6f) * 30f * RotationMultiplier) : base.transform.eulerAngles.x, RotateY ? (Mathf.Sin(Time.time * 6f) * 30f * RotationMultiplier) : base.transform.eulerAngles.y, base.transform.eulerAngles.z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetSineValue(bool bounce, float mult)
	{
		float num = Mathf.Sin(Time.time * 6f) * 3f * mult;
		if (!bounce)
		{
			return num;
		}
		return Mathf.Abs(num);
	}
}

using System;
using UnityEngine;

public class LightLODHeld : LightLOD
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHeightCheckY = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCastOffset = 0.02f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCastRadius = 0.15f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cClosestDist = 0.15f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMask = -554734598;

	public float flickerRadius;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 flickerOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 flickerTargetOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform rootT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 parentStartPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		startPos = selfT.localPosition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		Transform transform = selfT;
		if (!rootT)
		{
			rootT = RootTransformRefEntity.FindEntityUpwards(transform);
			if ((bool)rootT)
			{
				player = rootT.GetComponent<EntityPlayerLocal>();
				parentStartPos = parentT.localPosition;
			}
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		bool flag = (bool)player && player.bFirstPersonView;
		Vector3 position;
		if (flickerRadius > 0f)
		{
			flickerTargetOffset = world.GetGameRandom().RandomOnUnitSphere;
			flickerOffset = Vector3.Lerp(flickerOffset, flickerTargetOffset, 0.2f);
			transform.localPosition = startPos + flickerOffset * flickerRadius;
			position = transform.position;
			if (!flag)
			{
				position.y += 0.2f;
			}
		}
		else if (flag)
		{
			transform.localPosition = startPos;
			position = transform.position;
		}
		else
		{
			parentT.localPosition = Vector3.Lerp(parentStartPos, parentT.localPosition, 0.1f);
			transform.localPosition = startPos;
			position = transform.position;
			position.y += 0.2f;
		}
		Vector3 position2 = rootT.position;
		position2.y += 0.5f;
		if (Physics.Raycast(position2, Vector3.up, out var hitInfo, 1.4f, -554734598))
		{
			position2.y = hitInfo.point.y - 0.22f - 0.15f;
		}
		else
		{
			position2.y += 0.95000005f;
		}
		if (position2.y > position.y + 0.25f)
		{
			position2.y = position.y + 0.25f;
		}
		Vector3 vector = position - position2;
		float magnitude = vector.magnitude;
		vector *= 1f / magnitude;
		magnitude = ((!flag) ? (magnitude * 1.5f) : (magnitude * 0.72f));
		position = position2 + vector * magnitude;
		position2 += vector * 0.02f;
		if (Physics.SphereCast(position2, 0.15f, vector, out hitInfo, magnitude - 0.15f - 0.02f + 0.15f, -554734598))
		{
			position2 = hitInfo.point;
			position2 += hitInfo.normal * 0.15f;
			transform.position = position2;
		}
		else
		{
			transform.position = position;
		}
	}
}

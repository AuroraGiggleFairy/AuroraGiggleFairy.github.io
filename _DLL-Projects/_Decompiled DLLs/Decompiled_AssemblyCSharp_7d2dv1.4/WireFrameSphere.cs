using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WireFrameSphere : MonoBehaviour
{
	public Vector3 center;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float TAU = MathF.PI * 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float radius = 1f;

	public float newRadius = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LineRenderer lr;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> positions;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int numOfVertices = 100;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float angle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerCamera player;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		lr = base.gameObject.GetComponent<LineRenderer>();
		lr.startWidth = 0.01f;
		lr.endWidth = 0.01f;
		positions = new List<Vector3>();
		player = UnityEngine.Object.FindObjectOfType<LocalPlayerCamera>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (player != null)
		{
			float num = Mathf.Abs(player.transform.position.magnitude - base.transform.position.magnitude);
			LineRenderer lineRenderer = lr;
			float startWidth = (lr.endWidth = 0.01f * num);
			lineRenderer.startWidth = startWidth;
		}
		if (radius != newRadius)
		{
			radius = newRadius;
			positions.Clear();
			positions.AddRange(RenderCircleOnPlane(xyPlane: true));
			positions.AddRange(RenderCircleOnPlane(xyPlane: false));
			lr.positionCount = positions.Count;
			lr.SetPositions(positions.ToArray());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> RenderCircleOnPlane(bool xyPlane)
	{
		numOfVertices = 100 + (int)Math.Pow((int)radius, 2.0);
		List<Vector3> list = new List<Vector3>(numOfVertices);
		angle = MathF.PI * 2f / (float)(numOfVertices - 1);
		for (int i = 0; i < numOfVertices; i++)
		{
			float x = center.x + radius * Mathf.Cos((float)i * angle);
			float y = center.y + (xyPlane ? (radius * Mathf.Sin((float)i * angle)) : 0f);
			float z = center.z + (xyPlane ? 0f : (radius * Mathf.Sin((float)i * angle)));
			list.Add(new Vector3(x, y, z));
		}
		return list;
	}

	public void KillWF()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}

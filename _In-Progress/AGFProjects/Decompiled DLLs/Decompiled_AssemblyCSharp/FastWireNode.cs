using System;
using System.Collections.Generic;
using UnityEngine;

public class FastWireNode : MonoBehaviour, IWireNode
{
	public const int cLayerMaskRayCast = 65537;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int NODE_COUNT = 15;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float BASE_WIRE_RADIUS = 0.01f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float BASE_MIN_WIRE_DIP = 0f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const float BASE_MAX_WIRE_DIP = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static Material BaseMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxWireDip = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float wireRadius = 0.01f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPosition;

	public Vector3 StartOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 endPosition;

	public Vector3 EndOffset;

	public Color pulseColor = Color.yellow;

	public Color wireColor = Color.black;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool canHide = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh mesh;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter meshFilter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshCollider meshCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer meshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color prevWireColor = Color.white;

	public Vector3 StartPosition
	{
		get
		{
			return startPosition;
		}
		set
		{
			startPosition = value;
		}
	}

	public Vector3 EndPosition
	{
		get
		{
			return endPosition;
		}
		set
		{
			endPosition = value;
		}
	}

	public void Awake()
	{
		if (BaseMaterial == null)
		{
			BaseMaterial = Resources.Load<Material>("Materials/WireMaterial");
		}
		if (meshFilter == null)
		{
			meshFilter = base.transform.gameObject.AddMissingComponent<MeshFilter>();
		}
		if (meshCollider == null)
		{
			meshCollider = base.transform.gameObject.AddMissingComponent<MeshCollider>();
			meshCollider.convex = true;
			meshCollider.isTrigger = true;
		}
		if (meshRenderer == null)
		{
			meshRenderer = base.transform.gameObject.AddMissingComponent<MeshRenderer>();
			meshRenderer.material = BaseMaterial;
		}
		Utils.SetColliderLayerRecursively(base.gameObject, 29);
	}

	public void BuildMesh()
	{
		Vector3 vector = EndPosition + EndOffset;
		Vector3 vector2 = StartPosition + StartOffset;
		float num = Vector3.Distance(vector, vector2);
		if (num > 256f || num < 0.01f)
		{
			return;
		}
		Vector3 vector3 = vector2 - vector;
		if (vector3.magnitude == 0f)
		{
			return;
		}
		Vector3 vector4 = vector3 / 16f;
		if (vector4.magnitude == 0f)
		{
			return;
		}
		Vector3 vector5 = vector + vector4;
		List<Vector3> list = new List<Vector3>();
		list.Add(vector);
		for (int i = 0; i < 15; i++)
		{
			if (vector3.normalized != Vector3.up && vector3.normalized != Vector3.down)
			{
				float num2 = Mathf.Abs((8f - (float)(i + 1)) / 8f);
				num2 *= num2;
				list.Add(vector5 - new Vector3(0f, Mathf.Lerp(0f, maxWireDip, 1f - num2), 0f));
			}
			else
			{
				list.Add(vector5);
			}
			vector5 += vector4;
		}
		if (list.Count > 1)
		{
			list[0] = vector;
			list[list.Count - 1] = vector2;
		}
		Vector3 vector6 = Vector3.one * float.PositiveInfinity;
		Vector3 vector7 = -Vector3.one * float.PositiveInfinity;
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector2> list4 = new List<Vector2>();
		List<Vector2> list5 = new List<Vector2>();
		int[] array = new int[396];
		for (int j = 0; j < list.Count; j++)
		{
			float num3 = (float)j / (float)list.Count * (num * 0.25f);
			list4.Add(Vector2.right * num3);
			list4.Add(Vector2.right * num3 + Vector2.up);
			list4.Add(Vector2.right * num3);
			list4.Add(Vector2.right * num3 + Vector2.up);
			list5.Add(Vector2.zero);
			list5.Add(Vector2.zero);
			list5.Add(Vector2.zero);
			list5.Add(Vector2.zero);
			if (j > 0)
			{
				vector3 = list[j] - list[j - 1];
			}
			Vector3 normalized = Vector3.Cross(Vector3.up, vector3.normalized).normalized;
			Vector3 normalized2 = Vector3.Cross(vector3.normalized, normalized).normalized;
			if (vector3.normalized == Vector3.up || vector3.normalized == Vector3.down)
			{
				normalized = Vector3.Cross(Vector3.forward, vector3.normalized).normalized;
				normalized2 = Vector3.Cross(vector3.normalized, normalized).normalized;
			}
			list2.Add(normalized2 * wireRadius + list[j]);
			list2.Add(-normalized2 * wireRadius + list[j]);
			list2.Add(normalized * wireRadius + list[j]);
			list2.Add(-normalized * wireRadius + list[j]);
			if (j == 0)
			{
				normalized2 = Vector3.Lerp(normalized2, (vector - vector2).normalized, 0.5f).normalized;
				normalized = Vector3.Lerp(normalized, (vector - vector2).normalized, 0.5f).normalized;
			}
			else if (j == list.Count - 1)
			{
				normalized2 = Vector3.Lerp(normalized2, -(vector - vector2).normalized, 0.5f).normalized;
				normalized = Vector3.Lerp(normalized, -(vector - vector2).normalized, 0.5f).normalized;
			}
			list3.Add(normalized2);
			list3.Add(-normalized2);
			list3.Add(normalized);
			list3.Add(-normalized);
			if (list[j].x < vector6.x)
			{
				vector6.x = list[j].x;
			}
			if (list[j].x > vector7.x)
			{
				vector7.x = list[j].x;
			}
			if (list[j].y < vector6.y)
			{
				vector6.y = list[j].y;
			}
			if (list[j].y > vector7.y)
			{
				vector7.y = list[j].y;
			}
			if (list[j].z < vector6.z)
			{
				vector6.z = list[j].z;
			}
			if (list[j].z > vector7.z)
			{
				vector7.z = list[j].z;
			}
		}
		int num4 = 0;
		for (int k = 0; k < 15; k++)
		{
			array[num4++] = 4 * k;
			array[num4++] = 4 + 4 * k;
			array[num4++] = 7 + 4 * k;
			array[num4++] = 7 + 4 * k;
			array[num4++] = 3 + 4 * k;
			array[num4++] = 4 * k;
			array[num4++] = 4 + 4 * k;
			array[num4++] = 4 * k;
			array[num4++] = 2 + 4 * k;
			array[num4++] = 2 + 4 * k;
			array[num4++] = 6 + 4 * k;
			array[num4++] = 4 + 4 * k;
			array[num4++] = 3 + 4 * k;
			array[num4++] = 7 + 4 * k;
			array[num4++] = 5 + 4 * k;
			array[num4++] = 5 + 4 * k;
			array[num4++] = 1 + 4 * k;
			array[num4++] = 3 + 4 * k;
			array[num4++] = 6 + 4 * k;
			array[num4++] = 2 + 4 * k;
			array[num4++] = 1 + 4 * k;
			array[num4++] = 1 + 4 * k;
			array[num4++] = 5 + 4 * k;
			array[num4++] = 6 + 4 * k;
		}
		array[num4++] = 0;
		array[num4++] = 3;
		array[num4++] = 1;
		array[num4++] = 1;
		array[num4++] = 2;
		array[num4++] = 0;
		array[num4++] = 60;
		array[num4++] = 62;
		array[num4++] = 61;
		array[num4++] = 61;
		array[num4++] = 63;
		array[num4++] = 60;
		if (list2.Count >= 3 && array.Length >= 3)
		{
			if (mesh == null)
			{
				mesh = new Mesh();
			}
			mesh.SetVertices(list2);
			mesh.uv = list4.ToArray();
			mesh.uv2 = list5.ToArray();
			mesh.SetNormals(list3);
			mesh.SetIndices(array, MeshTopology.Triangles, 0);
			mesh.RecalculateBounds();
			meshFilter.mesh = mesh;
			meshCollider.sharedMesh = mesh;
			if (prevWireColor != wireColor)
			{
				prevWireColor = wireColor;
				SetWireColor(wireColor);
			}
		}
	}

	public void SetWireColor(Color color)
	{
		if (!(meshRenderer.material == null))
		{
			meshRenderer.material.SetColor("_Color", color);
			wireColor = color;
		}
	}

	public void SetPulseSpeed(float speed)
	{
		if (!(meshRenderer.material == null))
		{
			meshRenderer.material.SetFloat("_PulseSpeed", speed);
		}
	}

	public void SetPulseColor(Color color)
	{
		pulseColor = color;
	}

	public void TogglePulse(bool isOn)
	{
		if (!(meshRenderer.material == null))
		{
			meshRenderer.material.SetColor("_PulseColor", isOn ? pulseColor : wireColor);
		}
	}

	public void SetStartPosition(Vector3 pos)
	{
		StartPosition = pos;
	}

	public void SetStartPositionOffset(Vector3 pos)
	{
		StartOffset = pos;
	}

	public void SetEndPosition(Vector3 pos)
	{
		EndPosition = pos;
	}

	public void SetEndPositionOffset(Vector3 pos)
	{
		EndOffset = pos;
	}

	public void SetWireDip(float _dist)
	{
		maxWireDip = _dist;
	}

	public float GetWireDip()
	{
		return maxWireDip;
	}

	public void SetWireRadius(float _radius)
	{
		wireRadius = _radius;
	}

	public void SetWireCanHide(bool _canHide)
	{
		canHide = _canHide;
	}

	public Vector3 GetStartPosition()
	{
		return StartPosition;
	}

	public Vector3 GetStartPositionOffset()
	{
		return StartOffset;
	}

	public Vector3 GetEndPosition()
	{
		return EndPosition;
	}

	public Vector3 GetEndPositionOffset()
	{
		return EndOffset;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public void SetVisible(bool _visible)
	{
		if (canHide)
		{
			base.gameObject.SetActive(_visible);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public Bounds GetBounds()
	{
		return mesh.bounds;
	}

	public void Reset()
	{
		maxWireDip = 0.25f;
		wireRadius = 0.01f;
		pulseColor = Color.yellow;
	}
}

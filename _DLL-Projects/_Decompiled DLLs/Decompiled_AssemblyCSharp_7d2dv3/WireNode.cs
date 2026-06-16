using System;
using System.Collections.Generic;
using UnityEngine;

public class WireNode : MonoBehaviour, IWireNode
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct ShockSpot
	{
		public float timer;

		public int shockDataIndex;

		public int vertex;

		public GameObject shockPrefab;
	}

	public const int cLayerMaskRayCast = 65537;

	public Vector2[] shocks;

	public Material material;

	public GameObject Source;

	public GameObject parentGO;

	public Color pulseColor = new Color32(0, 97, byte.MaxValue, byte.MaxValue);

	public Color wireColor = Color.black;

	public float size;

	public float weightMod;

	public float springMod;

	public float minSegmentLength = 0.25f;

	public float snagThreshold = 0.5f;

	public float tensionMultiplier = 2f;

	public float playerHeight = 1.8f;

	public Vector3 SourcePosition = Vector3.zero;

	public Vector3 LocalPosition = Vector3.one;

	public Vector3 localOffset = Vector3.zero;

	public Vector3 sourceOffset = Vector3.zero;

	public Vector3 cameraOffset;

	public bool attatchSoureToCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float playerShockTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentTotalForces = 1000f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 prevSourcePos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentSegmentLength;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject rootShockPrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] shockTimers = new float[8];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] shockIndices = new int[8];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] currentShockingVertex = new int[8];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ShockSpot> shockSpots;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 min = Vector3.one * float.PositiveInfinity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 max = -Vector3.one * float.PositiveInfinity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 prevSourcePosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 prevLocalPosition = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prevParentGO;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float prevSize = 0.01f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material personalMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int numNodes = 15;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color prevWireColor = Color.black;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float prevWeightMod = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float prevSpringMod = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform prevParent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh mesh;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter meshFilter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer meshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshCollider meshCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> points;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> forces;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> verts;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2> uvs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2> uvs2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> normals;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<bool> shockNodes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] indices;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool usingLocalPosition = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool usingSourcePosition = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float checkPlayerConnectTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int attachedNode = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int prevAttachedNode = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float slopeSpeed = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 currLocalPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 currSourcePosition;

	public float drag = 0.89f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 cachedSourcePosition;

	public void Init()
	{
		if (rootShockPrefab == null)
		{
			rootShockPrefab = Resources.Load<GameObject>("Prefabs/ElectricShock");
		}
		prevParent = (parent = base.transform.parent);
		if (parent != null)
		{
			prevParentGO = (parentGO = parent.gameObject);
			usingLocalPosition = false;
		}
		shockSpots = new List<ShockSpot>();
		base.transform.parent = null;
		base.transform.position = Vector3.zero;
		base.transform.localEulerAngles = Vector3.zero;
		base.transform.localScale = Vector3.one;
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
		meshCollider.convex = true;
		meshCollider.isTrigger = true;
		mesh = new Mesh();
		mesh.MarkDynamic();
		shockNodes = new List<bool>();
		points = new List<Vector3>();
		forces = new List<Vector3>();
		verts = new List<Vector3>();
		uvs = new List<Vector2>();
		uvs2 = new List<Vector2>();
		normals = new List<Vector3>();
		indices = new int[24 * (1 + numNodes) + 12];
		if (personalMaterial == null)
		{
			personalMaterial = UnityEngine.Object.Instantiate(material);
		}
		meshRenderer.material = personalMaterial;
	}

	public void OnDestroy()
	{
		if (personalMaterial != null)
		{
			UnityEngine.Object.Destroy(personalMaterial);
			personalMaterial = null;
		}
		for (int i = 0; i < shockSpots.Count; i++)
		{
			UnityEngine.Object.Destroy(shockSpots[i].shockPrefab);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Init();
	}

	public void ToggleMeshCollider(bool _bOn)
	{
		meshCollider.enabled = _bOn;
	}

	public void SetPulseSpeed(float speed)
	{
		if (!(personalMaterial == null))
		{
			personalMaterial.SetFloat("_PulseSpeed", speed);
		}
	}

	public void TogglePulse(bool isOn)
	{
		if (!(personalMaterial == null))
		{
			personalMaterial.SetColor("_PulseColor", isOn ? pulseColor : Color.black);
		}
	}

	public void SetPulseColor(Color color)
	{
		pulseColor = color;
	}

	public void PlayShockAtPosition(Vector3 shockPosition)
	{
		if (shockPosition.x < min.x - 1f || shockPosition.y < min.y - 1f || shockPosition.z < min.z - 1f || shockPosition.x > max.x + 1f || shockPosition.y > max.y + 1f || shockPosition.z > max.z + 1f)
		{
			return;
		}
		int num = 99999;
		float num2 = 99999f;
		for (int i = 1; i < points.Count - 1; i++)
		{
			Vector3 vector = points[i] - shockPosition;
			if (vector.magnitude < num2)
			{
				num2 = vector.magnitude;
				num = i;
			}
		}
		if (!(num2 > 1f) && (num >= shockNodes.Count || !shockNodes[num]))
		{
			ShockSpot item = default(ShockSpot);
			item.timer = Time.time;
			item.vertex = num;
			item.shockDataIndex = 0;
			item.shockPrefab = UnityEngine.Object.Instantiate(rootShockPrefab);
			item.shockPrefab.transform.position = points[num];
			item.shockPrefab.transform.parent = base.transform;
			shockSpots.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePlayerIntersection()
	{
		if (!(GameManager.Instance == null) && GameManager.Instance.World != null)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (!(primaryPlayer == null) && Time.time > playerShockTimer + 0.25f)
			{
				playerShockTimer = Time.time;
				PlayShockAtPosition(primaryPlayer.GetPosition());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RayCastHeightAt(ref Vector3 position)
	{
		if (Physics.Raycast(new Ray(position + Vector3.up - Origin.position, Vector3.down), out var hitInfo, 1.75f, 65537) && hitInfo.point.y > position.y)
		{
			position.y = hitInfo.point.y + 0.01f - Origin.position.y;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateForces()
	{
		if (points.Count < 3)
		{
			return;
		}
		float magnitude = (points[points.Count - 1] - points[0]).magnitude;
		if (magnitude == 0f)
		{
			return;
		}
		float value = magnitude / 10000f;
		value = Mathf.Clamp(value, 1E-05f, 10f);
		float num = ((weightMod > 0f) ? (value * weightMod) : value);
		Vector3 vector = Vector3.down * 9.81f * num;
		float value2 = 0.35f * ((weightMod > 0f) ? (1f - weightMod * value) : (1f - value)) * springMod;
		value2 = Mathf.Clamp(value2, 0.001f, 0.4999f);
		for (int i = 1; i < points.Count - 1; i++)
		{
			Vector3 vector2 = points[i - 1] - points[i];
			vector2 *= tensionMultiplier;
			vector2 *= Mathf.Clamp01(vector2.magnitude - minSegmentLength);
			Vector3 vector3 = points[i + 1] - points[i];
			vector3 *= tensionMultiplier;
			vector3 *= Mathf.Clamp01(vector3.magnitude - minSegmentLength);
			Vector3 vector4 = vector2 + vector3;
			float num2 = ((i >= points.Count - 4) ? Mathf.Clamp01(1.1f - (float)(i - (points.Count - 5)) / 3f) : 1f);
			float num3 = value2 * num2;
			Vector3 vector5 = vector * (1f + (1f - num2));
			forces[i] = forces[i] * drag + (vector4 + vector5) * num3;
		}
		currentTotalForces = 0f;
		for (int j = 1; j < points.Count - 1; j++)
		{
			if (forces[j].magnitude > 0f && forces[j].magnitude < snagThreshold && Physics.Raycast(new Ray(points[j] - Origin.position, forces[j].normalized), out var hitInfo, forces[j].magnitude, 65537) && hitInfo.distance < forces[j].magnitude)
			{
				forces[j] = Vector3.zero;
			}
			points[j] += forces[j];
			currentTotalForces += forces[j].magnitude;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreatePoints()
	{
		Vector3 vector = currSourcePosition - currLocalPosition;
		if (vector.magnitude == 0f)
		{
			return;
		}
		Vector3 vector2 = vector / (1 + numNodes);
		if (vector2.magnitude == 0f)
		{
			return;
		}
		Vector3 vector3 = currLocalPosition + vector2;
		if (points.Count == 0)
		{
			Dictionary<Vector3, bool> dictionary = new Dictionary<Vector3, bool>(Vector3EqualityComparer.Instance);
			dictionary.Add(currLocalPosition, value: true);
			points.Add(currLocalPosition);
			forces.Add(Vector3.zero);
			shockNodes.Add(item: false);
			for (int i = 0; i < numNodes; i++)
			{
				forces.Add(Vector3.zero);
				shockNodes.Add(item: false);
				if (dictionary.ContainsKey(vector3))
				{
					points.Clear();
					forces.Clear();
					shockNodes.Clear();
					return;
				}
				points.Add(vector3);
				dictionary.Add(vector3, value: true);
				vector3 += vector2;
			}
		}
		if (points.Count > 1)
		{
			points[0] = currLocalPosition;
			points[points.Count - 1] = currSourcePosition;
		}
	}

	public void BuildMesh()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		usingSourcePosition = Source == null;
		if (!usingLocalPosition && parentGO == null)
		{
			usingLocalPosition = true;
			return;
		}
		Vector3 vector = currLocalPosition;
		Vector3 vector2 = currSourcePosition;
		if (prevWireColor != wireColor)
		{
			prevWireColor = wireColor;
			personalMaterial.SetColor("_WireColor", wireColor);
		}
		for (int num = shockSpots.Count - 1; num >= 0; num--)
		{
			if (Time.time > shockSpots[num].timer + shocks[shockSpots[num].shockDataIndex].x)
			{
				ShockSpot value = shockSpots[num];
				value.timer = Time.time;
				value.shockDataIndex++;
				if (value.shockDataIndex >= shocks.Length)
				{
					UnityEngine.Object.Destroy(value.shockPrefab);
					shockSpots.RemoveAt(num);
					shockNodes[num] = false;
				}
				else
				{
					shockSpots[num] = value;
				}
			}
		}
		currLocalPosition = localOffset + (usingLocalPosition ? LocalPosition : parent.position);
		Vector3 vector3 = sourceOffset;
		if (!usingSourcePosition)
		{
			vector3 = sourceOffset.x * Camera.main.transform.right;
			vector3 += sourceOffset.y * Camera.main.transform.up;
			vector3 += sourceOffset.z * Camera.main.transform.forward;
		}
		currSourcePosition = vector3 + (usingSourcePosition ? SourcePosition : cachedSourcePosition);
		if (currSourcePosition == currLocalPosition)
		{
			return;
		}
		prevLocalPosition = currLocalPosition;
		prevSourcePosition = currSourcePosition;
		prevWeightMod = weightMod;
		prevSpringMod = springMod;
		prevSize = size;
		mesh.Clear(keepVertexLayout: false);
		verts.Clear();
		uvs.Clear();
		uvs2.Clear();
		normals.Clear();
		CreatePoints();
		UpdateForces();
		uvs.Add(Vector2.zero);
		uvs.Add(Vector2.zero);
		uvs.Add(Vector2.zero);
		uvs.Add(Vector2.zero);
		uvs2.Add(Vector2.zero);
		uvs2.Add(Vector2.zero);
		uvs2.Add(Vector2.zero);
		uvs2.Add(Vector2.zero);
		for (int i = 1; i < points.Count; i++)
		{
			uvs.Add(Vector2.right * i / points.Count);
			uvs.Add(Vector2.right * i / points.Count + Vector2.up);
			uvs.Add(Vector2.right * i / points.Count);
			uvs.Add(Vector2.right * i / points.Count + Vector2.up);
			bool flag = false;
			for (int j = 0; j < shockSpots.Count; j++)
			{
				if (shockSpots[j].vertex == i)
				{
					uvs2.Add(Vector2.right * shocks[shockSpots[j].shockDataIndex].y);
					uvs2.Add(Vector2.right * shocks[shockSpots[j].shockDataIndex].y);
					uvs2.Add(Vector2.right * shocks[shockSpots[j].shockDataIndex].y);
					uvs2.Add(Vector2.right * shocks[shockSpots[j].shockDataIndex].y);
					flag = true;
					shockNodes[i] = true;
					break;
				}
			}
			if (!flag)
			{
				uvs2.Add(Vector2.zero);
				uvs2.Add(Vector2.zero);
				uvs2.Add(Vector2.zero);
				uvs2.Add(Vector2.zero);
			}
		}
		Vector3 vector4 = vector2 - vector;
		min = Vector3.one * float.PositiveInfinity;
		max = -Vector3.one * float.PositiveInfinity;
		for (int k = 0; k < points.Count; k++)
		{
			if (k > 0)
			{
				vector4 = points[k] - points[k - 1];
			}
			Vector3 normalized = Vector3.Cross(Vector3.up, vector4.normalized).normalized;
			Vector3 normalized2 = Vector3.Cross(vector4.normalized, normalized).normalized;
			verts.Add(normalized2 * size + points[k]);
			verts.Add(-normalized2 * size + points[k]);
			verts.Add(normalized * size + points[k]);
			verts.Add(-normalized * size + points[k]);
			if (k == 0)
			{
				normalized2 = Vector3.Lerp(normalized2, (vector2 - vector).normalized, 0.5f).normalized;
				normalized = Vector3.Lerp(normalized, (vector2 - vector).normalized, 0.5f).normalized;
			}
			else if (k == points.Count - 1)
			{
				normalized2 = Vector3.Lerp(normalized2, -(vector2 - vector).normalized, 0.5f).normalized;
				normalized = Vector3.Lerp(normalized, -(vector2 - vector).normalized, 0.5f).normalized;
			}
			normals.Add(normalized2);
			normals.Add(-normalized2);
			normals.Add(normalized);
			normals.Add(-normalized);
			if (points[k].x < min.x)
			{
				min.x = points[k].x;
			}
			if (points[k].x > max.x)
			{
				max.x = points[k].x;
			}
			if (points[k].y < min.y)
			{
				min.y = points[k].y;
			}
			if (points[k].y > max.y)
			{
				max.y = points[k].y;
			}
			if (points[k].z < min.z)
			{
				min.z = points[k].z;
			}
			if (points[k].z > max.z)
			{
				max.z = points[k].z;
			}
		}
		if (((min.x == max.x) ? 1 : ((0f + min.y == max.y) ? 1 : ((0f + min.z == max.z) ? 1 : 0))) < 2)
		{
			int num2 = 0;
			for (int l = 0; l < numNodes; l++)
			{
				indices[num2++] = 4 * l;
				indices[num2++] = 4 + 4 * l;
				indices[num2++] = 7 + 4 * l;
				indices[num2++] = 7 + 4 * l;
				indices[num2++] = 3 + 4 * l;
				indices[num2++] = 4 * l;
				indices[num2++] = 4 + 4 * l;
				indices[num2++] = 4 * l;
				indices[num2++] = 2 + 4 * l;
				indices[num2++] = 2 + 4 * l;
				indices[num2++] = 6 + 4 * l;
				indices[num2++] = 4 + 4 * l;
				indices[num2++] = 3 + 4 * l;
				indices[num2++] = 7 + 4 * l;
				indices[num2++] = 5 + 4 * l;
				indices[num2++] = 5 + 4 * l;
				indices[num2++] = 1 + 4 * l;
				indices[num2++] = 3 + 4 * l;
				indices[num2++] = 6 + 4 * l;
				indices[num2++] = 2 + 4 * l;
				indices[num2++] = 1 + 4 * l;
				indices[num2++] = 1 + 4 * l;
				indices[num2++] = 5 + 4 * l;
				indices[num2++] = 6 + 4 * l;
			}
			indices[num2++] = 0;
			indices[num2++] = 3;
			indices[num2++] = 1;
			indices[num2++] = 1;
			indices[num2++] = 2;
			indices[num2++] = 0;
			indices[num2++] = 4 + 4 * (numNodes - 1);
			indices[num2++] = 6 + 4 * (numNodes - 1);
			indices[num2++] = 5 + 4 * (numNodes - 1);
			indices[num2++] = 5 + 4 * (numNodes - 1);
			indices[num2++] = 7 + 4 * (numNodes - 1);
			indices[num2++] = 4 + 4 * (numNodes - 1);
			if (verts.Count >= 3 && uvs.Count >= 3 && uvs2.Count >= 3 && normals.Count >= 3 && indices.Length >= 3)
			{
				mesh.SetVertices(verts);
				mesh.uv = uvs.ToArray();
				mesh.uv2 = uvs2.ToArray();
				mesh.SetNormals(normals);
				mesh.SetIndices(indices, MeshTopology.Triangles, 0);
				meshFilter.mesh = mesh;
			}
		}
	}

	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MyUpdate()
	{
		if (Source != null)
		{
			if (cachedSourcePosition != Source.transform.position)
			{
				cachedSourcePosition = Source.transform.position;
				BuildMesh();
			}
		}
		else
		{
			BuildMesh();
		}
	}

	public void FixedUpdate()
	{
		MyUpdate();
	}

	public void LateUpdate()
	{
		MyUpdate();
	}

	public Vector3 GetStartPosition()
	{
		return SourcePosition;
	}

	public Vector3 GetStartPositionOffset()
	{
		return sourceOffset;
	}

	public void SetStartPosition(Vector3 pos)
	{
		SourcePosition = pos;
	}

	public void SetStartPositionOffset(Vector3 pos)
	{
		sourceOffset = pos;
	}

	public Vector3 GetEndPosition()
	{
		return LocalPosition;
	}

	public Vector3 GetEndPositionOffset()
	{
		return localOffset;
	}

	public void SetEndPosition(Vector3 pos)
	{
		LocalPosition = pos;
	}

	public void SetEndPositionOffset(Vector3 pos)
	{
		localOffset = pos;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public Bounds GetBounds()
	{
		return mesh.bounds;
	}

	public void SetWireDip(float _dist)
	{
	}

	public float GetWireDip()
	{
		return 0f;
	}

	public void SetWireRadius(float _radius)
	{
	}

	public void Reset()
	{
		pulseColor = new Color32(0, 97, byte.MaxValue, byte.MaxValue);
	}

	public void SetWireCanHide(bool _canHide)
	{
	}

	public void SetVisible(bool _visible)
	{
	}
}

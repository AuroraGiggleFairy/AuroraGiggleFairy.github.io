using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeNew : BlockShape
{
	public enum EnumFaceOcclusionInfo
	{
		None,
		Part,
		Full,
		Remove,
		Continuous,
		RemoveIfAny,
		OwnFaces,
		HideIfSame,
		Transparent
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MySimpleMesh
	{
		public List<ushort> Indices = new List<ushort>();

		public List<Vector2> Uvs = new List<Vector2>();

		public List<Vector3> Vertices = new List<Vector3>();

		public List<Vector3> Normals = new List<Vector3>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MeshData
	{
		public class Arrays
		{
			public MySimpleMesh[] meshes = new MySimpleMesh[7];

			public MySimpleMesh[] colliderMeshes = new MySimpleMesh[7];

			public EnumFaceOcclusionInfo[] faceInfo = new EnumFaceOcclusionInfo[7];

			public Bounds[] boundsRotations = new Bounds[32];
		}

		public GameObject obj;

		public int symTypeOverride = -1;

		public bool IsSolidCube;

		public Dictionary<Vector3, Arrays> posArrays = new Dictionary<Vector3, Arrays>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum PlaneAxis
	{
		X,
		Y,
		Z
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PlanePick
	{
		public PlaneAxis axis;

		public bool neg;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRotationsUsed = 28;

	public static bool bImposterGenerationActive;

	public string ShapeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public MySimpleMesh[] visualMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public MySimpleMesh[] colliderMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumFaceOcclusionInfo[] faceInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 centerOffsetV;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds[] boundsRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2[] boundsPathOffsetRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[,] convertRotationCached;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, MeshData> meshData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Quaternion[] rotationsToQuats;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[,] rotations;

	public BlockShapeNew()
	{
		IsSolidCube = false;
		IsSolidSpace = false;
		IsRotatable = true;
	}

	public override void Init(Block _block)
	{
		ShapeName = _block.Properties.Values["Model"];
		if (ShapeName == null)
		{
			throw new Exception("No model specified on block with name " + _block.GetBlockName());
		}
		Vector3 optionalValue = new Vector3(1f, 0f, 1f);
		_block.Properties.ParseVec("ModelOffset", ref optionalValue);
		if (!meshData.TryGetValue(ShapeName, out var value))
		{
			value = new MeshData();
			meshData.Add(ShapeName, value);
		}
		if (!value.posArrays.TryGetValue(optionalValue, out var value2))
		{
			if (!value.obj)
			{
				GameObject gameObject = DataLoader.LoadAsset<GameObject>(ShapeName ?? "");
				if (!gameObject)
				{
					throw new Exception("Model with name " + ShapeName + " not found");
				}
				value.obj = gameObject;
			}
			value2 = new MeshData.Arrays();
			value.posArrays.Add(optionalValue, value2);
			ParseModel(value, value2, optionalValue);
		}
		visualMeshes = value2.meshes;
		colliderMeshes = value2.colliderMeshes;
		faceInfo = value2.faceInfo;
		boundsRotations = value2.boundsRotations;
		if (value.symTypeOverride != -1)
		{
			SymmetryType = value.symTypeOverride;
		}
		IsSolidCube = value.IsSolidCube;
		if (_block.PathType < 0)
		{
			boundsPathOffsetRotations = new Vector2[32];
			Vector2 vector = default(Vector2);
			for (int i = 0; i < 28; i++)
			{
				Bounds bounds = boundsRotations[i];
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;
				vector.x = 0f;
				float num = min.x + centerOffsetV.x;
				if (num >= -0.01f)
				{
					vector.x = (-0.5f + num) * 0.5f;
				}
				num = max.x + centerOffsetV.x;
				if (num <= 0.01f)
				{
					vector.x = (0.5f + num) * 0.5f;
				}
				vector.y = 0f;
				float num2 = min.z + centerOffsetV.z;
				if (num2 >= -0.01f)
				{
					vector.y = (-0.5f + num2) * 0.5f;
				}
				num2 = max.z + centerOffsetV.z;
				if (num2 <= 0.01f)
				{
					vector.y = (0.5f + num2) * 0.5f;
				}
				if (vector.x != 0f || vector.y != 0f)
				{
					_block.PathType = -1;
					boundsPathOffsetRotations[i] = vector;
				}
			}
		}
		base.Init(_block);
		if (!value.obj)
		{
			return;
		}
		MeshRenderer[] componentsInChildren = value.obj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			Material sharedMaterial = meshRenderer.sharedMaterial;
			if (sharedMaterial != null)
			{
				meshRenderer.sharedMaterial = null;
				Resources.UnloadAsset(sharedMaterial);
			}
		}
		MeshFilter[] componentsInChildren2 = value.obj.GetComponentsInChildren<MeshFilter>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Mesh sharedMesh = componentsInChildren2[j].sharedMesh;
			if (sharedMesh != null)
			{
				value.obj = null;
				Resources.UnloadAsset(sharedMesh);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseModel(MeshData _data, MeshData.Arrays _arrays, Vector3 _modelOffset)
	{
		if (convertRotationCached == null)
		{
			convertRotationCached = new int[32, 7];
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					convertRotationCached[i, j] = convertRotation((BlockFace)j, i);
				}
			}
		}
		Transform transform = _data.obj.transform;
		for (int k = 0; k < transform.childCount; k++)
		{
			Transform child = transform.GetChild(k);
			switch (child.name)
			{
			case "Solid":
				_data.IsSolidCube = true;
				break;
			case "LOD0":
			{
				for (int m = 0; m < child.childCount; m++)
				{
					Transform child3 = child.GetChild(m);
					string name2 = child3.name;
					int num2 = CharToFaceIndex(name2[0]);
					if (num2 == -1)
					{
						continue;
					}
					_arrays.meshes[num2] = CreateMeshFromMeshFilter(child3, _modelOffset);
					if (child3.childCount > 0)
					{
						Log.Error($"{_data.obj.name} face \"{name2}\" has 1 base mesh and {child3.childCount} submeshes, exceeding the total limit of {1} meshes per face.");
					}
					for (int n = 0; n < child3.childCount && n < 0; n++)
					{
						Transform child4 = child3.GetChild(n);
						_arrays.meshes[num2 + (n + 1) * 7] = CreateMeshFromMeshFilter(child4, _modelOffset);
					}
					if (name2.Length <= 2)
					{
						continue;
					}
					for (int num3 = 2; num3 < name2.Length; num3++)
					{
						switch (name2[num3])
						{
						case 'F':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.Full;
							break;
						case 'P':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.Part;
							break;
						case 'A':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.Remove;
							break;
						case 'C':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.Continuous;
							break;
						case 'Y':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.RemoveIfAny;
							break;
						case 'O':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.OwnFaces;
							break;
						case 'H':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.HideIfSame;
							break;
						case 'T':
							_arrays.faceInfo[num2] = EnumFaceOcclusionInfo.Transparent;
							break;
						}
					}
				}
				break;
			}
			case "Collider":
			{
				for (int l = 0; l < child.childCount; l++)
				{
					Transform child2 = child.GetChild(l);
					string name = child2.name;
					int num = CharToFaceIndex(name[0]);
					if (num != -1)
					{
						_arrays.colliderMeshes[num] = CreateMeshFromMeshFilter(child2, _modelOffset);
					}
				}
				break;
			}
			case "SymType_0":
				_data.symTypeOverride = 0;
				break;
			case "SymType_2":
				_data.symTypeOverride = 2;
				break;
			case "SymType_3":
				_data.symTypeOverride = 3;
				break;
			case "SymType_4":
				_data.symTypeOverride = 4;
				break;
			}
		}
		CalcBounds(_arrays);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcBounds(MeshData.Arrays _arrays)
	{
		Bounds bounds = default(Bounds);
		for (int i = 0; i < 28; i++)
		{
			Quaternion quaternion = rotationsToQuats[i];
			Vector3 positiveInfinity = Vector3.positiveInfinity;
			Vector3 negativeInfinity = Vector3.negativeInfinity;
			for (int j = 0; j < 6; j++)
			{
				MySimpleMesh mySimpleMesh = _arrays.meshes[j];
				if (mySimpleMesh == null)
				{
					continue;
				}
				List<Vector3> vertices = mySimpleMesh.Vertices;
				int count = vertices.Count;
				for (int k = 0; k < count; k++)
				{
					Vector3 vector = quaternion * (vertices[k] + centerOffsetV);
					if (vector.x < positiveInfinity.x)
					{
						positiveInfinity.x = vector.x;
					}
					if (vector.x > negativeInfinity.x)
					{
						negativeInfinity.x = vector.x;
					}
					if (vector.y < positiveInfinity.y)
					{
						positiveInfinity.y = vector.y;
					}
					if (vector.y > negativeInfinity.y)
					{
						negativeInfinity.y = vector.y;
					}
					if (vector.z < positiveInfinity.z)
					{
						positiveInfinity.z = vector.z;
					}
					if (vector.z > negativeInfinity.z)
					{
						negativeInfinity.z = vector.z;
					}
				}
			}
			positiveInfinity -= centerOffsetV;
			negativeInfinity -= centerOffsetV;
			bounds.SetMinMax(positiveInfinity, negativeInfinity);
			bounds.extents = new Vector3(Utils.FastMax(bounds.extents.x, 0.1f), Utils.FastMax(bounds.extents.y, 0.1f), Utils.FastMax(bounds.extents.z, 0.1f));
			_arrays.boundsRotations[i] = bounds;
		}
	}

	public static void Cleanup()
	{
		meshData.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CharToFaceIndex(char _c)
	{
		return _c switch
		{
			'N' => 2, 
			'S' => 4, 
			'E' => 5, 
			'W' => 3, 
			'T' => 0, 
			'B' => 1, 
			'M' => 6, 
			_ => -1, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int convertRotation(BlockFace _face, int _rotation)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = _face switch
		{
			BlockFace.Top => Vector3.up, 
			BlockFace.Bottom => Vector3.down, 
			BlockFace.North => Vector3.forward, 
			BlockFace.South => Vector3.back, 
			BlockFace.East => Vector3.right, 
			BlockFace.West => Vector3.left, 
			_ => Vector3.zero, 
		};
		Quaternion quaternion = Quaternion.Inverse(rotationsToQuats[_rotation]);
		zero = quaternion * zero;
		vector = quaternion * vector;
		Vector3 vector2 = vector - zero;
		if (vector2.x > 0.9f)
		{
			return 5;
		}
		if (vector2.x < -0.9f)
		{
			return 3;
		}
		if (vector2.y > 0.9f)
		{
			return 0;
		}
		if (vector2.y < -0.9f)
		{
			return 1;
		}
		if (vector2.z > 0.9f)
		{
			return 2;
		}
		if (vector2.z < -0.9f)
		{
			return 4;
		}
		return (int)_face;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void convertRotationUVDirs(BlockFace _face, int _rotation, out Vector3 _dX, out Vector3 _dY)
	{
		Vector3 vector;
		Vector3 vector2;
		switch (_face)
		{
		case BlockFace.Top:
			vector = Vector3.right;
			vector2 = Vector3.forward;
			break;
		case BlockFace.Bottom:
			vector = Vector3.left;
			vector2 = Vector3.forward;
			break;
		case BlockFace.North:
			vector = Vector3.left;
			vector2 = Vector3.up;
			break;
		case BlockFace.South:
			vector = Vector3.right;
			vector2 = Vector3.up;
			break;
		case BlockFace.East:
			vector = Vector3.forward;
			vector2 = Vector3.up;
			break;
		case BlockFace.West:
			vector = Vector3.back;
			vector2 = Vector3.up;
			break;
		case BlockFace.Middle:
			vector = Vector3.left;
			vector2 = Vector3.up;
			break;
		default:
			vector = Vector3.zero;
			vector2 = Vector3.zero;
			break;
		}
		Quaternion quaternion = rotationsToQuats[_rotation];
		_dX = quaternion * vector;
		_dY = quaternion * vector2;
	}

	public static int ConvertRotationFree(int _rotation, Quaternion _q, bool _bApplyRotFirst = false)
	{
		Vector3 vector = new Vector3(-0.5f, -0.5f, -0.5f);
		Vector3 up = Vector3.up;
		Quaternion rotationStatic = GetRotationStatic(_rotation);
		if (_bApplyRotFirst)
		{
			vector = _q * vector;
			up = _q * up;
			vector = rotationStatic * vector;
			up = rotationStatic * up;
		}
		else
		{
			vector = rotationStatic * vector;
			up = rotationStatic * up;
			vector = _q * vector;
			up = _q * up;
		}
		vector.x += 0.5f;
		vector.y += 0.5f;
		vector.z += 0.5f;
		Vector3i vector3i = new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
		Vector3i vector3i2 = new Vector3i(Mathf.RoundToInt(up.x), Mathf.RoundToInt(up.y), Mathf.RoundToInt(up.z));
		if (vector3i2.x == 0)
		{
			if (vector3i2.z == 0)
			{
				if (vector3i2.y == 1)
				{
					if (vector3i.y == 0)
					{
						int num = BlockFaceToRot(BlockFace.Top);
						if (vector3i.x == 0)
						{
							if (vector3i.z == 0)
							{
								return num;
							}
							return num + 1;
						}
						if (vector3i.z == 1)
						{
							return num + 2;
						}
						return num + 3;
					}
				}
				else if (vector3i.y == 1)
				{
					int num2 = BlockFaceToRot(BlockFace.Bottom);
					if (vector3i.x == 1)
					{
						if (vector3i.z == 0)
						{
							return num2;
						}
						return num2 + 1;
					}
					if (vector3i.z == 1)
					{
						return num2 + 2;
					}
					return num2 + 3;
				}
			}
			if (vector3i2.y == 0)
			{
				if (vector3i2.z == 1)
				{
					if (vector3i.z == 0)
					{
						int num3 = BlockFaceToRot(BlockFace.North);
						if (vector3i.x == 1)
						{
							if (vector3i.y == 0)
							{
								return num3;
							}
							return num3 + 1;
						}
						if (vector3i.y == 1)
						{
							return num3 + 2;
						}
						return num3 + 3;
					}
				}
				else if (vector3i.z == 1)
				{
					int num4 = BlockFaceToRot(BlockFace.South);
					if (vector3i.x == 0)
					{
						if (vector3i.y == 0)
						{
							return num4;
						}
						return num4 + 1;
					}
					if (vector3i.y == 1)
					{
						return num4 + 2;
					}
					return num4 + 3;
				}
			}
		}
		else if (vector3i2.y == 0 && vector3i2.z == 0)
		{
			if (vector3i2.x == -1)
			{
				if (vector3i.x == 1)
				{
					int num5 = BlockFaceToRot(BlockFace.West);
					if (vector3i.y == 0)
					{
						if (vector3i.z == 0)
						{
							return num5;
						}
						return num5 + 1;
					}
					if (vector3i.z == 1)
					{
						return num5 + 2;
					}
					return num5 + 3;
				}
			}
			else if (vector3i.x == 0)
			{
				int num6 = BlockFaceToRot(BlockFace.East);
				if (vector3i.y == 1)
				{
					if (vector3i.z == 0)
					{
						return num6;
					}
					return num6 + 1;
				}
				if (vector3i.z == 1)
				{
					return num6 + 2;
				}
				return num6 + 3;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MySimpleMesh CreateMeshFromMeshFilter(Transform _transform, Vector3 _modelOffset)
	{
		MeshFilter component = _transform.GetComponent<MeshFilter>();
		if (component == null)
		{
			return null;
		}
		Mesh sharedMesh = component.sharedMesh;
		if (!sharedMesh.isReadable)
		{
			Log.Error("Mesh '" + sharedMesh.name + "' not readable in shape with Model=" + ShapeName);
			return null;
		}
		MySimpleMesh mySimpleMesh = new MySimpleMesh();
		Matrix4x4 localToWorldMatrix = _transform.localToWorldMatrix;
		localToWorldMatrix.m03 += _modelOffset.x;
		localToWorldMatrix.m13 += _modelOffset.y;
		localToWorldMatrix.m23 += _modelOffset.z;
		sharedMesh.GetVertices(mySimpleMesh.Vertices);
		int count = mySimpleMesh.Vertices.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 point = mySimpleMesh.Vertices[i];
			point = localToWorldMatrix.MultiplyPoint3x4(point);
			mySimpleMesh.Vertices[i] = point;
		}
		if (sharedMesh.subMeshCount == 1)
		{
			sharedMesh.GetTriangles(mySimpleMesh.Indices, 0);
		}
		else
		{
			int[] triangles = sharedMesh.triangles;
			for (int j = 0; j < triangles.Length; j++)
			{
				mySimpleMesh.Indices.Add((ushort)triangles[j]);
			}
		}
		sharedMesh.GetNormals(mySimpleMesh.Normals);
		int count2 = mySimpleMesh.Normals.Count;
		for (int k = 0; k < count2; k++)
		{
			Vector3 vector = mySimpleMesh.Normals[k];
			vector = localToWorldMatrix.MultiplyVector(vector);
			mySimpleMesh.Normals[k] = vector.normalized;
		}
		sharedMesh.GetUVs(0, mySimpleMesh.Uvs);
		return mySimpleMesh;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		int num = 0;
		for (int i = 0; i < faceInfo.Length; i++)
		{
			int num2 = convertRotationCached[_blockValue.rotation, i];
			if (faceInfo[num2] == EnumFaceOcclusionInfo.Full)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		if (_blockValue.ischild)
		{
			return false;
		}
		int num = convertRotationCached[_blockValue.rotation, (uint)_face];
		if (visualMeshes[num] == null)
		{
			return false;
		}
		Block block = _blockValue.Block;
		switch (faceInfo[num])
		{
		case EnumFaceOcclusionInfo.None:
			return false;
		case EnumFaceOcclusionInfo.Remove:
			if (_adjBlockValue.Block.shape is BlockShapeNew blockShapeNew3)
			{
				int num3 = convertRotationCached[_adjBlockValue.rotation, (uint)BlockFaceFlags.OppositeFace(_face)];
				if (blockShapeNew3.faceInfo[num3] == EnumFaceOcclusionInfo.Remove)
				{
					return false;
				}
			}
			return true;
		case EnumFaceOcclusionInfo.Part:
		{
			BlockShapeNew blockShapeNew4 = null;
			if (!_adjBlockValue.ischild && block.MeshIndex == _adjBlockValue.Block.MeshIndex && _adjBlockValue.Block.shape is BlockShapeNew blockShapeNew5)
			{
				int num4 = convertRotationCached[_adjBlockValue.rotation, (uint)BlockFaceFlags.OppositeFace(_face)];
				if (_adjBlockValue.rotation == _blockValue.rotation && blockShapeNew5.ShapeName == ShapeName && blockShapeNew5.faceInfo[num4] == EnumFaceOcclusionInfo.Part)
				{
					return false;
				}
				if (blockShapeNew5.faceInfo[num4] == EnumFaceOcclusionInfo.Full)
				{
					return false;
				}
			}
			break;
		}
		case EnumFaceOcclusionInfo.Continuous:
		{
			BlockShapeNew blockShapeNew = null;
			if (!_adjBlockValue.ischild && block.MeshIndex == _adjBlockValue.Block.MeshIndex && _adjBlockValue.Block.shape is BlockShapeNew blockShapeNew2)
			{
				int num2 = convertRotationCached[_adjBlockValue.rotation, (uint)BlockFaceFlags.OppositeFace(_face)];
				if (blockShapeNew2.faceInfo[num2] == EnumFaceOcclusionInfo.Full)
				{
					return false;
				}
			}
			break;
		}
		case EnumFaceOcclusionInfo.RemoveIfAny:
			if (!_adjBlockValue.isair)
			{
				return false;
			}
			break;
		case EnumFaceOcclusionInfo.OwnFaces:
			if (_adjBlockValue.type == _blockValue.type)
			{
				return false;
			}
			break;
		case EnumFaceOcclusionInfo.HideIfSame:
			if (_adjBlockValue.type == _blockValue.type && _adjBlockValue.rotation == _blockValue.rotation)
			{
				return false;
			}
			break;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MySimpleMesh getVisualMesh(int _idx)
	{
		return visualMeshes[_idx];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AbsEpsilonEqual(float A, float B, float epsilon = 0.0001f)
	{
		if (Mathf.Abs(A) <= Mathf.Abs(B) + epsilon)
		{
			return Mathf.Abs(A) >= Mathf.Abs(B) - epsilon;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int FloorModInt(int a, int m)
	{
		if (m <= 0)
		{
			return 0;
		}
		int num = a % m;
		if (num < 0)
		{
			num += m;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int FloorModFloat(float x, int m)
	{
		return FloorModInt(Mathf.FloorToInt(x), m);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlanePick ClassifyTriPlane(MySimpleMesh mesh, int triStart, Quaternion q)
	{
		int index = mesh.Indices[triStart];
		int index2 = mesh.Indices[triStart + 1];
		int index3 = mesh.Indices[triStart + 2];
		Vector3 vector = q * mesh.Vertices[index];
		Vector3 vector2 = q * mesh.Vertices[index2];
		Vector3 vector3 = Vector3.Cross(rhs: q * mesh.Vertices[index3] - vector, lhs: vector2 - vector);
		float magnitude = vector3.magnitude;
		if (magnitude < 1E-09f)
		{
			return new PlanePick
			{
				axis = PlaneAxis.Y,
				neg = false
			};
		}
		Vector3 vector4 = vector3 / magnitude;
		float num = Mathf.Abs(vector4.x);
		Mathf.Abs(vector4.y);
		float num2 = Mathf.Abs(vector4.z);
		if (num > 0.25f && num - num2 - 0.001f > 0f)
		{
			return new PlanePick
			{
				axis = PlaneAxis.X,
				neg = (vector4.x < 0f)
			};
		}
		if (num2 > 0.25f)
		{
			return new PlanePick
			{
				axis = PlaneAxis.Z,
				neg = (vector4.z < 0f)
			};
		}
		return new PlanePick
		{
			axis = PlaneAxis.Y,
			neg = (vector4.y < 0f)
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSubsetTriPlanar(VoxelMesh targetMesh, MySimpleMesh src, List<int> triIdx, Quaternion q, Vector3 sum1, Vector3 sum2, int texArrayIndex, int blockW, int blockH, int xTiling, int yTiling, Vector3 drawPos, LightingAround lighting, Rect cracksRect, bool imposterActive)
	{
		if (triIdx == null || triIdx.Count == 0)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>(256);
		List<int> list = new List<int>(256);
		for (int i = 0; i < triIdx.Count; i++)
		{
			int index = triIdx[i];
			int num = src.Indices[index];
			if (!dictionary.ContainsKey(num))
			{
				dictionary[num] = list.Count;
				list.Add(num);
			}
		}
		int count = list.Count;
		targetMesh.CheckVertexLimit(count);
		if (count + targetMesh.m_Vertices.Count > 786432)
		{
			return;
		}
		int num2 = targetMesh.m_Vertices.Alloc(count);
		targetMesh.m_Normals.Alloc(count);
		targetMesh.m_ColorVertices.Alloc(count);
		Color value = default(Color);
		value.g = texArrayIndex;
		value.a = 1f;
		value.b = (float)(int)lighting[LightingAround.Pos.Middle].stability / 15f;
		for (int j = 0; j < count; j++)
		{
			int index2 = list[j];
			int idx = num2 + j;
			Vector3 vector = q * (src.Vertices[index2] + sum1) + sum2;
			Vector3 value2 = q * src.Normals[index2];
			targetMesh.m_Vertices[idx] = vector;
			targetMesh.m_Normals[idx] = value2;
			Vector3 vector2 = vector - drawPos;
			float num3 = (float)(int)lighting[LightingAround.Pos.X0Y0Z0].sun * (1f - vector2.x) * (1f - vector2.y) * (1f - vector2.z) + (float)(int)lighting[LightingAround.Pos.X1Y0Z0].sun * vector2.x * (1f - vector2.y) * (1f - vector2.z) + (float)(int)lighting[LightingAround.Pos.X0Y0Z1].sun * (1f - vector2.x) * (1f - vector2.y) * vector2.z + (float)(int)lighting[LightingAround.Pos.X1Y0Z1].sun * vector2.x * (1f - vector2.y) * vector2.z + (float)(int)lighting[LightingAround.Pos.X0Y1Z0].sun * (1f - vector2.x) * vector2.y * (1f - vector2.z) + (float)(int)lighting[LightingAround.Pos.X0Y1Z1].sun * (1f - vector2.x) * vector2.y * vector2.z + (float)(int)lighting[LightingAround.Pos.X1Y1Z0].sun * vector2.x * vector2.y * (1f - vector2.z) + (float)(int)lighting[LightingAround.Pos.X1Y1Z1].sun * vector2.x * vector2.y * vector2.z;
			value.r = num3 / 15f;
			targetMesh.m_ColorVertices[idx] = value;
		}
		int curTriangleIndex = targetMesh.CurTriangleIndex;
		int count2 = triIdx.Count;
		int num4 = targetMesh.m_Indices.Alloc(count2);
		for (int k = 0; k < count2; k++)
		{
			int index3 = triIdx[k];
			int key = src.Indices[index3];
			int num5 = dictionary[key];
			targetMesh.m_Indices[num4 + k] = curTriangleIndex + num5;
		}
		targetMesh.CurTriangleIndex += count;
		int num6 = targetMesh.m_Uvs.Alloc(count);
		Vector2 value3 = new Vector2((float)blockW / (float)Mathf.Max(1, xTiling), (float)blockH / (float)Mathf.Max(1, yTiling));
		for (int l = 0; l < count; l++)
		{
			targetMesh.m_Uvs[num6 + l] = value3;
		}
		if (targetMesh.UvsCrack == null)
		{
			return;
		}
		int num7 = targetMesh.UvsCrack.Alloc(count);
		Vector2 value4 = default(Vector2);
		for (int m = 0; m < count; m++)
		{
			int index4 = list[m];
			if (!bImposterGenerationActive)
			{
				value4.x = cracksRect.x + src.Uvs[index4].x * cracksRect.width;
				value4.y = cracksRect.y + src.Uvs[index4].y * cracksRect.height;
			}
			else
			{
				value4.x = texArrayIndex;
				value4.y = 1f;
			}
			targetMesh.UvsCrack[num7 + m] = value4;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ComputeTaIndexOffsFor(PlaneAxis plane, UVRectTiling uvTexRect, Vector3 uvPos, bool flipU)
	{
		int num = Mathf.Max(1, (int)uvTexRect.uv.width);
		int num2 = Mathf.Max(1, (int)uvTexRect.uv.height);
		float x;
		float x2;
		switch (plane)
		{
		case PlaneAxis.Y:
			x = uvPos.x;
			x2 = uvPos.z;
			break;
		case PlaneAxis.X:
			x = uvPos.z;
			x2 = uvPos.y;
			break;
		default:
			x = uvPos.x;
			x2 = uvPos.y;
			break;
		}
		int num3 = FloorModFloat(x, num);
		int num4 = FloorModFloat(x2, num2);
		if (flipU)
		{
			num3 = num - 1 - num3;
		}
		num4 = num2 - 1 - num4;
		return num3 + num4 * num;
	}

	public override void renderFace(Vector3i _chunkPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		int num = convertRotationCached[_blockValue.rotation, (uint)_face];
		Block block = _blockValue.Block;
		int num2;
		if (faceInfo[num] == EnumFaceOcclusionInfo.Transparent)
		{
			num2 = 2;
			_face = BlockFace.North;
		}
		else
		{
			num2 = block.MeshIndex;
		}
		VoxelMesh voxelMesh = _meshes[num2];
		if (voxelMesh == null)
		{
			return;
		}
		MeshDescription meshDescription = MeshDescription.meshes[num2];
		bool bTextureArray = meshDescription.bTextureArray;
		Vector3 vector = _drawPos;
		if (_purpose == MeshPurpose.Preview)
		{
			vector.x += _chunkPos.x + 1;
			vector.y += _chunkPos.y + 1;
			vector.z += _chunkPos.z + 1;
		}
		Quaternion quaternion = rotationsToQuats[_blockValue.rotation];
		Vector3 vector2 = centerOffsetV;
		Vector3 vector3 = _drawPos - centerOffsetV + GetRotationOffset(_blockValue);
		Color value = default(Color);
		Vector2 vector5 = default(Vector2);
		Vector3i vector3i = default(Vector3i);
		Vector3i vector3i2 = default(Vector3i);
		Vector3 vector6 = default(Vector3);
		for (int i = 0; i < 1; i++)
		{
			int num3 = i * 7;
			MySimpleMesh visualMesh = getVisualMesh(num + num3);
			if (visualMesh == null)
			{
				continue;
			}
			int count = visualMesh.Vertices.Count;
			voxelMesh.CheckVertexLimit(count);
			if (count + voxelMesh.m_Vertices.Count > 786432)
			{
				continue;
			}
			int num4 = 0;
			if (num2 == 0)
			{
				int num5 = Chunk.Value64FullToIndex(_textureFullArray[i], (BlockFace)num);
				BlockTextureData blockTextureData = BlockTextureData.list[num5];
				if (blockTextureData == null)
				{
					if (!DynamicMeshBlockSwap.InvalidPaintIds.Contains(num5))
					{
						DynamicMeshBlockSwap.InvalidPaintIds.Add(num5);
						Log.Out("Missing paint ID XML entry: " + num5 + " for block '" + block.GetBlockName() + "'");
					}
				}
				else
				{
					num4 = blockTextureData.TextureID;
				}
			}
			int num6 = ((num4 == 0) ? block.GetSideTextureId(_blockValue, (BlockFace)num, i) : num4);
			if ((uint)num6 >= meshDescription.textureAtlas.uvMapping.Length)
			{
				continue;
			}
			UVRectTiling uvTexRect = meshDescription.textureAtlas.uvMapping[num6];
			if (uvTexRect.blockW == 0 || uvTexRect.blockH == 0)
			{
				Log.Error("Block with name '{0}' uses a texture id {1} that is not in the atlas!", block.GetBlockName(), num6);
				continue;
			}
			int num7 = uvTexRect.blockW;
			int num8 = uvTexRect.blockH;
			bool flag = uvTexRect.bGlobalUV;
			switch (block.GetUVMode(num, i))
			{
			case Block.UVMode.Global:
				flag = true;
				break;
			case Block.UVMode.Local:
				flag = false;
				break;
			}
			flag = flag && _purpose != MeshPurpose.Local;
			convertRotationUVDirs((BlockFace)num, _blockValue.rotation, out var _dX, out var _dY);
			float num9 = 0f;
			float num10 = 0f;
			float x = 0f;
			float x2 = 0f;
			if (!flag || !bTextureArray)
			{
				switch (_face)
				{
				case BlockFace.Top:
				case BlockFace.Bottom:
					if (_dX.x < -0.9f || _dX.x > 0.9f)
					{
						num9 = _dX.x;
						x = vector.x;
					}
					else
					{
						num9 = _dX.z;
						x = vector.z;
					}
					if (_dY.z < -0.9f || _dY.z > 0.9f)
					{
						num10 = _dY.z;
						x2 = vector.z;
					}
					else
					{
						num10 = _dY.x;
						x2 = vector.x;
					}
					break;
				case BlockFace.West:
				case BlockFace.East:
					if (_dX.z < -0.9f || _dX.z > 0.9f)
					{
						num9 = _dX.z;
						x = vector.z;
					}
					else
					{
						num9 = _dX.y;
						x = vector.y;
					}
					if (_dY.y < -0.9f || _dY.y > 0.9f)
					{
						num10 = _dY.y;
						x2 = vector.y;
					}
					else
					{
						num10 = _dY.z;
						x2 = vector.z;
					}
					break;
				case BlockFace.North:
				case BlockFace.South:
					if (_dX.x < -0.9f || _dX.x > 0.9f)
					{
						num9 = _dX.x;
						x = vector.x;
					}
					else
					{
						num9 = _dX.y;
						x = vector.y;
					}
					if (_dY.y < -0.9f || _dY.y > 0.9f)
					{
						num10 = _dY.y;
						x2 = vector.y;
					}
					else
					{
						num10 = _dY.x;
						x2 = vector.x;
					}
					break;
				}
			}
			Vector2 vector4 = Vector3.zero;
			int num11 = (int)uvTexRect.uv.width;
			int num12 = (int)uvTexRect.uv.height;
			int num13 = 0;
			if (!bTextureArray)
			{
				vector4.x = ((num9 > 0f) ? (uvTexRect.uv.x + (float)FloorModFloat(x, num7) * uvTexRect.uv.width) : (uvTexRect.uv.x + uvTexRect.uv.width * (float)(num7 - 1) - (float)FloorModFloat(x, num7) * uvTexRect.uv.width));
				vector4.y = ((num10 > 0f) ? (uvTexRect.uv.y + (float)FloorModFloat(x2, num8) * uvTexRect.uv.height) : (uvTexRect.uv.y + uvTexRect.uv.height * (float)(num8 - 1) - (float)FloorModFloat(x2, num8) * uvTexRect.uv.height));
			}
			else if (!flag)
			{
				num7 = Utils.FastMax(num7, num11);
				num8 = Utils.FastMax(num8, num12);
				vector4.x = ((num9 > 0f) ? FloorModFloat(x, num7) : (num7 - 1 - FloorModFloat(x, num7)));
				vector4.y = ((num10 > 0f) ? FloorModFloat(x2, num8) : (num8 - 1 - FloorModFloat(x2, num8)));
				if (num11 > 1)
				{
					num13 += FloorModInt((int)vector4.x, num11);
				}
				if (num12 > 1)
				{
					num13 += FloorModInt((int)vector4.y, num12) * num11;
				}
				vector4.x -= (int)vector4.x;
				vector4.y -= (int)vector4.y;
			}
			int num14 = uvTexRect.index + num13;
			if (bTextureArray)
			{
				value.g = num14;
			}
			else
			{
				value.g = (block.Properties.Contains("Frame") ? 1 : 0);
			}
			value.b = (float)(int)_lightingAround[LightingAround.Pos.Middle].stability / 15f;
			value.a = (flag ? 1 : 0);
			Rect cracksRect;
			if (bTextureArray && flag)
			{
				List<int> list = new List<int>(128);
				List<int> list2 = new List<int>(128);
				List<int> list3 = new List<int>(128);
				List<int> list4 = new List<int>(128);
				List<int> list5 = new List<int>(128);
				List<int> list6 = new List<int>(128);
				for (int j = 0; j < visualMesh.Indices.Count; j += 3)
				{
					PlanePick planePick = ClassifyTriPlane(visualMesh, j, quaternion);
					object obj = planePick.axis switch
					{
						PlaneAxis.X => planePick.neg ? list2 : list, 
						PlaneAxis.Y => planePick.neg ? list4 : list3, 
						_ => planePick.neg ? list6 : list5, 
					};
					((List<int>)obj).Add(j);
					((List<int>)obj).Add(j + 1);
					((List<int>)obj).Add(j + 2);
				}
				int blockW = Mathf.Max(uvTexRect.blockW, num11);
				int blockH = Mathf.Max(uvTexRect.blockH, num12);
				cracksRect = WorldConstants.MapDamageToUVRect(_blockValue);
				Vector3 uvPos = _drawPos + _chunkPos + new Vector3(0.5f, 0.5f, 0.5f);
				if (list.Count > 0)
				{
					int texArrayIndex = uvTexRect.index + ComputeTaIndexOffsFor(PlaneAxis.X, uvTexRect, uvPos, flipU: false);
					AppendSubsetTriPlanar(voxelMesh, visualMesh, list, quaternion, vector2, vector3, texArrayIndex, blockW, blockH, num11, num12, _drawPos, _lightingAround, cracksRect, bImposterGenerationActive);
				}
				if (list2.Count > 0)
				{
					int texArrayIndex2 = uvTexRect.index + ComputeTaIndexOffsFor(PlaneAxis.X, uvTexRect, uvPos, flipU: true);
					AppendSubsetTriPlanar(voxelMesh, visualMesh, list2, quaternion, vector2, vector3, texArrayIndex2, blockW, blockH, num11, num12, _drawPos, _lightingAround, cracksRect, bImposterGenerationActive);
				}
				if (list3.Count > 0)
				{
					int texArrayIndex3 = uvTexRect.index + ComputeTaIndexOffsFor(PlaneAxis.Y, uvTexRect, uvPos, flipU: false);
					AppendSubsetTriPlanar(voxelMesh, visualMesh, list3, quaternion, vector2, vector3, texArrayIndex3, blockW, blockH, num11, num12, _drawPos, _lightingAround, cracksRect, bImposterGenerationActive);
				}
				if (list4.Count > 0)
				{
					int texArrayIndex4 = uvTexRect.index + ComputeTaIndexOffsFor(PlaneAxis.Y, uvTexRect, uvPos, flipU: false);
					AppendSubsetTriPlanar(voxelMesh, visualMesh, list4, quaternion, vector2, vector3, texArrayIndex4, blockW, blockH, num11, num12, _drawPos, _lightingAround, cracksRect, bImposterGenerationActive);
				}
				if (list5.Count > 0)
				{
					int texArrayIndex5 = uvTexRect.index + ComputeTaIndexOffsFor(PlaneAxis.Z, uvTexRect, uvPos, flipU: true);
					AppendSubsetTriPlanar(voxelMesh, visualMesh, list5, quaternion, vector2, vector3, texArrayIndex5, blockW, blockH, num11, num12, _drawPos, _lightingAround, cracksRect, bImposterGenerationActive);
				}
				if (list6.Count > 0)
				{
					int texArrayIndex6 = uvTexRect.index + ComputeTaIndexOffsFor(PlaneAxis.Z, uvTexRect, uvPos, flipU: false);
					AppendSubsetTriPlanar(voxelMesh, visualMesh, list6, quaternion, vector2, vector3, texArrayIndex6, blockW, blockH, num11, num12, _drawPos, _lightingAround, cracksRect, bImposterGenerationActive);
				}
				continue;
			}
			int count2 = voxelMesh.m_Vertices.Count;
			_ = voxelMesh.m_CollVertices?.Count;
			int num15 = voxelMesh.m_Vertices.Alloc(count);
			voxelMesh.m_Normals.Alloc(count);
			voxelMesh.m_ColorVertices.Alloc(count);
			for (int k = 0; k < count; k++)
			{
				int idx = num15 + k;
				Vector3 value2 = quaternion * (visualMesh.Vertices[k] + vector2) + vector3;
				voxelMesh.m_Vertices[idx] = value2;
				Vector3 value3 = quaternion * visualMesh.Normals[k];
				voxelMesh.m_Normals[idx] = value3;
				value2 -= _drawPos;
				float num16 = (float)(int)_lightingAround[LightingAround.Pos.X0Y0Z0].sun * (1f - value2.x) * (1f - value2.y) * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X1Y0Z0].sun * value2.x * (1f - value2.y) * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X0Y0Z1].sun * (1f - value2.x) * (1f - value2.y) * value2.z + (float)(int)_lightingAround[LightingAround.Pos.X1Y0Z1].sun * value2.x * (1f - value2.y) * value2.z + (float)(int)_lightingAround[LightingAround.Pos.X0Y1Z0].sun * (1f - value2.x) * value2.y * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X0Y1Z1].sun * (1f - value2.x) * value2.y * value2.z + (float)(int)_lightingAround[LightingAround.Pos.X1Y1Z0].sun * value2.x * value2.y * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X1Y1Z1].sun * value2.x * value2.y * value2.z;
				value.r = num16 / 15f;
				voxelMesh.m_ColorVertices[idx] = value;
			}
			int num17 = voxelMesh.m_Indices.Alloc(visualMesh.Indices.Count);
			for (int l = 0; l < visualMesh.Indices.Count; l++)
			{
				voxelMesh.m_Indices[num17 + l] = voxelMesh.CurTriangleIndex + visualMesh.Indices[l];
			}
			voxelMesh.CurTriangleIndex += visualMesh.Vertices.Count;
			int count3 = visualMesh.Uvs.Count;
			cracksRect = WorldConstants.MapDamageToUVRect(_blockValue);
			if (bTextureArray)
			{
				num17 = voxelMesh.m_Uvs.Alloc(count3);
				for (int m = 0; m < count3; m++)
				{
					int idx2 = num17 + m;
					if (flag)
					{
						vector5.x = num7 / num11;
						vector5.y = num8 / num12;
					}
					else
					{
						vector5.x = vector4.x + visualMesh.Uvs[m].x;
						vector5.y = vector4.y + visualMesh.Uvs[m].y;
					}
					voxelMesh.m_Uvs[idx2] = vector5;
				}
				if (voxelMesh.UvsCrack == null)
				{
					continue;
				}
				voxelMesh.UvsCrack.Alloc(count3);
				for (int n = 0; n < count3; n++)
				{
					int idx3 = num17 + n;
					if (!bImposterGenerationActive)
					{
						vector5.x = cracksRect.x + visualMesh.Uvs[n].x * cracksRect.width;
						vector5.y = cracksRect.y + visualMesh.Uvs[n].y * cracksRect.height;
					}
					else
					{
						vector5.x = value.g;
						vector5.y = value.a;
					}
					voxelMesh.UvsCrack[idx3] = vector5;
				}
				continue;
			}
			if (!flag)
			{
				for (int num18 = 0; num18 < count3; num18++)
				{
					vector5.x = vector4.x + visualMesh.Uvs[num18].x * uvTexRect.uv.width;
					vector5.y = vector4.y + visualMesh.Uvs[num18].y * uvTexRect.uv.height;
					voxelMesh.m_Uvs.Add(vector5);
					if (voxelMesh.UvsCrack != null)
					{
						vector5.x = cracksRect.x + visualMesh.Uvs[num18].x * cracksRect.width;
						vector5.y = cracksRect.y + visualMesh.Uvs[num18].y * cracksRect.height;
						voxelMesh.UvsCrack.Add(vector5);
					}
				}
				continue;
			}
			vector3i.x = num7;
			vector3i.y = num8;
			vector3i.z = num7;
			vector3i2.x = Mathf.Abs(_chunkPos.x + (int)_drawPos.x);
			vector3i2.y = Mathf.Abs(_chunkPos.y + (int)_drawPos.y);
			vector3i2.z = Mathf.Abs(_chunkPos.z + (int)_drawPos.z);
			if (_chunkPos.x < 0)
			{
				vector3i2.x = Mathf.Abs(_chunkPos.x + 16 - (int)_drawPos.x);
			}
			if (_chunkPos.y < 0)
			{
				vector3i2.y = Mathf.Abs(_chunkPos.y + 16 - (int)_drawPos.y);
			}
			if (_chunkPos.z < 0)
			{
				vector3i2.z = Mathf.Abs(_chunkPos.z + 16 - (int)_drawPos.z);
			}
			vector6.x = vector3i2.x % vector3i.x;
			vector6.y = vector3i2.y % vector3i.y;
			vector6.z = vector3i2.z % vector3i.z;
			bool flag2 = false;
			for (int num19 = 0; num19 < 6; num19++)
			{
				MySimpleMesh visualMesh2 = getVisualMesh(num19);
				if (visualMesh2 != null && visualMesh2.Uvs.Count > 6)
				{
					flag2 = true;
					break;
				}
			}
			for (int num20 = 0; num20 < count3; num20++)
			{
				Vector3 vector7 = voxelMesh.m_Normals[count2];
				bool num21 = Mathf.Abs(vector7.y) > Mathf.Abs(vector7.x) && Mathf.Abs(vector7.y) > Mathf.Abs(vector7.z);
				bool flag3 = Mathf.Abs(vector7.x) > Mathf.Abs(vector7.y) && Mathf.Abs(vector7.x) > Mathf.Abs(vector7.z);
				if (num21)
				{
					vector3i.z = uvTexRect.blockH;
					vector6.z = vector3i2.z % vector3i.z;
				}
				Vector2 zero = Vector2.zero;
				vector7 = voxelMesh.m_Normals[count2 + num20];
				if (!flag2)
				{
					BlockFace blockFace = BlockFace.Top;
					if (vector7.z > 0.95f)
					{
						blockFace = BlockFace.North;
					}
					else if (vector7.z < -0.95f)
					{
						blockFace = BlockFace.South;
					}
					else if (vector7.x > 0.95f)
					{
						blockFace = BlockFace.East;
					}
					else if (vector7.x < -0.95f)
					{
						blockFace = BlockFace.West;
					}
					else if (vector7.y < 0f)
					{
						blockFace = BlockFace.Bottom;
					}
					bool flag4 = Mathf.Abs(vector7.z) > 0.0001f;
					bool flag5 = vector7.y < 0f;
					bool flag6 = Mathf.Abs(vector7.y) > 0.99f;
					bool flag7 = Mathf.Abs(vector7.x) > 0.0001f;
					bool flag8 = vector7.x > 0.0001f;
					bool flag9 = vector7.z > 0.0001f;
					bool flag10 = Mathf.Abs(vector7.x) > 0.1f && Mathf.Abs(vector7.y) > 0.1f && Mathf.Abs(vector7.z) > 0.1f;
					if ((blockFace == BlockFace.Top || blockFace == BlockFace.Bottom) && flag7 && !flag4)
					{
						vector3i.x = uvTexRect.blockH;
						vector3i.z = uvTexRect.blockW;
					}
					Vector3 vector8 = voxelMesh.m_Vertices[count2 + num20] - _drawPos;
					vector8.x = Mathf.Abs(vector8.x);
					vector8.y = Mathf.Abs(vector8.y);
					vector8.z = Mathf.Abs(vector8.z);
					vector6.x = Mathf.Abs(vector6.x);
					vector6.y = Mathf.Abs(vector6.y);
					vector6.z = Mathf.Abs(vector6.z);
					Vector3 vector9 = vector6 + vector8;
					vector9.x = (((int)Mathf.Abs(vector9.x) != vector3i.x) ? (Mathf.Abs(vector9.x) % (float)vector3i.x) : Mathf.Abs(vector9.x));
					vector9.y = (((int)Mathf.Abs(vector9.y) != vector3i.y) ? (Mathf.Abs(vector9.y) % (float)vector3i.y) : Mathf.Abs(vector9.y));
					vector9.z = (((int)Mathf.Abs(vector9.z) != vector3i.z) ? (Mathf.Abs(vector9.z) % (float)vector3i.z) : Mathf.Abs(vector9.z));
					if ((int)vector9.x > vector3i.x)
					{
						vector9.x %= vector3i.x;
					}
					if ((int)vector9.y > vector3i.y)
					{
						vector9.y %= vector3i.y;
					}
					if ((int)vector9.z > vector3i.z)
					{
						vector9.z %= vector3i.z;
					}
					Vector3 vector10 = vector3i.ToVector3() - vector9;
					float num22 = (((int)Mathf.Abs(vector9.x) != vector3i.x) ? (Mathf.Abs(vector9.x) % (float)vector3i.x) : Mathf.Abs(vector9.x));
					float y = (((int)Mathf.Abs(vector9.z) != vector3i.z) ? (Mathf.Abs(vector9.z) % (float)vector3i.z) : Mathf.Abs(vector9.z));
					switch (blockFace)
					{
					case BlockFace.Top:
					case BlockFace.Bottom:
						if (!flag6)
						{
							if (flag7)
							{
								if (flag8)
								{
									if (flag4)
									{
										if (flag9)
										{
											if (flag10)
											{
												zero.x = (vector9.x + vector10.z) * 0.5f;
											}
											else
											{
												zero.x = (float)vector3i.x - num22;
											}
										}
										else if (flag10)
										{
											zero.x = (vector10.x + vector10.z) * 0.5f;
										}
										else
										{
											zero.x = num22;
										}
										zero.y = vector9.y;
									}
									else
									{
										zero.x = vector9.z;
										if (flag5)
										{
											zero.y = vector9.x;
										}
										else
										{
											zero.y = vector10.x;
										}
									}
								}
								else if (flag4)
								{
									if (flag9)
									{
										if (flag10)
										{
											zero.x = (vector10.x + vector10.z) * 0.5f;
										}
										else
										{
											zero.x = (float)vector3i.x - num22;
										}
									}
									else if (flag10)
									{
										zero.x = (vector9.x + vector10.z) * 0.5f;
									}
									else
									{
										zero.x = num22;
									}
									zero.y = vector9.y;
								}
								else
								{
									zero.x = vector10.z;
									if (flag5)
									{
										zero.y = vector10.x;
									}
									else
									{
										zero.y = vector9.x;
									}
								}
							}
							else if (flag9)
							{
								zero.x = vector10.x;
								if (flag5)
								{
									zero.y = vector9.z;
								}
								else
								{
									zero.y = vector10.z;
								}
							}
							else
							{
								zero.x = vector9.x;
								if (flag5)
								{
									zero.y = vector10.z;
								}
								else
								{
									zero.y = vector9.z;
								}
							}
						}
						else
						{
							zero.x = num22;
							zero.y = y;
						}
						break;
					case BlockFace.West:
						zero.x = vector10.z;
						zero.y = vector9.y;
						break;
					case BlockFace.East:
						zero.x = vector9.z;
						zero.y = vector9.y;
						break;
					case BlockFace.North:
						zero.x = vector10.x;
						zero.y = vector9.y;
						break;
					default:
						zero.x = vector9.x;
						zero.y = vector9.y;
						break;
					}
					voxelMesh.m_Uvs.Add(new Vector2(uvTexRect.uv.x + zero.x * uvTexRect.uv.width, uvTexRect.uv.y + zero.y * uvTexRect.uv.height));
				}
				else
				{
					zero.x = Mathf.Floor(1f / uvTexRect.uv.width * 1000f);
					zero.y = (float)uvTexRect.blockW + Mathf.Floor(uvTexRect.blockH * 1000);
					zero.x += uvTexRect.uv.x;
					zero.y += uvTexRect.uv.y;
					voxelMesh.m_Uvs.Add(zero);
				}
				if (voxelMesh.UvsCrack != null)
				{
					voxelMesh.UvsCrack.Add(new Vector2(cracksRect.x + visualMesh.Uvs[num20].x * cracksRect.width, cracksRect.y + visualMesh.Uvs[num20].y * cracksRect.height));
				}
			}
		}
		if (voxelMesh.m_CollVertices == null)
		{
			return;
		}
		if (colliderMeshes[num] != null)
		{
			CopyColliderMesh(voxelMesh, quaternion, vector2, vector3, colliderMeshes[num]);
			return;
		}
		for (int num23 = 0; num23 < 1; num23++)
		{
			int num24 = num23 * 7;
			MySimpleMesh mySimpleMesh = visualMeshes[num24 + num];
			if (mySimpleMesh != null)
			{
				CopyColliderMesh(voxelMesh, quaternion, vector2, vector3, mySimpleMesh);
			}
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void CopyColliderMesh(VoxelMesh targetMesh, Quaternion q, Vector3 sum1, Vector3 sum2, MySimpleMesh colliderMesh)
		{
			int count4 = targetMesh.m_CollVertices.Count;
			int num25 = targetMesh.m_CollVertices.Alloc(colliderMesh.Vertices.Count);
			for (int num26 = 0; num26 < colliderMesh.Vertices.Count; num26++)
			{
				Vector3 value4 = q * (colliderMesh.Vertices[num26] + sum1) + sum2;
				targetMesh.m_CollVertices[num25 + num26] = value4;
			}
			num25 = targetMesh.m_CollIndices.Alloc(colliderMesh.Indices.Count);
			for (int num27 = 0; num27 < colliderMesh.Indices.Count; num27++)
			{
				targetMesh.m_CollIndices[num25 + num27] = count4 + colliderMesh.Indices[num27];
			}
		}
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override void renderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		for (int i = 0; i < faceInfo.Length; i++)
		{
			int num = convertRotationCached[_blockValue.rotation, i];
			if (visualMeshes[num] == null)
			{
				continue;
			}
			switch (faceInfo[num])
			{
			case EnumFaceOcclusionInfo.None:
			case EnumFaceOcclusionInfo.Transparent:
				renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFullArray, _meshes);
				break;
			case EnumFaceOcclusionInfo.OwnFaces:
			{
				BlockFace blockFace = (BlockFace)i;
				Vector3i vector3i = BlockFaceFlags.OffsetIForFace(blockFace);
				BlockValue blockValue = _nBlocks.Get(vector3i.x, vector3i.y + (int)_drawPos.y, vector3i.z);
				Block block = blockValue.Block;
				bool flag = !blockValue.ischild && block.shape.IsSolidCube && !block.shape.IsTerrain();
				if (!flag)
				{
					int num2 = 0;
					switch (blockFace)
					{
					case BlockFace.North:
						num2 = 16;
						break;
					case BlockFace.South:
						num2 = 4;
						break;
					case BlockFace.East:
						num2 = 8;
						break;
					case BlockFace.West:
						num2 = 32;
						break;
					case BlockFace.Top:
						num2 = 2;
						break;
					case BlockFace.Bottom:
						num2 = 1;
						break;
					}
					int facesDrawnFullBitfield = block.shape.getFacesDrawnFullBitfield(blockValue);
					flag = flag || (facesDrawnFullBitfield & num2) != 0;
				}
				if (flag && isRenderFace(_blockValue, blockFace, blockValue))
				{
					renderFace(_worldPos, _blockValue, _drawPos, blockFace, _vertices, _lightingAround, _textureFullArray, _meshes);
				}
				break;
			}
			}
		}
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		if (!_blockValue.ischild)
		{
			int num = 7;
			for (int i = 0; i < num; i++)
			{
				renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFullArray, _meshes, _purpose);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int BlockFaceToRot(BlockFace _blockFace)
	{
		return (int)((uint)_blockFace << 2);
	}

	public static BlockFace RotToBlockFace(int _rotation)
	{
		int num = (_rotation >> 2) & 7;
		if (num <= 5)
		{
			return (BlockFace)num;
		}
		return BlockFace.Top;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int RotToLocalRot(int _rotation)
	{
		return _rotation & 3;
	}

	public static Quaternion GetRotationStatic(int _rotation)
	{
		return rotationsToQuats[_rotation];
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return rotationsToQuats[_blockValue.rotation];
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += ((!_bLeft) ? 1 : (-1));
		if (_rotation > 23)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 23;
		}
		return (byte)_rotation;
	}

	public override Quaternion GetPreviewRotation()
	{
		return Quaternion.AngleAxis(180f, Vector3.up);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static BlockShapeNew()
	{
		centerOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);
		meshData = new Dictionary<string, MeshData>();
		rotationsToQuats = new Quaternion[32]
		{
			Quaternion.AngleAxis(0f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.up),
			Quaternion.AngleAxis(180f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.up),
			Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
			Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
			Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(360f, Vector3.up),
			Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(450f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(360f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(450f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(0f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(180f, Vector3.up),
			Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(270f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(0f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(90f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(0f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(180f, Vector3.up),
			Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(270f, Vector3.up),
			Quaternion.AngleAxis(45f, Vector3.up),
			Quaternion.AngleAxis(135f, Vector3.up),
			Quaternion.AngleAxis(225f, Vector3.up),
			Quaternion.AngleAxis(315f, Vector3.up),
			Quaternion.identity,
			Quaternion.identity,
			Quaternion.identity,
			Quaternion.identity
		};
		rotations = new byte[3, 28];
		for (int i = 1; i < 4; i++)
		{
			for (byte b = 0; b < 28; b++)
			{
				rotations[i - 1, b] = CalcRotation(b, i);
			}
		}
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		if (_rotCount == 0)
		{
			return _blockValue;
		}
		if (_bLeft)
		{
			_rotCount = 4 - _rotCount;
		}
		_blockValue.rotation = rotations[_rotCount - 1, _blockValue.rotation];
		return _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte CalcRotation(byte _rotation, int _rotCount)
	{
		if (_rotation >= 24)
		{
			for (int i = 0; i < _rotCount; i++)
			{
				_rotation++;
				if (_rotation > 27)
				{
					_rotation = 24;
				}
				else if (_rotation < 24)
				{
					_rotation = 27;
				}
			}
		}
		else
		{
			int num = 90 * _rotCount;
			_rotation = (byte)ConvertRotationFree(_rotation, Quaternion.AngleAxis(num, Vector3.up));
		}
		return _rotation;
	}

	public static int MirrorStatic(EnumMirrorAlong _axis, int _rotation, int _symType = 1)
	{
		switch (_symType)
		{
		default:
		{
			Quaternion rotationStatic = GetRotationStatic(_rotation);
			switch (_axis)
			{
			case EnumMirrorAlong.ZAxis:
				rotationStatic.z = 0f - rotationStatic.z;
				rotationStatic.w = 0f - rotationStatic.w;
				rotationStatic *= Quaternion.AngleAxis(180f, Vector3.up);
				break;
			case EnumMirrorAlong.YAxis:
				rotationStatic.y = 0f - rotationStatic.y;
				rotationStatic.w = 0f - rotationStatic.w;
				rotationStatic *= Quaternion.AngleAxis(180f, Vector3.forward);
				break;
			case EnumMirrorAlong.XAxis:
				rotationStatic.x = 0f - rotationStatic.x;
				rotationStatic.w = 0f - rotationStatic.w;
				break;
			}
			return ConvertRotationFree(0, rotationStatic);
		}
		case 0:
			return _rotation;
		case 2:
		{
			BlockFace blockFace2 = RotToBlockFace(_rotation);
			int num3 = 0;
			switch (_axis)
			{
			case EnumMirrorAlong.ZAxis:
				switch (blockFace2)
				{
				case BlockFace.North:
					blockFace2 = BlockFace.South;
					break;
				case BlockFace.South:
					blockFace2 = BlockFace.North;
					break;
				default:
					num3 = 1;
					break;
				}
				break;
			case EnumMirrorAlong.YAxis:
				switch (blockFace2)
				{
				case BlockFace.Bottom:
					blockFace2 = BlockFace.Top;
					break;
				case BlockFace.Top:
					blockFace2 = BlockFace.Bottom;
					break;
				case BlockFace.North:
				case BlockFace.South:
					num3 = 1;
					break;
				}
				break;
			case EnumMirrorAlong.XAxis:
				switch (blockFace2)
				{
				case BlockFace.East:
					blockFace2 = BlockFace.West;
					break;
				case BlockFace.West:
					blockFace2 = BlockFace.East;
					break;
				}
				break;
			}
			int num4 = RotToLocalRot(_rotation);
			switch (num3)
			{
			case 0:
				switch (num4)
				{
				case 0:
					num4 = 3;
					break;
				case 3:
					num4 = 0;
					break;
				case 1:
					num4 = 2;
					break;
				case 2:
					num4 = 1;
					break;
				}
				break;
			case 1:
				switch (num4)
				{
				case 0:
					num4 = 1;
					break;
				case 1:
					num4 = 0;
					break;
				case 2:
					num4 = 3;
					break;
				case 3:
					num4 = 2;
					break;
				}
				break;
			}
			return BlockFaceToRot(blockFace2) | num4;
		}
		case 3:
		{
			BlockFace blockFace = RotToBlockFace(_rotation);
			int num = 1;
			switch (_axis)
			{
			case EnumMirrorAlong.ZAxis:
				switch (blockFace)
				{
				case BlockFace.North:
					blockFace = BlockFace.South;
					break;
				case BlockFace.South:
					blockFace = BlockFace.North;
					break;
				default:
					num = 0;
					break;
				}
				break;
			case EnumMirrorAlong.YAxis:
				switch (blockFace)
				{
				case BlockFace.Bottom:
					blockFace = BlockFace.Top;
					break;
				case BlockFace.Top:
					blockFace = BlockFace.Bottom;
					break;
				case BlockFace.North:
				case BlockFace.South:
					num = 0;
					break;
				}
				break;
			case EnumMirrorAlong.XAxis:
				switch (blockFace)
				{
				case BlockFace.East:
					blockFace = BlockFace.West;
					break;
				case BlockFace.West:
					blockFace = BlockFace.East;
					break;
				}
				break;
			}
			int num2 = RotToLocalRot(_rotation);
			switch (num)
			{
			case 0:
				switch (num2)
				{
				case 0:
					num2 = 3;
					break;
				case 3:
					num2 = 0;
					break;
				case 1:
					num2 = 2;
					break;
				case 2:
					num2 = 1;
					break;
				}
				break;
			case 1:
				switch (num2)
				{
				case 0:
					num2 = 1;
					break;
				case 1:
					num2 = 0;
					break;
				case 2:
					num2 = 3;
					break;
				case 3:
					num2 = 2;
					break;
				}
				break;
			}
			return BlockFaceToRot(blockFace) | num2;
		}
		case 4:
			GetRotationStatic(_rotation);
			switch (_axis)
			{
			case EnumMirrorAlong.ZAxis:
				_rotation = ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), _bApplyRotFirst: true);
				return ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.forward));
			case EnumMirrorAlong.YAxis:
				_rotation = ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), _bApplyRotFirst: true);
				return ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up));
			case EnumMirrorAlong.XAxis:
				_rotation = ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), _bApplyRotFirst: true);
				return ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.right));
			default:
				return _rotation;
			}
		}
	}

	public BlockFace GetBlockFaceFromColliderTriangle(BlockValue _blockValue, Vector3 _v1, Vector3 _v2, Vector3 _v3)
	{
		for (int i = 0; i < colliderMeshes.Length; i++)
		{
			if (CheckMeshForTri(colliderMeshes[i], _v1, _v2, _v3))
			{
				return (BlockFace)i;
			}
		}
		for (int j = 0; j < visualMeshes.Length; j++)
		{
			if (CheckMeshForTri(visualMeshes[j], _v1, _v2, _v3))
			{
				return (BlockFace)(j % 7);
			}
		}
		return BlockFace.None;
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool CheckMeshForTri(MySimpleMesh mesh, Vector3 vector, Vector3 vector2, Vector3 vector3)
		{
			if (mesh == null)
			{
				return false;
			}
			for (int k = 0; k < mesh.Indices.Count; k += 3)
			{
				if ((vector - mesh.Vertices[mesh.Indices[k]]).sqrMagnitude < 0.001f && (vector2 - mesh.Vertices[mesh.Indices[k + 1]]).sqrMagnitude < 0.001f && (vector3 - mesh.Vertices[mesh.Indices[k + 2]]).sqrMagnitude < 0.001f)
				{
					return true;
				}
			}
			return false;
		}
	}

	public int GetVisualMeshChannelFromHitInfo(Vector3i blockPos, BlockValue bv, BlockFace blockFace, WorldRayHitInfo hitInfo)
	{
		if (visualMeshes.Length <= 7)
		{
			return 0;
		}
		int num = 0;
		int result = -1;
		for (int i = 0; i < 1; i++)
		{
			int num2 = i * 7;
			if (getVisualMesh((int)blockFace + num2) != null)
			{
				num++;
				result = i;
			}
		}
		if (num == 1)
		{
			return result;
		}
		Vector3 vector = Vector3.one * 0.5f;
		Quaternion quaternion = Quaternion.Inverse(GetRotation(bv));
		Ray ray = hitInfo.ray;
		ray.origin = quaternion * (ray.origin - blockPos - vector) + vector;
		ray.direction = quaternion * ray.direction;
		float num3 = float.MaxValue;
		int result2 = -1;
		for (int j = 0; j < 1; j++)
		{
			int num4 = j * 7;
			MySimpleMesh visualMesh = getVisualMesh((int)blockFace + num4);
			if (visualMesh == null)
			{
				continue;
			}
			for (int k = 0; k < visualMesh.Indices.Count; k += 3)
			{
				if (GeometryUtils.IntersectRayTriangle(tri: new GeometryUtils.Triangle(visualMesh.Vertices[visualMesh.Indices[k]], visualMesh.Vertices[visualMesh.Indices[k + 1]], visualMesh.Vertices[visualMesh.Indices[k + 2]]), ray: ray, outNormal: out var _, hitDistance: out var hitDistance) && hitDistance < num3)
				{
					num3 = hitDistance;
					result2 = j;
				}
			}
		}
		return result2;
	}

	public override Vector2 GetPathOffset(int _rotation)
	{
		return boundsPathOffsetRotations[_rotation];
	}

	public override float GetStepHeight(BlockValue blockDef, BlockFace crossingFace)
	{
		return blockDef.Block.IsCollideMovement ? 1 : 0;
	}

	public override Bounds[] GetBounds(BlockValue _blockValue)
	{
		boundsArr[0] = boundsRotations[_blockValue.rotation];
		return boundsArr;
	}

	public override BlockFace GetRotatedBlockFace(BlockValue _blockValue, BlockFace _face)
	{
		return (BlockFace)convertRotationCached[_blockValue.rotation, (uint)_face];
	}

	public override void MirrorFace(EnumMirrorAlong _axis, int _sourceRot, int _targetRot, BlockFace _face, out BlockFace _sourceFace, out BlockFace _targetFace)
	{
		_sourceFace = (BlockFace)convertRotationCached[_sourceRot, (uint)_face];
		switch (_axis)
		{
		case EnumMirrorAlong.ZAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Top;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.North:
				_face = BlockFace.South;
				break;
			case BlockFace.West:
				_face = BlockFace.West;
				break;
			case BlockFace.South:
				_face = BlockFace.North;
				break;
			case BlockFace.East:
				_face = BlockFace.East;
				break;
			}
			break;
		case EnumMirrorAlong.YAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Top;
				break;
			case BlockFace.North:
				_face = BlockFace.North;
				break;
			case BlockFace.West:
				_face = BlockFace.West;
				break;
			case BlockFace.South:
				_face = BlockFace.South;
				break;
			case BlockFace.East:
				_face = BlockFace.East;
				break;
			}
			break;
		case EnumMirrorAlong.XAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Top;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.North:
				_face = BlockFace.North;
				break;
			case BlockFace.West:
				_face = BlockFace.East;
				break;
			case BlockFace.South:
				_face = BlockFace.South;
				break;
			case BlockFace.East:
				_face = BlockFace.West;
				break;
			}
			break;
		}
		_targetFace = (BlockFace)convertRotationCached[_targetRot, (uint)_face];
	}

	public override int GetVertexCount()
	{
		int num = 0;
		for (int i = 0; i < visualMeshes.Length; i++)
		{
			num += ((visualMeshes[i] != null) ? visualMeshes[i].Vertices.Count : 0);
		}
		return num;
	}

	public override int GetTriangleCount()
	{
		int num = 0;
		for (int i = 0; i < visualMeshes.Length; i++)
		{
			num += ((visualMeshes[i] != null) ? (visualMeshes[i].Indices.Count / 3) : 0);
		}
		return num;
	}

	public override string GetName()
	{
		return ShapeName;
	}

	public EnumFaceOcclusionInfo GetFaceInfo(BlockValue _blockValue, BlockFace _face)
	{
		_face = GetRotatedBlockFace(_blockValue, _face);
		return faceInfo[(uint)_face];
	}
}

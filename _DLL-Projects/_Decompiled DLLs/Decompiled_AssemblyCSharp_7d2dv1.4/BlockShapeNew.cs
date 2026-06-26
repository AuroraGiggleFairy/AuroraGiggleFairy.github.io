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
				GameObject gameObject = DataLoader.LoadAsset<GameObject>(DataLoader.IsInResources(ShapeName) ? ("Shapes/" + ShapeName) : ShapeName);
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
					if (name2.Length <= 2)
					{
						continue;
					}
					for (int n = 2; n < name2.Length; n++)
					{
						switch (name2[n])
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
	public int roundToIntAndMod(float x, int mod)
	{
		if (mod == 0)
		{
			return 0;
		}
		if (!(x >= 0f))
		{
			return (mod + (int)(x - 0.9999999f) % mod) % mod;
		}
		return (int)x % mod;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MySimpleMesh getVisualMesh(int _idx)
	{
		return visualMeshes[_idx];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MySimpleMesh getColliderMesh(int _idx, MySimpleMesh _visualMesh)
	{
		if (colliderMeshes[_idx] != null)
		{
			return colliderMeshes[_idx];
		}
		return _visualMesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float DOT(Vector3 A, Vector3 B)
	{
		return A.x * B.x + A.y * B.y + A.z * B.z;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EpsilonEqual(float A, float B, float epsilon = 0.0001f)
	{
		if (A <= B + epsilon)
		{
			return A >= B - epsilon;
		}
		return false;
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
	public bool AbsEpsilonLessEqual(float A, float B, float epsilon = 0.0001f)
	{
		if (!(Mathf.Abs(A) <= Mathf.Abs(B)))
		{
			return AbsEpsilonEqual(A, B, epsilon);
		}
		return true;
	}

	public override void renderFace(Vector3i _chunkPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		int num = convertRotationCached[_blockValue.rotation, (uint)_face];
		MySimpleMesh visualMesh = getVisualMesh(num);
		if (visualMesh == null)
		{
			return;
		}
		Block block = _blockValue.Block;
		int num2 = 0;
		int num3;
		if (faceInfo[num] == EnumFaceOcclusionInfo.Transparent)
		{
			num3 = 2;
			_face = BlockFace.North;
		}
		else
		{
			num3 = block.MeshIndex;
			if (num3 == 0)
			{
				int num4 = Chunk.Value64FullToIndex(_textureFull, (BlockFace)num);
				BlockTextureData blockTextureData = BlockTextureData.list[num4];
				if (blockTextureData == null)
				{
					if (!DynamicMeshBlockSwap.InvalidPaintIds.Contains(num4))
					{
						DynamicMeshBlockSwap.InvalidPaintIds.Add(num4);
						Log.Out("Missing paint ID XML entry: " + num4 + " for block '" + block.GetBlockName() + "'");
					}
				}
				else
				{
					num2 = blockTextureData.TextureID;
				}
			}
		}
		VoxelMesh voxelMesh = _meshes[num3];
		if (voxelMesh == null)
		{
			return;
		}
		MeshDescription meshDescription = MeshDescription.meshes[num3];
		int num5 = ((num2 == 0) ? block.GetSideTextureId(_blockValue, (BlockFace)num) : num2);
		if ((uint)num5 >= meshDescription.textureAtlas.uvMapping.Length)
		{
			return;
		}
		UVRectTiling uVRectTiling = meshDescription.textureAtlas.uvMapping[num5];
		if (uVRectTiling.blockW == 0 || uVRectTiling.blockH == 0)
		{
			Log.Error("Block with name '{0}' uses a texture id {1} that is not in the atlas!", block.GetBlockName(), num5);
			return;
		}
		int num6 = uVRectTiling.blockW;
		int num7 = uVRectTiling.blockH;
		bool flag = uVRectTiling.bGlobalUV;
		switch (block.GetUVMode(num))
		{
		case Block.UVMode.Global:
			flag = true;
			break;
		case Block.UVMode.Local:
			flag = false;
			break;
		}
		flag = flag && _purpose != MeshPurpose.Local;
		bool bTextureArray = meshDescription.bTextureArray;
		Vector3 vector = _drawPos;
		if (_purpose == MeshPurpose.Preview)
		{
			vector.x += _chunkPos.x + 1;
			vector.y += _chunkPos.y + 1;
			vector.z += _chunkPos.z + 1;
		}
		convertRotationUVDirs((BlockFace)num, _blockValue.rotation, out var _dX, out var _dY);
		float num8 = 0f;
		float num9 = 0f;
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
					num8 = _dX.x;
					x = vector.x;
				}
				else
				{
					num8 = _dX.z;
					x = vector.z;
				}
				if (_dY.z < -0.9f || _dY.z > 0.9f)
				{
					num9 = _dY.z;
					x2 = vector.z;
				}
				else
				{
					num9 = _dY.x;
					x2 = vector.x;
				}
				break;
			case BlockFace.West:
			case BlockFace.East:
				if (_dX.z < -0.9f || _dX.z > 0.9f)
				{
					num8 = _dX.z;
					x = vector.z;
				}
				else
				{
					num8 = _dX.y;
					x = vector.y;
				}
				if (_dY.y < -0.9f || _dY.y > 0.9f)
				{
					num9 = _dY.y;
					x2 = vector.y;
				}
				else
				{
					num9 = _dY.z;
					x2 = vector.z;
				}
				break;
			case BlockFace.North:
			case BlockFace.South:
				if (_dX.x < -0.9f || _dX.x > 0.9f)
				{
					num8 = _dX.x;
					x = vector.x;
				}
				else
				{
					num8 = _dX.y;
					x = vector.y;
				}
				if (_dY.y < -0.9f || _dY.y > 0.9f)
				{
					num9 = _dY.y;
					x2 = vector.y;
				}
				else
				{
					num9 = _dY.x;
					x2 = vector.x;
				}
				break;
			}
		}
		int num10 = (int)uVRectTiling.uv.width;
		int num11 = (int)uVRectTiling.uv.height;
		int num12 = 0;
		Vector2 vector2 = default(Vector2);
		if (!bTextureArray)
		{
			vector2.x = ((num8 > 0f) ? (uVRectTiling.uv.x + (float)roundToIntAndMod(x, num6) * uVRectTiling.uv.width) : (uVRectTiling.uv.x + uVRectTiling.uv.width * (float)(num6 - 1) - (float)roundToIntAndMod(x, num6) * uVRectTiling.uv.width));
			vector2.y = ((num9 > 0f) ? (uVRectTiling.uv.y + (float)roundToIntAndMod(x2, num7) * uVRectTiling.uv.height) : (uVRectTiling.uv.y + uVRectTiling.uv.height * (float)(num7 - 1) - (float)roundToIntAndMod(x2, num7) * uVRectTiling.uv.height));
		}
		else
		{
			num6 = Utils.FastMax(num6, num10);
			num7 = Utils.FastMax(num7, num11);
			if (flag)
			{
				switch (_face)
				{
				case BlockFace.Top:
				case BlockFace.Bottom:
					x = vector.x;
					x2 = vector.z;
					break;
				case BlockFace.West:
				case BlockFace.East:
					x = vector.z;
					x2 = vector.y;
					break;
				case BlockFace.North:
				case BlockFace.South:
					x = vector.x;
					x2 = vector.y;
					break;
				}
				vector2.x = roundToIntAndMod(x, num6);
				vector2.y = roundToIntAndMod(x2, num7);
			}
			else
			{
				vector2.x = ((num8 > 0f) ? roundToIntAndMod(x, num6) : (num6 - 1 - roundToIntAndMod(x, num6)));
				vector2.y = ((num9 > 0f) ? roundToIntAndMod(x2, num7) : (num7 - 1 - roundToIntAndMod(x2, num7)));
			}
			if (num10 > 1)
			{
				num12 += (int)(vector2.x % (float)num10);
			}
			if (num11 > 1)
			{
				num12 += (int)(vector2.y % (float)num11) * num10;
			}
			vector2.x -= (int)vector2.x;
			vector2.y -= (int)vector2.y;
		}
		int num13 = uVRectTiling.index + num12;
		Quaternion quaternion = rotationsToQuats[_blockValue.rotation];
		Vector3 vector3 = centerOffsetV;
		Vector3 vector4 = _drawPos - centerOffsetV + GetRotationOffset(_blockValue);
		Color value = default(Color);
		if (bTextureArray)
		{
			value.g = num13;
		}
		else
		{
			value.g = (block.Properties.Contains("Frame") ? 1 : 0);
		}
		value.b = (float)(int)_lightingAround[LightingAround.Pos.Middle].stability / 15f;
		value.a = (flag ? 1 : 0);
		int count = voxelMesh.m_Vertices.Count;
		int num14 = voxelMesh.m_CollVertices?.Count ?? 0;
		int count2 = visualMesh.Vertices.Count;
		voxelMesh.CheckVertexLimit(count2);
		if (count2 + voxelMesh.m_Vertices.Count > 786432)
		{
			return;
		}
		int num15 = voxelMesh.m_Vertices.Alloc(count2);
		voxelMesh.m_Normals.Alloc(count2);
		voxelMesh.m_ColorVertices.Alloc(count2);
		for (int i = 0; i < count2; i++)
		{
			int idx = num15 + i;
			Vector3 value2 = quaternion * (visualMesh.Vertices[i] + vector3) + vector4;
			voxelMesh.m_Vertices[idx] = value2;
			Vector3 value3 = quaternion * visualMesh.Normals[i];
			voxelMesh.m_Normals[idx] = value3;
			value2 -= _drawPos;
			float num16 = (float)(int)_lightingAround[LightingAround.Pos.X0Y0Z0].sun * (1f - value2.x) * (1f - value2.y) * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X1Y0Z0].sun * value2.x * (1f - value2.y) * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X0Y0Z1].sun * (1f - value2.x) * (1f - value2.y) * value2.z + (float)(int)_lightingAround[LightingAround.Pos.X1Y0Z1].sun * value2.x * (1f - value2.y) * value2.z + (float)(int)_lightingAround[LightingAround.Pos.X0Y1Z0].sun * (1f - value2.x) * value2.y * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X0Y1Z1].sun * (1f - value2.x) * value2.y * value2.z + (float)(int)_lightingAround[LightingAround.Pos.X1Y1Z0].sun * value2.x * value2.y * (1f - value2.z) + (float)(int)_lightingAround[LightingAround.Pos.X1Y1Z1].sun * value2.x * value2.y * value2.z;
			value.r = num16 / 15f;
			voxelMesh.m_ColorVertices[idx] = value;
		}
		int num17 = voxelMesh.m_Indices.Alloc(visualMesh.Indices.Count);
		for (int j = 0; j < visualMesh.Indices.Count; j++)
		{
			voxelMesh.m_Indices[num17 + j] = voxelMesh.CurTriangleIndex + visualMesh.Indices[j];
		}
		voxelMesh.CurTriangleIndex += visualMesh.Vertices.Count;
		if (voxelMesh.m_CollVertices != null)
		{
			MySimpleMesh colliderMesh = getColliderMesh(num, visualMesh);
			num17 = voxelMesh.m_CollVertices.Alloc(colliderMesh.Vertices.Count);
			if (colliderMesh != visualMesh)
			{
				for (int k = 0; k < colliderMesh.Vertices.Count; k++)
				{
					Vector3 value4 = quaternion * (colliderMesh.Vertices[k] + vector3) + vector4;
					voxelMesh.m_CollVertices[num17 + k] = value4;
				}
			}
			else
			{
				for (int l = 0; l < colliderMesh.Vertices.Count; l++)
				{
					voxelMesh.m_CollVertices[num17 + l] = voxelMesh.m_Vertices[l + count];
				}
			}
			num17 = voxelMesh.m_CollIndices.Alloc(colliderMesh.Indices.Count);
			for (int m = 0; m < colliderMesh.Indices.Count; m++)
			{
				voxelMesh.m_CollIndices[num17 + m] = num14 + colliderMesh.Indices[m];
			}
		}
		int count3 = visualMesh.Uvs.Count;
		Rect rect = WorldConstants.MapDamageToUVRect(_blockValue);
		Vector2 vector5 = default(Vector2);
		if (bTextureArray)
		{
			num17 = voxelMesh.m_Uvs.Alloc(count3);
			for (int n = 0; n < count3; n++)
			{
				int idx2 = num17 + n;
				if (flag)
				{
					vector5.x = num6 / num10;
					vector5.y = num7 / num11;
				}
				else
				{
					vector5.x = vector2.x + visualMesh.Uvs[n].x;
					vector5.y = vector2.y + visualMesh.Uvs[n].y;
				}
				voxelMesh.m_Uvs[idx2] = vector5;
			}
			if (voxelMesh.UvsCrack == null)
			{
				return;
			}
			voxelMesh.UvsCrack.Alloc(count3);
			for (int num18 = 0; num18 < count3; num18++)
			{
				int idx3 = num17 + num18;
				if (!bImposterGenerationActive)
				{
					vector5.x = rect.x + visualMesh.Uvs[num18].x * rect.width;
					vector5.y = rect.y + visualMesh.Uvs[num18].y * rect.height;
				}
				else
				{
					vector5.x = value.g;
					vector5.y = value.a;
				}
				voxelMesh.UvsCrack[idx3] = vector5;
			}
			return;
		}
		if (!flag)
		{
			for (int num19 = 0; num19 < count3; num19++)
			{
				vector5.x = vector2.x + visualMesh.Uvs[num19].x * uVRectTiling.uv.width;
				vector5.y = vector2.y + visualMesh.Uvs[num19].y * uVRectTiling.uv.height;
				voxelMesh.m_Uvs.Add(vector5);
				if (voxelMesh.UvsCrack != null)
				{
					vector5.x = rect.x + visualMesh.Uvs[num19].x * rect.width;
					vector5.y = rect.y + visualMesh.Uvs[num19].y * rect.height;
					voxelMesh.UvsCrack.Add(vector5);
				}
			}
			return;
		}
		Vector3i vector3i = default(Vector3i);
		vector3i.x = num6;
		vector3i.y = num7;
		vector3i.z = num6;
		Vector3i vector3i2 = default(Vector3i);
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
		Vector3 vector6 = default(Vector3);
		vector6.x = vector3i2.x % vector3i.x;
		vector6.y = vector3i2.y % vector3i.y;
		vector6.z = vector3i2.z % vector3i.z;
		bool flag2 = false;
		for (int num20 = 0; num20 < 6; num20++)
		{
			MySimpleMesh visualMesh2 = getVisualMesh(num20);
			if (visualMesh2 != null && visualMesh2.Uvs.Count > 6)
			{
				flag2 = true;
				break;
			}
		}
		for (int num21 = 0; num21 < count3; num21++)
		{
			Vector3 vector7 = voxelMesh.m_Normals[count];
			bool num22 = Mathf.Abs(vector7.y) > Mathf.Abs(vector7.x) && Mathf.Abs(vector7.y) > Mathf.Abs(vector7.z);
			bool flag3 = Mathf.Abs(vector7.x) > Mathf.Abs(vector7.y) && Mathf.Abs(vector7.x) > Mathf.Abs(vector7.z);
			if (num22)
			{
				vector3i.z = uVRectTiling.blockH;
				vector6.z = vector3i2.z % vector3i.z;
			}
			Vector2 zero = Vector2.zero;
			vector7 = voxelMesh.m_Normals[count + num21];
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
					vector3i.x = uVRectTiling.blockH;
					vector3i.z = uVRectTiling.blockW;
				}
				Vector3 vector8 = voxelMesh.m_Vertices[count + num21] - _drawPos;
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
				float num23 = (((int)Mathf.Abs(vector9.x) != vector3i.x) ? (Mathf.Abs(vector9.x) % (float)vector3i.x) : Mathf.Abs(vector9.x));
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
											zero.x = (float)vector3i.x - num23;
										}
									}
									else if (flag10)
									{
										zero.x = (vector10.x + vector10.z) * 0.5f;
									}
									else
									{
										zero.x = num23;
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
										zero.x = (float)vector3i.x - num23;
									}
								}
								else if (flag10)
								{
									zero.x = (vector9.x + vector10.z) * 0.5f;
								}
								else
								{
									zero.x = num23;
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
						zero.x = num23;
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
				voxelMesh.m_Uvs.Add(new Vector2(uVRectTiling.uv.x + zero.x * uVRectTiling.uv.width, uVRectTiling.uv.y + zero.y * uVRectTiling.uv.height));
			}
			else
			{
				zero.x = Mathf.Floor(1f / uVRectTiling.uv.width * 1000f);
				zero.y = (float)uVRectTiling.blockW + Mathf.Floor(uVRectTiling.blockH * 1000);
				zero.x += uVRectTiling.uv.x;
				zero.y += uVRectTiling.uv.y;
				voxelMesh.m_Uvs.Add(zero);
			}
			if (voxelMesh.UvsCrack != null)
			{
				voxelMesh.UvsCrack.Add(new Vector2(rect.x + visualMesh.Uvs[num21].x * rect.width, rect.y + visualMesh.Uvs[num21].y * rect.height));
			}
		}
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override void renderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
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
				renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFull, _meshes);
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
					renderFace(_worldPos, _blockValue, _drawPos, blockFace, _vertices, _lightingAround, _textureFull, _meshes);
				}
				break;
			}
			}
		}
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, MeshPurpose _purpose = MeshPurpose.World)
	{
		if (!_blockValue.ischild)
		{
			int num = visualMeshes.Length;
			for (int i = 0; i < num; i++)
			{
				renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFull, _meshes, _purpose);
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
		int num = visualMeshes.Length;
		for (int i = 0; i < num; i++)
		{
			MySimpleMesh colliderMesh = getColliderMesh(i, getVisualMesh(i));
			if (colliderMesh == null)
			{
				continue;
			}
			int num2 = convertRotationCached[_blockValue.rotation, i];
			for (int j = 0; j < colliderMesh.Indices.Count; j += 3)
			{
				if (!((_v1 - colliderMesh.Vertices[colliderMesh.Indices[j]]).sqrMagnitude < 0.001f) || !((_v2 - colliderMesh.Vertices[colliderMesh.Indices[j + 1]]).sqrMagnitude < 0.001f) || !((_v3 - colliderMesh.Vertices[colliderMesh.Indices[j + 2]]).sqrMagnitude < 0.001f))
				{
					continue;
				}
				for (int k = 0; k < convertRotationCached.GetLength(1); k++)
				{
					if (convertRotationCached[_blockValue.rotation, k] == num2)
					{
						return (BlockFace)k;
					}
				}
			}
		}
		return BlockFace.None;
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

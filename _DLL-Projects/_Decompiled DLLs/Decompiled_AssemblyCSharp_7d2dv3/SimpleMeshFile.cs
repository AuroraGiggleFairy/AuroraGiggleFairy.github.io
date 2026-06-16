using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public static class SimpleMeshFile
{
	public delegate void GameObjectLoadedCallback(GameObject _go, object _userCallbackData);

	public delegate void GameObjectMeshesReadCallback(SimpleMeshInfo _meshInfo, object _userCallbackData);

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class SimpleMeshDataArray : IDisposable
	{
		public readonly struct SimpleMeshDataWrapper(SimpleMeshDataArray _array, int _offset)
		{
			[PublicizedFrom(EAccessModifier.Private)]
			public readonly SimpleMeshDataArray array = _array;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly int offset = _offset;

			public Mesh.MeshData MeshData => array.meshData[offset];

			public string Name
			{
				get
				{
					return array.names[offset];
				}
				set
				{
					array.names[offset] = value;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public Mesh.MeshDataArray meshData;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool meshDataDisposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] names;

		[PublicizedFrom(EAccessModifier.Private)]
		public SimpleMeshDataWrapper[] wrappers;

		public int Length => meshData.Length;

		public SimpleMeshDataWrapper this[int i] => wrappers[i];

		public SimpleMeshDataArray(Mesh.MeshDataArray _array)
		{
			meshData = _array;
			names = new string[meshData.Length];
			wrappers = new SimpleMeshDataWrapper[meshData.Length];
			for (int i = 0; i < wrappers.Length; i++)
			{
				wrappers[i] = new SimpleMeshDataWrapper(this, i);
			}
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				DisposeMeshData();
				names = null;
				wrappers = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DisposeMeshData()
		{
			if (!meshDataDisposed)
			{
				meshDataDisposed = true;
				meshData.Dispose();
				meshData = default(Mesh.MeshDataArray);
			}
		}

		public void ApplyAndDisposeWritableMeshData(Mesh[] meshes, MeshUpdateFlags flags = MeshUpdateFlags.Default)
		{
			Mesh.ApplyAndDisposeWritableMeshData(meshData, meshes, flags);
			meshDataDisposed = true;
		}

		public static void ReadFromReader(SimpleMeshDataWrapper _meshDataWrapper, int _version, BinaryReader _br, UVRectTiling[] _uvMapping, bool _bTextureArray)
		{
			try
			{
				Mesh.MeshData meshData = _meshDataWrapper.MeshData;
				_meshDataWrapper.Name = ((_version > 1) ? _br.ReadString() : "mesh");
				long position = _br.BaseStream.Position;
				int num = (int)_br.ReadUInt32();
				int num2 = ((_version >= 6) ? 12 : 6);
				_br.BaseStream.Seek(num * num2, SeekOrigin.Current);
				int num3 = (int)_br.ReadUInt32();
				int num4 = 0;
				if (_version > 2)
				{
					num4 += 2;
				}
				if (_version > 4)
				{
					num4++;
				}
				if (_version > 3)
				{
					num4++;
				}
				num4 += 4;
				_br.BaseStream.Seek(num3 * num4, SeekOrigin.Current);
				int num5 = (int)_br.ReadUInt32();
				int num6 = 2;
				_br.BaseStream.Seek(num5 * num6, SeekOrigin.Current);
				_br.BaseStream.Seek(position, SeekOrigin.Begin);
				VertexAttributeDescriptor[] attributes = new VertexAttributeDescriptor[4]
				{
					new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
					new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
					new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 2),
					new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, 3)
				};
				meshData.SetVertexBufferParams(num, attributes);
				meshData.SetIndexBufferParams(num5, IndexFormat.UInt16);
				num = (int)_br.ReadUInt32();
				NativeArray<Vector3> vertexData = meshData.GetVertexData<Vector3>();
				for (int i = 0; i < num; i++)
				{
					if (_version < 6)
					{
						vertexData[i] = new Vector3((float)_br.ReadInt16() / 100f, (float)_br.ReadInt16() / 100f, (float)_br.ReadInt16() / 100f);
					}
					else
					{
						vertexData[i] = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
					}
				}
				num3 = (int)_br.ReadUInt32();
				NativeArray<Vector2> vertexData2 = meshData.GetVertexData<Vector2>(2);
				NativeArray<Color> vertexData3 = meshData.GetVertexData<Color>(3);
				for (int j = 0; j < num3; j++)
				{
					int num7 = 0;
					int num8 = 0;
					if (_version > 2)
					{
						num7 = _br.ReadInt16();
					}
					if (_version > 4)
					{
						num8 = _br.ReadByte();
					}
					int num9 = ((num7 >= 0 && num7 < _uvMapping.Length) ? (_uvMapping[num7].index + num8) : 0);
					bool flag = false;
					if (_version > 3)
					{
						flag = _br.ReadBoolean();
					}
					vertexData2[j] = new Vector2((float)(int)_br.ReadUInt16() / 10000f, (float)(int)_br.ReadUInt16() / 10000f);
					if (!_bTextureArray && num9 >= 0 && num9 < _uvMapping.Length)
					{
						vertexData2[j] += new Vector2(_uvMapping[num9].uv.x, _uvMapping[num9].uv.y);
					}
					vertexData3[j] = new Color(0f, num9, 0f, flag ? 1 : 0);
				}
				num5 = (int)_br.ReadUInt32();
				NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
				for (int k = 0; k < num5; k++)
				{
					indexData[k] = _br.ReadUInt16();
				}
				meshData.subMeshCount = 1;
				meshData.SetSubMesh(0, new SubMeshDescriptor(0, num5));
			}
			finally
			{
			}
		}

		public static Mesh[] ToMeshes(SimpleMeshDataArray _meshDataArray, bool _markMeshesNoLongerReadable)
		{
			Mesh[] array = new Mesh[_meshDataArray.Length];
			for (int i = 0; i < array.Length; i++)
			{
				(array[i] = new Mesh()).name = "Simple";
			}
			_meshDataArray.ApplyAndDisposeWritableMeshData(array);
			Mesh[] array2 = array;
			foreach (Mesh obj in array2)
			{
				obj.RecalculateNormals();
				obj.RecalculateBounds();
				GameUtils.SetMeshVertexAttributes(obj);
				obj.UploadMeshData(_markMeshesNoLongerReadable);
			}
			return array;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSaveFileVersion = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cHeader = 1835365224;

	public static void WriteGameObject(BinaryWriter _bw, GameObject _go, List<ImposterCanvas> _signs = null)
	{
		try
		{
			_bw.Write(1835365224);
			_bw.Write((byte)7);
			MeshFilter[] componentsInChildren = _go.GetComponentsInChildren<MeshFilter>();
			_bw.Write((short)componentsInChildren.Length);
			int num = 0;
			MeshFilter[] array = componentsInChildren;
			foreach (MeshFilter meshFilter in array)
			{
				_bw.Write(meshFilter.transform.name);
				writeMesh(_bw, meshFilter.mesh, MeshDescription.meshes[0].textureAtlas.uvMapping);
				num += meshFilter.mesh.vertexCount;
			}
			Log.Out("Saved. Meshes: " + componentsInChildren.Length + " Vertices: " + num);
			_bw.Write((ushort)((_signs != null) ? ((ushort)_signs.Count) : 0));
			if (_signs == null)
			{
				return;
			}
			foreach (ImposterCanvas _sign in _signs)
			{
				_sign.Write(_bw);
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void writeMesh(BinaryWriter _bw, Mesh _mesh, UVRectTiling[] _uvMapping)
	{
		try
		{
			Vector3[] vertices = _mesh.vertices;
			_bw.Write((uint)vertices.Length);
			for (int i = 0; i < vertices.Length; i++)
			{
				_bw.Write(vertices[i].x);
				_bw.Write(vertices[i].y);
				_bw.Write(vertices[i].z);
			}
			int[] indices = _mesh.GetIndices(0);
			Vector2[] uv = _mesh.uv;
			Vector2[] uv2 = _mesh.uv2;
			_bw.Write((uint)uv.Length);
			for (int j = 0; j < uv.Length; j++)
			{
				int num = (int)uv2[j].x;
				int num2 = -1;
				for (int k = 0; k < _uvMapping.Length; k++)
				{
					if (_uvMapping[k].index == num || k + 1 >= _uvMapping.Length || (float)_uvMapping[k].index + _uvMapping[k].uv.width * _uvMapping[k].uv.height > (float)num)
					{
						num2 = k;
						break;
					}
				}
				if (num2 == -1)
				{
					num2 = 0;
				}
				_bw.Write((short)num2);
				_bw.Write((byte)(num - _uvMapping[num2].index));
				bool value = (double)uv2[j].y > 0.5;
				_bw.Write(value);
				_bw.Write((ushort)(uv[j].x * 10000f));
				_bw.Write((ushort)(uv[j].y * 10000f));
			}
			_bw.Write((uint)indices.Length);
			for (int l = 0; l < indices.Length; l++)
			{
				_bw.Write((ushort)indices[l]);
			}
		}
		finally
		{
		}
	}

	public static GameObject ReadGameObject(PathAbstractions.AbstractedLocation _meshLocation, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, GameObjectLoadedCallback _asyncCallback = null)
	{
		return ReadGameObject(_meshLocation.FullPath, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback);
	}

	public static GameObject ReadGameObject(string _filename, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, GameObjectLoadedCallback _asyncCallback = null)
	{
		SimpleMeshInfo _meshInfo;
		return ReadGameObject(_filename, out _meshInfo, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback);
	}

	public static GameObject ReadGameObject(string _filename, out SimpleMeshInfo _meshInfo, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, GameObjectLoadedCallback _asyncCallback = null)
	{
		_meshInfo = null;
		try
		{
			if (_asyncCallback == null)
			{
				_meshInfo = ReadGameObjectSync(_filename, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable);
				if (_meshInfo != null)
				{
					return CreateUnityObjects(_meshInfo);
				}
				return null;
			}
			Task.Run([PublicizedFrom(EAccessModifier.Internal)] () => ReadGameObjectAsync(_filename, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback));
		}
		catch (Exception e)
		{
			Log.Error("Reading mesh " + _filename + " failed:");
			Log.Exception(e);
		}
		finally
		{
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SimpleMeshInfo ReadGameObjectSync(string _filename, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable)
	{
		try
		{
			using Stream inputStream = SdFile.OpenRead(_filename);
			return ReadGameObjectSync(inputStream, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable);
		}
		catch (Exception e)
		{
			Log.Error("Reading mesh " + _filename + " failed:");
			Log.Exception(e);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static async Task ReadGameObjectAsync(string _filename, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable, object _userCallbackData, GameObjectLoadedCallback _asyncCallback)
	{
		try
		{
			await using Stream fileStream = SdFile.OpenRead(_filename);
			await ReadGameObjectAsync(fileStream, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback);
		}
		catch (Exception e)
		{
			Log.Error("Reading mesh " + _filename + " failed:");
			Log.Exception(e);
			ThreadManager.AddSingleTaskMainThread("SimpleMeshFile.ReadGameObjectAsyncCallback", [PublicizedFrom(EAccessModifier.Internal)] (object data) =>
			{
				_asyncCallback(null, data);
			}, _userCallbackData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SimpleMeshInfo ReadGameObjectSync(Stream _inputStream, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable)
	{
		using SimpleMeshDataArray meshDatas = new SimpleMeshDataArray(Mesh.AllocateWritableMeshData(readLengthFromHeaderAndReset(_inputStream)));
		readData(meshDatas, _inputStream, _bTextureArray, out var _signs);
		return createMeshInfo(meshDatas, _markMeshesNoLongerReadable, _offsetY, _mat, _signs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static async Task ReadGameObjectAsync(Stream _inputStream, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable, object _userCallbackData, GameObjectLoadedCallback _asyncCallback)
	{
		int meshCnt = readLengthFromHeaderAndReset(_inputStream);
		SimpleMeshDataArray meshDatas = new SimpleMeshDataArray(await ThreadManager.MainThreadFactory.StartNew([PublicizedFrom(EAccessModifier.Internal)] () => Mesh.AllocateWritableMeshData(meshCnt)));
		try
		{
			readData(meshDatas, _inputStream, _bTextureArray, out var signs);
			GameObject result = await ThreadManager.MainThreadFactory.StartNew([PublicizedFrom(EAccessModifier.Internal)] () => CreateUnityObjects(createMeshInfo(meshDatas, _markMeshesNoLongerReadable, _offsetY, _mat, signs)));
			ThreadManager.AddSingleTaskMainThread("SimpleMeshFile.ReadGameObjectAsyncCallback", [PublicizedFrom(EAccessModifier.Internal)] (object data) =>
			{
				_asyncCallback(result, data);
			}, _userCallbackData);
		}
		finally
		{
			if (meshDatas != null)
			{
				((IDisposable)meshDatas).Dispose();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int readLengthFromHeaderAndReset(Stream _inputStream)
	{
		long position = _inputStream.Position;
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
		pooledBinaryReader.SetBaseStream(_inputStream);
		pooledBinaryReader.ReadInt32();
		pooledBinaryReader.ReadByte();
		short result = pooledBinaryReader.ReadInt16();
		_inputStream.Seek(position, SeekOrigin.Begin);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void readData(SimpleMeshDataArray _meshDatas, Stream _inputStream, bool _bTextureArray, out List<ImposterCanvas> _signs)
	{
		_signs = null;
		try
		{
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
			pooledBinaryReader.SetBaseStream(_inputStream);
			pooledBinaryReader.ReadInt32();
			int num = pooledBinaryReader.ReadByte();
			int num2 = pooledBinaryReader.ReadInt16();
			UVRectTiling[] uvMapping = ((MeshDescription.meshes.Length != 0) ? MeshDescription.meshes[0].textureAtlas.uvMapping : new UVRectTiling[0]);
			for (int i = 0; i < num2; i++)
			{
				SimpleMeshDataArray.ReadFromReader(_meshDatas[i], num, pooledBinaryReader, uvMapping, _bTextureArray);
			}
			if (num < 7)
			{
				return;
			}
			int num3 = pooledBinaryReader.ReadUInt16();
			if (num3 > 0)
			{
				_signs = new List<ImposterCanvas>(num3);
				for (int j = 0; j < num3; j++)
				{
					_signs.Add(ImposterCanvas.Read(pooledBinaryReader));
				}
			}
		}
		finally
		{
		}
	}

	public static GameObject CreateUnityObjects(SimpleMeshInfo _meshInfo)
	{
		try
		{
			Mesh[] meshes = _meshInfo.meshes;
			string[] meshNames = _meshInfo.meshNames;
			float offsetY = _meshInfo.offsetY;
			Material material = _meshInfo.mat;
			if (!material)
			{
				material = MeshDescription.GetOpaqueMaterial();
			}
			GameObject gameObject = new GameObject();
			Transform transform = gameObject.transform;
			Vector3 localPosition = new Vector3(0f, offsetY, 0f);
			for (int i = 0; i < meshes.Length; i++)
			{
				GameObject gameObject2 = new GameObject(meshNames[i]);
				gameObject2.AddComponent<MeshFilter>().mesh = meshes[i];
				gameObject2.AddComponent<MeshRenderer>().material = material;
				Transform transform2 = gameObject2.transform;
				transform2.SetParent(transform, worldPositionStays: false);
				transform2.localPosition = localPosition;
			}
			return gameObject;
		}
		finally
		{
		}
	}

	public static Mesh[] ReadMesh(string _filename, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, GameObjectMeshesReadCallback _asyncCallback = null)
	{
		try
		{
			if (_asyncCallback == null)
			{
				return ReadMeshSync(_filename, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable);
			}
			Task.Run([PublicizedFrom(EAccessModifier.Internal)] () => ReadMeshAsync(_filename, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback));
		}
		catch (Exception e)
		{
			Log.Error("Reading mesh " + _filename + " failed:");
			Log.Exception(e);
		}
		finally
		{
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mesh[] ReadMeshSync(string _filename, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable)
	{
		using Stream inputStream = SdFile.OpenRead(_filename);
		return ReadMeshSync(inputStream, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static async Task ReadMeshAsync(string _filename, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable, object _userCallbackData, GameObjectMeshesReadCallback _asyncCallback)
	{
		try
		{
			await ReadMeshAsyncInternal(_filename, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback);
		}
		catch (Exception e)
		{
			Log.Error("Reading mesh " + _filename + " failed:");
			Log.Exception(e);
			ThreadManager.AddSingleTaskMainThread("SimpleMeshFile.ReadMeshAsyncCallback", [PublicizedFrom(EAccessModifier.Internal)] (object data) =>
			{
				_asyncCallback(null, data);
			}, _userCallbackData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mesh[] ReadMeshSync(Stream _inputStream, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable)
	{
		using SimpleMeshDataArray meshDatas = new SimpleMeshDataArray(Mesh.AllocateWritableMeshData(readLengthFromHeaderAndReset(_inputStream)));
		readData(meshDatas, _inputStream, _bTextureArray, out var _signs);
		return createMeshInfo(meshDatas, _markMeshesNoLongerReadable, _offsetY, _mat, _signs).meshes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static async Task ReadMeshAsyncInternal(string _filename, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable, object _userCallbackData, GameObjectMeshesReadCallback _asyncCallback)
	{
		Stream stream = SdFile.OpenRead(_filename);
		int meshCnt = readLengthFromHeaderAndReset(stream);
		stream.Dispose();
		SimpleMeshDataArray meshDatas = new SimpleMeshDataArray(await ThreadManager.MainThreadFactory.StartNew([PublicizedFrom(EAccessModifier.Internal)] () => Mesh.AllocateWritableMeshData(meshCnt)));
		try
		{
			stream = SdFile.OpenRead(_filename);
			readData(meshDatas, stream, _bTextureArray, out var signs);
			stream.Dispose();
			SimpleMeshInfo meshInfo = await ThreadManager.MainThreadFactory.StartNew([PublicizedFrom(EAccessModifier.Internal)] () => createMeshInfo(meshDatas, _markMeshesNoLongerReadable, _offsetY, _mat, signs));
			ThreadManager.AddSingleTaskMainThread("SimpleMeshFile.ReadMeshAsyncCallback", [PublicizedFrom(EAccessModifier.Internal)] (object data) =>
			{
				_asyncCallback(meshInfo, data);
			}, _userCallbackData);
		}
		finally
		{
			if (meshDatas != null)
			{
				((IDisposable)meshDatas).Dispose();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SimpleMeshInfo createMeshInfo(SimpleMeshDataArray _meshDatas, bool _markMeshesNoLongerReadable, float _offsetY, Material _mat, List<ImposterCanvas> _signs)
	{
		string[] array = new string[_meshDatas.Length];
		Mesh[] meshes = SimpleMeshDataArray.ToMeshes(_meshDatas, _markMeshesNoLongerReadable);
		for (int i = 0; i < _meshDatas.Length; i++)
		{
			array[i] = _meshDatas[i].Name;
		}
		return new SimpleMeshInfo(array, meshes, _offsetY, _mat, _signs);
	}
}

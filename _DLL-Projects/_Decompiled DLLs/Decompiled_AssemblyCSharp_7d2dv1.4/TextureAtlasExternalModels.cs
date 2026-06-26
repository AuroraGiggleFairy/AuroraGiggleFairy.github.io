using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class TextureAtlasExternalModels : TextureAtlas
{
	public Dictionary<string, VoxelMeshExt3dModel> Meshes = new Dictionary<string, VoxelMeshExt3dModel>();

	public override bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		try
		{
			Stream stream = new MemoryStream(_tac.meshes[_idx].MetaData.bytes);
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
			{
				pooledBinaryReader.SetBaseStream(stream);
				uint num = pooledBinaryReader.ReadUInt32();
				for (int i = 0; i < num; i++)
				{
					string key = pooledBinaryReader.ReadString();
					VoxelMeshExt3dModel voxelMeshExt3dModel = (VoxelMeshExt3dModel)VoxelMesh.Create(_idx, _tac.meshes[_idx].meshType);
					voxelMeshExt3dModel.Read(pooledBinaryReader);
					Meshes[key] = voxelMeshExt3dModel;
				}
			}
			stream.Close();
			base.LoadTextureAtlas(_idx, _tac, _bLoadTextures);
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Loading model file. " + ex.Message);
			Log.Error(ex.StackTrace);
			return false;
		}
	}
}

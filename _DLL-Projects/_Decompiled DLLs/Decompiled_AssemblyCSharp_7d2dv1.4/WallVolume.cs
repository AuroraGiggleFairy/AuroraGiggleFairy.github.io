using System.IO;
using UnityEngine;

public class WallVolume
{
	public const int BinarySize = 25;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte VERSION = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance prefabInstance;

	public Vector3i BoxMin;

	public Vector3i BoxMax;

	public Vector3 Center;

	public PrefabInstance PrefabInstance => prefabInstance;

	public static WallVolume Create(Prefab.PrefabWallVolume psv, Vector3i _boxMin, Vector3i _boxMax)
	{
		WallVolume wallVolume = new WallVolume();
		wallVolume.SetMinMax(_boxMin, _boxMax);
		wallVolume.AddToPrefabInstance();
		return wallVolume;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMinMax(Vector3i _boxMin, Vector3i _boxMax)
	{
		BoxMin = _boxMin;
		BoxMax = _boxMax;
		Center = (BoxMin + BoxMax).ToVector3() * 0.5f;
	}

	public void AddToPrefabInstance()
	{
		prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetPrefabAtPosition(Center);
		if (prefabInstance != null)
		{
			prefabInstance.AddWallVolume(this);
		}
	}

	public static WallVolume Read(BinaryReader _br)
	{
		WallVolume wallVolume = new WallVolume();
		_br.ReadByte();
		wallVolume.SetMinMax(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()), new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()));
		return wallVolume;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)1);
		_bw.Write(BoxMin.x);
		_bw.Write(BoxMin.y);
		_bw.Write(BoxMin.z);
		_bw.Write(BoxMax.x);
		_bw.Write(BoxMax.y);
		_bw.Write(BoxMax.z);
	}
}

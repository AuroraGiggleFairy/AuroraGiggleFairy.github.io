using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TriggerVolume
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte VERSION = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerYOffset = 0.8f;

	public static Vector3i chunkPadding = new Vector3i(12, 1, 12);

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance prefabInstance;

	public List<byte> TriggersIndices = new List<byte>();

	public Vector3i BoxMin;

	public Vector3i BoxMax;

	public Vector3 Center;

	public bool isTriggered;

	public PrefabInstance PrefabInstance => prefabInstance;

	public static TriggerVolume Create(Prefab.PrefabTriggerVolume psv, Vector3i _boxMin, Vector3i _boxMax)
	{
		TriggerVolume triggerVolume = new TriggerVolume();
		triggerVolume.SetMinMax(_boxMin, _boxMax);
		triggerVolume.TriggersIndices = new List<byte>(psv.TriggersIndices);
		triggerVolume.AddToPrefabInstance();
		return triggerVolume;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMinMax(Vector3i _boxMin, Vector3i _boxMax)
	{
		BoxMin = _boxMin;
		BoxMax = _boxMax;
		Center = ((BoxMin + BoxMax) * 0.5f).ToVector3();
	}

	public void AddToPrefabInstance()
	{
		prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetPrefabAtPosition(Center);
		if (prefabInstance != null)
		{
			prefabInstance.AddTriggerVolume(this);
		}
	}

	public void Reset()
	{
		isTriggered = false;
	}

	public bool HasAnyTriggers()
	{
		return TriggersIndices.Count > 0;
	}

	public void CheckTouching(World _world, EntityPlayer _player)
	{
		if (!isTriggered)
		{
			Vector3 position = _player.position;
			position.y += 0.8f;
			if (position.x >= (float)BoxMin.x && position.x < (float)BoxMax.x && position.y >= (float)BoxMin.y && position.y < (float)BoxMax.y && position.z >= (float)BoxMin.z && position.z < (float)BoxMax.z)
			{
				Touch(_world, _player);
			}
		}
	}

	public bool Intersects(Bounds bounds)
	{
		return BoundsUtils.Intersects(bounds, BoxMin, BoxMax);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Touch(World _world, EntityPlayer _player)
	{
		isTriggered = true;
		_world.triggerManager.TriggerBlocks(_player, prefabInstance, this);
	}

	public static TriggerVolume Read(BinaryReader _br)
	{
		TriggerVolume triggerVolume = new TriggerVolume();
		int num = _br.ReadByte();
		triggerVolume.SetMinMax(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()), new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()));
		int num2 = _br.ReadByte();
		triggerVolume.TriggersIndices.Clear();
		for (int i = 0; i < num2; i++)
		{
			triggerVolume.TriggersIndices.Add(_br.ReadByte());
		}
		if (num > 1)
		{
			triggerVolume.isTriggered = _br.ReadBoolean();
		}
		return triggerVolume;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)2);
		_bw.Write(BoxMin.x);
		_bw.Write(BoxMin.y);
		_bw.Write(BoxMin.z);
		_bw.Write(BoxMax.x);
		_bw.Write(BoxMax.y);
		_bw.Write(BoxMax.z);
		_bw.Write((byte)TriggersIndices.Count);
		for (int i = 0; i < TriggersIndices.Count; i++)
		{
			_bw.Write(TriggersIndices[i]);
		}
		_bw.Write(isTriggered);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawVolume()
	{
		Vector3 start = BoxMin.ToVector3();
		start -= Origin.position;
		Vector3 vector = BoxMax.ToVector3();
		vector -= Origin.position;
		Debug.DrawLine(start, new Vector3(start.x, start.y, vector.z), Color.blue, 1f);
		Debug.DrawLine(start, new Vector3(vector.x, start.y, start.z), Color.blue, 1f);
		Debug.DrawLine(new Vector3(start.x, start.y, vector.z), new Vector3(vector.x, start.y, vector.z), Color.blue, 1f);
		Debug.DrawLine(new Vector3(vector.x, start.y, start.z), new Vector3(vector.x, start.y, vector.z), Color.blue, 1f);
		Debug.DrawLine(new Vector3(start.x, vector.y, start.z), new Vector3(start.x, vector.y, vector.z), Color.cyan, 1f);
		Debug.DrawLine(new Vector3(start.x, vector.y, start.z), new Vector3(vector.x, vector.y, start.z), Color.cyan, 1f);
		Debug.DrawLine(new Vector3(start.x, vector.y, vector.z), new Vector3(vector.x, vector.y, vector.z), Color.cyan, 1f);
		Debug.DrawLine(new Vector3(vector.x, vector.y, start.z), new Vector3(vector.x, vector.y, vector.z), Color.cyan, 1f);
	}

	public void DrawDebugLines(float _duration)
	{
		string name = $"TriggerVolume{BoxMin},{BoxMax}";
		Color color = new Color(0.1f, 0.1f, 1f);
		if (isTriggered)
		{
			color = new Color(0f, 0f, 0.5f, 0.16f);
		}
		Vector3 cornerPos = BoxMin.ToVector3();
		Vector3 cornerPos2 = BoxMax.ToVector3();
		cornerPos += DebugLines.InsideOffsetV * 2f;
		cornerPos2 -= DebugLines.InsideOffsetV * 2f;
		DebugLines.Create(name, GameManager.Instance.World.GetPrimaryPlayer().RootTransform, color, color, 0.03f, 0.03f, _duration).AddCube(cornerPos, cornerPos2);
	}
}

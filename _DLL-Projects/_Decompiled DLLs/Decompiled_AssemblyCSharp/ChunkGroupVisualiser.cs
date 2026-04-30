using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGroupVisualiser : SingletonMonoBehaviour<ChunkGroupVisualiser>
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float alpha = 0.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<(Color color, List<Vector3> positions)> groupInfos = new List<(Color, List<Vector3>)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrawGizmos()
	{
		Vector3 size = Vector3.one * 16f;
		size.y = 128f;
		Vector3 vector = -Origin.Instance.OriginPos;
		vector.y = 0f;
		Gizmos.matrix = Matrix4x4.Translate(vector);
		foreach (var groupInfo in groupInfos)
		{
			(Gizmos.color, _) = groupInfo;
			foreach (Vector3 item in groupInfo.positions)
			{
				Gizmos.DrawCube(item, size);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetGroups(IEnumerable<HashSetLong> chunkGroups)
	{
		groupInfos.Clear();
		UnityEngine.Random.InitState(42);
		foreach (HashSetLong chunkGroup in chunkGroups)
		{
			(Color, List<Vector3>) item = (UnityEngine.Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f, 0.25f, 0.25f), new List<Vector3>(chunkGroup.Count));
			foreach (long item2 in chunkGroup)
			{
				int num = WorldChunkCache.extractX(item2) * 16 + 8;
				int num2 = WorldChunkCache.extractZ(item2) * 16 + 8;
				item.Item2.Add(new Vector3(num, 8f, num2));
			}
			groupInfos.Add(item);
		}
	}
}

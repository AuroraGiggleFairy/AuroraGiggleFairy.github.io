using System.Collections.Generic;
using UnityEngine;

public static class CollectWaterUtils
{
	public struct WaterPoint
	{
		public Vector3i worldPos;

		public int mass;

		public int massToTake;

		public int finalMass => mass - massToTake;

		public WaterPoint(Vector3i _pos, int _mass)
		{
			worldPos = _pos;
			mass = _mass;
			massToTake = _mass;
		}
	}

	public static int CollectWater(ChunkCluster cc, int requiredMass, Vector3i origin, int maxRadius, List<WaterPoint> points)
	{
		int num = requiredMass;
		for (int i = 0; i <= maxRadius; i++)
		{
			int num2 = 0;
			int num3 = 0;
			foreach (Vector3i item in GenerateCollectionPositions(origin, i))
			{
				WaterValue water = cc.GetWater(item);
				if (water.HasMass())
				{
					int mass = water.GetMass();
					points.Add(new WaterPoint(item, mass));
					num2 += mass;
					num3++;
				}
			}
			if (num2 > num)
			{
				int num4 = num2 - num;
				int a = (num2 - num) / num3;
				a = Mathf.Max(a, 1);
				int num5 = points.Count - num3;
				while (num4 > 0)
				{
					WaterPoint value = points[num5];
					if (value.massToTake > 0)
					{
						int num6 = Mathf.Min(a, value.massToTake);
						value.massToTake -= num6;
						num4 -= num6;
						num2 -= num6;
						points[num5] = value;
					}
					num5++;
					if (num5 == points.Count)
					{
						num5 = points.Count - num3;
					}
				}
			}
			num -= num2;
			if (num <= 0)
			{
				break;
			}
		}
		return requiredMass - num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateCollectionPositions(Vector3i center, int radius)
	{
		if (radius <= 0)
		{
			yield return center;
			yield break;
		}
		for (int x = -radius; x <= radius; x++)
		{
			int remainingYZ = radius - Mathf.Abs(x);
			for (int y = -remainingYZ; y <= remainingYZ; y++)
			{
				int zAbs = remainingYZ - Mathf.Abs(y);
				if (zAbs == 0)
				{
					yield return new Vector3i(center.x + x, center.y + y, center.z);
					continue;
				}
				yield return new Vector3i(center.x + x, center.y + y, center.z + zAbs);
				yield return new Vector3i(center.x + x, center.y + y, center.z - zAbs);
			}
		}
	}
}

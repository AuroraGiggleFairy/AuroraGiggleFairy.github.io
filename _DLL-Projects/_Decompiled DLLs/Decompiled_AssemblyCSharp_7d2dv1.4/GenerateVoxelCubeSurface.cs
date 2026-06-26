using System.Collections.Generic;

public class GenerateVoxelCubeSurface
{
	public static IEnumerable<Vector3i> GenerateCubeSurfacePositions(Vector3i origin, int radius)
	{
		if (radius <= 0)
		{
			yield return origin;
			yield break;
		}
		foreach (Vector3i item in GenerateCubeSurfaceTop(origin, radius))
		{
			yield return item;
		}
		foreach (Vector3i item2 in GenerateCubeSurfaceBottom(origin, radius))
		{
			yield return item2;
		}
		foreach (Vector3i item3 in GenerateCubeSurfaceLeft(origin, radius))
		{
			yield return item3;
		}
		foreach (Vector3i item4 in GenerateCubeSurfaceRight(origin, radius))
		{
			yield return item4;
		}
		foreach (Vector3i item5 in GenerateCubeSurfaceFront(origin, radius))
		{
			yield return item5;
		}
		foreach (Vector3i item6 in GenerateCubeSurfaceBack(origin, radius))
		{
			yield return item6;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateCubeSurfaceTop(Vector3i origin, int radius)
	{
		origin.x -= radius;
		origin.y += radius;
		origin.z -= radius;
		int num = radius * 2 + 1;
		foreach (Vector3i item in GenerateXZ(origin, num, num))
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateCubeSurfaceBottom(Vector3i origin, int radius)
	{
		origin.x -= radius;
		origin.y -= radius;
		origin.z -= radius;
		int num = radius * 2 + 1;
		foreach (Vector3i item in GenerateXZ(origin, num, num))
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateCubeSurfaceRight(Vector3i origin, int radius)
	{
		origin.x += radius;
		origin.y -= radius - 1;
		origin.z -= radius;
		int num = radius * 2 + 1;
		foreach (Vector3i item in GenerateYZ(origin, num - 2, num))
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateCubeSurfaceLeft(Vector3i origin, int radius)
	{
		origin.x -= radius;
		origin.y -= radius - 1;
		origin.z -= radius;
		int num = radius * 2 + 1;
		foreach (Vector3i item in GenerateYZ(origin, num - 2, num))
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateCubeSurfaceFront(Vector3i origin, int radius)
	{
		origin.x -= radius - 1;
		origin.y -= radius - 1;
		origin.z += radius;
		int num = radius * 2 - 1;
		foreach (Vector3i item in GenerateXY(origin, num, num))
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateCubeSurfaceBack(Vector3i origin, int radius)
	{
		origin.x -= radius - 1;
		origin.y -= radius - 1;
		origin.z -= radius;
		int num = radius * 2 - 1;
		foreach (Vector3i item in GenerateXY(origin, num, num))
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateXZ(Vector3i min, int xLength, int zLength)
	{
		if (min.y < 0 || min.y > 255)
		{
			yield break;
		}
		int xEnd = min.x + xLength;
		int zEnd = min.z + zLength;
		for (int x = min.x; x < xEnd; x++)
		{
			for (int z = min.z; z < zEnd; z++)
			{
				yield return new Vector3i(x, min.y, z);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateYZ(Vector3i min, int yLength, int zLength)
	{
		int yEnd = min.y + yLength;
		int zEnd = min.z + zLength;
		for (int y = min.y; y < yEnd; y++)
		{
			if (y >= 0 && y < 256)
			{
				for (int z = min.z; z < zEnd; z++)
				{
					yield return new Vector3i(min.x, y, z);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<Vector3i> GenerateXY(Vector3i min, int xLength, int yLength)
	{
		int xEnd = min.x + xLength;
		int yEnd = min.y + yLength;
		for (int y = min.y; y < yEnd; y++)
		{
			if (y >= 0 && y < 256)
			{
				for (int x = min.x; x < xEnd; x++)
				{
					yield return new Vector3i(x, y, min.z);
				}
			}
		}
	}
}

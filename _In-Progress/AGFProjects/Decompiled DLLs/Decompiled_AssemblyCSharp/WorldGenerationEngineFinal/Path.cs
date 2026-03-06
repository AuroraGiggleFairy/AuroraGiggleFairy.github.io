using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class Path
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSingleLaneRadius = 4.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cShoulderWidth = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBlendDistCountry = 6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBlendDistHighway = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHeightSmoothAverageBias = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHeightSmoothDecreasePer = 0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	public readonly Vector2i StartPosition;

	public readonly Vector2i EndPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lanes;

	[PublicizedFrom(EAccessModifier.Private)]
	public float radius = 8f;

	public bool isCountryRoad;

	public bool isRiver;

	public bool connectsToHighway;

	public int Cost;

	public bool IsValid;

	public NativeList<Vector2> FinalPathPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public NativeList<Vector3> pathPoints3d = new NativeList<Vector3>(Allocator.Persistent);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FreeId = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CountryId = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int HighwayId = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int HighwayDirtId = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int WaterId = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int HighwayBlendIdMask = 128;

	public Path(WorldBuilder _worldBuilder, Vector2i _startPosition, Vector2i _endPosition, int _lanes, bool _isCountryRoad)
	{
		worldBuilder = _worldBuilder;
		StartPosition = _startPosition;
		EndPosition = _endPosition;
		lanes = _lanes;
		radius = (float)lanes * 0.5f * 4.5f;
		isCountryRoad = _isCountryRoad;
		CreatePath();
	}

	public Path(WorldBuilder _worldBuilder, Vector2i _startPosition, Vector2i _endPosition, float _radius, bool _isCountryRoad)
	{
		worldBuilder = _worldBuilder;
		StartPosition = _startPosition;
		EndPosition = _endPosition;
		radius = _radius;
		lanes = Mathf.CeilToInt(radius / 4.5f * 2f);
		isCountryRoad = _isCountryRoad;
		CreatePath();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~Path()
	{
		Cleanup();
	}

	public void Cleanup()
	{
		FinalPathPoints.Dispose();
		pathPoints3d.Dispose();
	}

	public void RemoveFromStreetTiles()
	{
		if (isCountryRoad || !FinalPathPoints.IsCreated)
		{
			return;
		}
		StreetTile streetTile = null;
		Vector2i pos = default(Vector2i);
		for (int i = 0; i < FinalPathPoints.Length; i++)
		{
			pos.x = (int)FinalPathPoints[i].x;
			pos.y = (int)FinalPathPoints[i].y;
			StreetTile streetTileWorld = worldBuilder.GetStreetTileWorld(pos);
			if (streetTileWorld != streetTile)
			{
				streetTileWorld?.RemoveHighway(this);
			}
			streetTile = streetTileWorld;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreatePath()
	{
		if (StartPosition.x < 8 || StartPosition.y < 8 || EndPosition.x < 8 || EndPosition.y < 8)
		{
			Log.Error("CreatePath position oob {0} to {1}", StartPosition, EndPosition);
			return;
		}
		NativeList<Vector2i> path = worldBuilder.PathingUtils.GetPath(StartPosition, EndPosition, isCountryRoad, isRiver);
		if (path.Length <= 1)
		{
			return;
		}
		IsValid = true;
		FinalPathPoints = new NativeList<Vector2>(16, Allocator.Persistent);
		Vector2 value = new Vector2(EndPosition.x, EndPosition.y);
		FinalPathPoints.Add(in value);
		for (int i = 1; i < path.Length; i++)
		{
			if (!(DistanceSqr(path[i], StartPosition) < 16f) && !(DistanceSqr(path[i], EndPosition) < 16f))
			{
				value.x = path[i].x;
				value.y = path[i].y;
				FinalPathPoints.Add(in value);
			}
		}
		FinalPathPoints.Add(new Vector2(StartPosition.x, StartPosition.y));
		ProcessPath();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPath()
	{
		float weight = (isCountryRoad ? 0.3f : 0.4f);
		int skipIndex = ((!isCountryRoad) ? 2 : 0);
		for (int i = 0; i < 4; i++)
		{
			RoundOffCorners(weight, skipIndex);
		}
		float num = worldBuilder.WorldSize;
		float num2 = 0f;
		for (int j = 0; j < FinalPathPoints.Length; j++)
		{
			float x = FinalPathPoints[j].x;
			if (x < 0f || x >= num)
			{
				IsValid = false;
				return;
			}
			float y = FinalPathPoints[j].y;
			if (y < 0f || y >= num)
			{
				IsValid = false;
				return;
			}
			if (j > 0)
			{
				num2 += Vector2.Distance(FinalPathPoints[j - 1], FinalPathPoints[j]);
			}
			pathPoints3d.Add(new Vector3(x, worldBuilder.GetHeight((int)x, (int)y), y));
		}
		Cost = Mathf.RoundToInt(num2);
		float[] heights = new float[pathPoints3d.Length];
		if (isCountryRoad)
		{
			for (int k = 0; k < 4; k++)
			{
				SmoothHeights(heights);
			}
		}
		else
		{
			float num3 = 0f;
			for (int l = 0; l < pathPoints3d.Length; l++)
			{
				num3 += pathPoints3d[l].y;
			}
			num3 /= (float)pathPoints3d.Length;
			num3 += 8f;
			for (int m = 0; m < pathPoints3d.Length; m++)
			{
				Vector3 value = pathPoints3d[m];
				if (value.y > num3)
				{
					int index = (int)value.x + (int)value.z * worldBuilder.WorldSize;
					if (worldBuilder.data.poiHeightMask[index] == 0)
					{
						value.y = num3 * 0.3f + value.y * 0.7f;
						pathPoints3d[m] = value;
					}
				}
			}
			for (int n = 0; n < 50; n++)
			{
				SmoothHeights(heights);
			}
		}
		FinalPathPoints.Clear();
		Vector2 value2 = Vector2.zero;
		for (int num4 = 0; num4 < pathPoints3d.Length; num4++)
		{
			value2.x = (int)(pathPoints3d[num4].x + 0.5f);
			value2.y = (int)(pathPoints3d[num4].z + 0.5f);
			FinalPathPoints.Add(in value2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RoundOffCorners(float _weight, int _skipIndex)
	{
		float num = _weight * 0.5f;
		float num2 = 1f - _weight;
		_skipIndex++;
		int num3 = FinalPathPoints.Length - _skipIndex;
		for (int i = _skipIndex; i < num3; i++)
		{
			FinalPathPoints[i] = FinalPathPoints[i] * num2 + (FinalPathPoints[i - 1] + FinalPathPoints[i + 1]) * num;
		}
	}

	public void DrawPathToRoadIds(byte[] ids)
	{
		float num = radius;
		num = ((!isCountryRoad) ? (num + 10f) : (num + 6f));
		float num2 = ((lanes >= 2) ? (radius - 1f) : radius);
		float num3 = radius * radius;
		float num4 = num * num;
		float num5 = num2 * num2;
		Vector2 point = default(Vector2);
		for (int i = 0; i < FinalPathPoints.Length - 1; i++)
		{
			float x = FinalPathPoints[i].x;
			float y = FinalPathPoints[i].y;
			float x2 = FinalPathPoints[i + 1].x;
			float y2 = FinalPathPoints[i + 1].y;
			int v = (int)(Utils.FastMin(x, x2) - num - 1.5f);
			v = Utils.FastMax(0, v);
			int v2 = (int)(Utils.FastMax(x, x2) + num + 1.5f);
			v2 = Utils.FastMin(v2, worldBuilder.WorldSize - 1);
			int v3 = (int)(Utils.FastMin(y, y2) - num - 1.5f);
			v3 = Utils.FastMax(0, v3);
			int v4 = (int)(Utils.FastMax(y, y2) + num + 1.5f);
			v4 = Utils.FastMin(v4, worldBuilder.WorldSize - 1);
			for (int j = v3; j < v4; j++)
			{
				point.y = j;
				for (int k = v; k < v2; k++)
				{
					point.x = k;
					Vector2 pointOnLine;
					float num6 = GetPointToLineDistanceSq(point, FinalPathPoints[i], FinalPathPoints[i + 1], out pointOnLine);
					float num7;
					if (num6 < num4)
					{
						num7 = Utils.FastClamp01(Vector2.Distance(FinalPathPoints[i], pointOnLine) / Vector2.Distance(FinalPathPoints[i], FinalPathPoints[i + 1]));
					}
					else
					{
						num6 = distanceSqr(k, j, FinalPathPoints[i]);
						if (num6 >= num4)
						{
							continue;
						}
						float num8 = distanceSqr(k, j, FinalPathPoints[i + 1]);
						if (num6 > num8)
						{
							continue;
						}
						if (i > 0)
						{
							float num9 = distanceSqr(k, j, FinalPathPoints[i - 1]);
							if (num6 >= num9 || GetPointToLineDistanceSq(point, FinalPathPoints[i - 1], FinalPathPoints[i], out var _) < num4)
							{
								continue;
							}
						}
						num7 = -1f;
					}
					int num10 = k + j * worldBuilder.WorldSize;
					if (isRiver)
					{
						if (num6 <= num3)
						{
							ids[num10] = 4;
							if (worldBuilder.GetHeight(k, j) < (float)worldBuilder.WaterHeight)
							{
								worldBuilder.SetWater(k, j, (byte)worldBuilder.WaterHeight);
								continue;
							}
							worldBuilder.SetWater(k, j, (byte)worldBuilder.WaterHeight);
						}
					}
					else
					{
						int num11 = ids[num10];
						if (num11 == 2 || (num6 > num3 && (num11 & 0x80) > 0))
						{
							continue;
						}
						if (!isCountryRoad)
						{
							if (num6 > num3)
							{
								ids[num10] |= 128;
							}
							else if (num6 > num5)
							{
								ids[num10] = 3;
							}
							else
							{
								ids[num10] = 2;
							}
						}
						else if (num6 <= num3)
						{
							ids[num10] = 1;
						}
					}
					float height = worldBuilder.GetHeight(k, j);
					float v5 = 3f;
					if (!isRiver)
					{
						v5 = Utils.FastMax(worldBuilder.data.GetWater(k, j), worldBuilder.WaterHeight) + 1;
					}
					float num12 = pathPoints3d[i].y;
					if (num7 > 0f)
					{
						num12 = Utils.FastLerpUnclamped(num12, pathPoints3d[i + 1].y, num7);
					}
					num12 = Utils.FastMax(v5, num12);
					float num13 = Utils.FastClamp01((Mathf.Sqrt(num6) - radius) / (num - radius));
					num13 *= num13;
					num12 = Utils.FastLerpUnclamped(num12, height, num13);
					worldBuilder.SetHeightTrusted(k, j, num12);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetPointToLineDistanceSq(Vector2 point, Vector2 lineStart, Vector2 lineEnd, out Vector2 pointOnLine)
	{
		Vector2 vector = default(Vector2);
		vector.x = lineEnd.x - lineStart.x;
		vector.y = lineEnd.y - lineStart.y;
		float num = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
		vector.x /= num;
		vector.y /= num;
		float num2 = Vector2.Dot(point - lineStart, vector);
		if (num2 < 0f || num2 > num)
		{
			pointOnLine = new Vector2(100000f, 100000f);
			return float.MaxValue;
		}
		pointOnLine = lineStart + num2 * vector;
		return distanceSqr(point, pointOnLine);
	}

	public bool Crosses(Path path)
	{
		foreach (Vector2 finalPathPoint in FinalPathPoints)
		{
			foreach (Vector2 finalPathPoint2 in path.FinalPathPoints)
			{
				if (distanceSqr(finalPathPoint, finalPathPoint2) < 100f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsConnectedTo(Path path)
	{
		if (Crosses(path))
		{
			return true;
		}
		foreach (Vector2 finalPathPoint in path.FinalPathPoints)
		{
			if (distanceSqr(StartPosition.AsVector2(), finalPathPoint) < 100f)
			{
				return true;
			}
			if (distanceSqr(EndPosition.AsVector2(), finalPathPoint) < 100f)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsConnectedToHighway()
	{
		if (worldBuilder.PathingUtils.IsPointOnHighwayWorld(StartPosition.x, StartPosition.y))
		{
			return true;
		}
		if (worldBuilder.PathingUtils.IsPointOnHighwayWorld(EndPosition.x, EndPosition.y))
		{
			return true;
		}
		foreach (Vector2 finalPathPoint in FinalPathPoints)
		{
			if (worldBuilder.PathingUtils.IsPointOnHighwayWorld((int)finalPathPoint.x, (int)finalPathPoint.y))
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float DistanceSqr(Vector2i v1, Vector2i v2)
	{
		int num = v1.x - v2.x;
		int num2 = v1.y - v2.y;
		return num * num + num2 * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float distanceSqr(Vector2 v1, Vector2 v2)
	{
		float num = v1.x - v2.x;
		float num2 = v1.y - v2.y;
		return num * num + num2 * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float distanceSqr(float x, float y, Vector2 v2)
	{
		float num = x - v2.x;
		float num2 = y - v2.y;
		return num * num + num2 * num2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector3> romChain(List<Vector3> points, int numberOfPointsOnCurve = 5, float parametricSplineVal = 0.2f)
	{
		List<Vector3> list = new List<Vector3>(points.Count * numberOfPointsOnCurve + 1);
		for (int i = 0; i < points.Count - 3; i++)
		{
			list.AddRange(catmulRom(points[i], points[i + 1], points[i + 2], points[i + 3], numberOfPointsOnCurve, parametricSplineVal));
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> catmulRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int numberOfPointsOnCurve, float parametricSplineVal)
	{
		List<Vector3> list = new List<Vector3>(numberOfPointsOnCurve + 1);
		float t = getT(0f, p0, p1, parametricSplineVal);
		float t2 = getT(t, p1, p2, parametricSplineVal);
		float t3 = getT(t2, p2, p3, parametricSplineVal);
		for (float num = t; num < t2; num += (t2 - t) / (float)numberOfPointsOnCurve)
		{
			Vector3 vector = (t - num) / (t - 0f) * p0 + (num - 0f) / (t - 0f) * p1;
			Vector3 vector2 = (t2 - num) / (t2 - t) * p1 + (num - t) / (t2 - t) * p2;
			Vector3 vector3 = (t3 - num) / (t3 - t2) * p2 + (num - t2) / (t3 - t2) * p3;
			Vector3 vector4 = (t2 - num) / (t2 - 0f) * vector + (num - 0f) / (t2 - 0f) * vector2;
			Vector3 vector5 = (t3 - num) / (t3 - t) * vector2 + (num - t) / (t3 - t) * vector3;
			Vector3 item = (t2 - num) / (t2 - t) * vector4 + (num - t) / (t2 - t) * vector5;
			list.Add(item);
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getT(float t, Vector3 p0, Vector3 p1, float alpha)
	{
		if (p0 == p1)
		{
			return t;
		}
		return Mathf.Pow((p1 - p0).sqrMagnitude, 0.5f * alpha) + t;
	}

	public void CommitPathingMapData()
	{
		PathTile tile = default(PathTile);
		tile.TileState = (isCountryRoad ? PathTile.PathTileStates.Country : PathTile.PathTileStates.Highway);
		Vector2i vector2i = default(Vector2i);
		for (int i = 0; i < FinalPathPoints.Length - 1; i++)
		{
			vector2i.x = (int)FinalPathPoints[i].x;
			vector2i.y = (int)FinalPathPoints[i].y;
			if (!isCountryRoad)
			{
				SetPathTileAnd4WayWorld(vector2i.x, vector2i.y, tile);
				worldBuilder.GetStreetTileWorld(vector2i.x, vector2i.y)?.AddHighway(this);
			}
			else
			{
				SetPathTileWorld(vector2i.x, vector2i.y, tile);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPathTileWorld(int x, int y, PathTile _tile)
	{
		x /= 10;
		if ((uint)x < worldBuilder.data.PathTileGridWidth)
		{
			y /= 10;
			if ((uint)y < worldBuilder.data.PathTileGridWidth)
			{
				int index = x + y * worldBuilder.data.PathTileGridWidth;
				worldBuilder.data.PathTileGrid[index] = _tile;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPathTileAnd4WayWorld(int x, int y, PathTile _tile)
	{
		int pathTileGridWidth = worldBuilder.data.PathTileGridWidth;
		x /= 10;
		x -= 2;
		if ((uint)x < pathTileGridWidth - 4)
		{
			y /= 10;
			y -= 2;
			if ((uint)y < pathTileGridWidth - 4)
			{
				int num = x + y * pathTileGridWidth;
				worldBuilder.data.PathTileGrid[num + 2] = _tile;
				num += pathTileGridWidth;
				worldBuilder.data.PathTileGrid[num + 2] = _tile;
				num += pathTileGridWidth;
				worldBuilder.data.PathTileGrid[num] = _tile;
				worldBuilder.data.PathTileGrid[num + 1] = _tile;
				worldBuilder.data.PathTileGrid[num + 2] = _tile;
				worldBuilder.data.PathTileGrid[num + 3] = _tile;
				worldBuilder.data.PathTileGrid[num + 4] = _tile;
				num += pathTileGridWidth;
				worldBuilder.data.PathTileGrid[num + 2] = _tile;
				num += pathTileGridWidth;
				worldBuilder.data.PathTileGrid[num + 2] = _tile;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmoothHeights(float[] heights)
	{
		for (int i = 0; i < pathPoints3d.Length; i++)
		{
			heights[i] = pathPoints3d[i].y;
		}
		for (int j = 1; j < pathPoints3d.Length - 1; j++)
		{
			Vector3 value = pathPoints3d[j];
			int index = (int)value.x + (int)value.z * worldBuilder.WorldSize;
			if (worldBuilder.data.poiHeightMask[index] == 0)
			{
				value.y = (heights[j - 1] + heights[j] + heights[j + 1]) * (1f / 3f);
				pathPoints3d[j] = value;
			}
		}
	}
}

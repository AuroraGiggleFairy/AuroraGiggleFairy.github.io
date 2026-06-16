using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIAirDrop
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class SupplyCrateSpawn
	{
		public float Delay;

		public Vector3 SpawnPos;

		public ChunkManager.ChunkObserver ChunkRef;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class PlayerCluster
	{
		public Vector2 XZCenter;

		public float Radius;

		public List<EntityPlayer> Players = new List<EntityPlayer>();

		public float Delay;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class FlightPath
	{
		public List<SupplyCrateSpawn> Crates = new List<SupplyCrateSpawn>();

		public Vector3 Start;

		public Vector3 End;

		public float Delay;

		public bool Spawned;
	}

	public const float cPlaneMetersPerSecond = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxPlayerClusterRadius = 70f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinPlayerClusterRadius = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxPlaneTangentPointRadius = 750f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinPlaneTangentPointRadius = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinPlaneFlightVector = 1500f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxPlaneFlightVector = 2000f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinDropRange = 150f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMaxDropRange = 700f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kMaxDropsPerPlane = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kSpawnYUp = 180;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorAirDropComponent controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlayerCluster> clusters;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<FlightPath> flightPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawningCrates;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity eSupplyPlane;

	public AIAirDrop(AIDirectorAirDropComponent _airDropController, World _world, List<EntityPlayer> _players)
	{
		controller = _airDropController;
		world = _world;
		numPlayers = _players.Count;
		MakePlayerClusters(_players);
	}

	public bool Tick(float dt)
	{
		if (flightPaths == null)
		{
			CreateFlightPaths();
			Log.Out("AIAirDrop: Computed flight paths for " + flightPaths.Count + " aircraft.");
			Log.Out("AIAirDrop: Waiting for supply crate chunk locations to load...");
		}
		if (!spawningCrates)
		{
			bool flag = true;
			for (int i = 0; i < flightPaths.Count; i++)
			{
				FlightPath flightPath = flightPaths[i];
				for (int j = 0; j < flightPath.Crates.Count; j++)
				{
					SupplyCrateSpawn supplyCrateSpawn = flightPath.Crates[j];
					if ((Chunk)world.GetChunkFromWorldPos(World.worldToBlockPos(supplyCrateSpawn.SpawnPos)) == null)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			spawningCrates = flag;
		}
		if (spawningCrates)
		{
			int num = 0;
			while (num < flightPaths.Count)
			{
				FlightPath flightPath2 = flightPaths[num];
				flightPath2.Delay -= dt;
				if (flightPath2.Delay <= 0f)
				{
					if (!flightPath2.Spawned)
					{
						SpawnPlane(flightPath2);
						flightPath2.Spawned = true;
					}
					int num2 = 0;
					while (num2 < flightPath2.Crates.Count)
					{
						SupplyCrateSpawn supplyCrateSpawn2 = flightPath2.Crates[num2];
						supplyCrateSpawn2.Delay -= dt;
						if (supplyCrateSpawn2.Delay <= 0f)
						{
							controller.SpawnSupplyCrate(supplyCrateSpawn2.SpawnPos, supplyCrateSpawn2.ChunkRef);
							Log.Out("AIAirDrop: Spawned supply crate at " + supplyCrateSpawn2.SpawnPos.ToCultureInvariantString() + ", plane is at " + ((eSupplyPlane != null) ? eSupplyPlane.position : Vector3.zero).ToString());
							flightPath2.Crates.RemoveAt(num2);
						}
						else
						{
							num2++;
						}
					}
				}
				if (flightPath2.Crates.Count == 0)
				{
					flightPaths.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			if (flightPaths.Count == 0)
			{
				flightPaths = null;
			}
		}
		return flightPaths == null;
	}

	public static float Angle(Vector2 p_vector2)
	{
		if (p_vector2.x < 0f)
		{
			return 360f - Mathf.Atan2(p_vector2.x, p_vector2.y) * 57.29578f * -1f;
		}
		return Mathf.Atan2(p_vector2.x, p_vector2.y) * 57.29578f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnPlane(FlightPath _fp)
	{
		Vector3 vector = _fp.End - _fp.Start;
		Vector3 normalized = vector.normalized;
		Vector2 vector2 = new Vector2(normalized.x, normalized.z);
		EntitySupplyPlane entitySupplyPlane = (EntitySupplyPlane)(eSupplyPlane = (EntitySupplyPlane)EntityFactory.CreateEntity(EntityClass.FromString("supplyPlane"), _fp.Start, new Vector3(0f, Angle(vector2), 0f)));
		entitySupplyPlane.SetDirectionToFly(normalized, (int)(20f * (vector.magnitude / 120f + 10f)));
		world.SpawnEntityInWorld(entitySupplyPlane);
		Log.Out("AIAirDrop: Spawned aircraft at (" + _fp.Start.ToCultureInvariantString() + "), heading (" + vector2.ToCultureInvariantString() + ")");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateFlightPaths()
	{
		flightPaths = new List<FlightPath>();
		CalcSupplyDropMetrics(numPlayers, clusters.Count, out var _numCrates, out var _numPlanes);
		int num = Math.Max(1, _numCrates / _numPlanes);
		int num2 = _numCrates - _numPlanes * num;
		HashSet<int> hashSet = new HashSet<int>();
		GameRandom random = controller.Random;
		while (_numCrates > 0)
		{
			int num3 = Mathf.Min(_numCrates, num + num2);
			_numCrates -= num3;
			num2 = 0;
			int num4;
			do
			{
				num4 = random.RandomRange(0, clusters.Count);
			}
			while (hashSet.Contains(num4));
			PlayerCluster playerCluster = clusters[num4];
			hashSet.Add(num4);
			if (hashSet.Count == clusters.Count)
			{
				hashSet.Clear();
			}
			float v = playerCluster.Players[random.RandomRange(0, playerCluster.Players.Count)].position.y + 180f;
			v = Utils.FastMin(v, 276f);
			Vector2 vector = random.RandomOnUnitCircle;
			Vector2 vector2 = playerCluster.XZCenter + vector * random.RandomRange(30f, 750f);
			float num5 = random.RandomRange(150f, 700f);
			float num6 = num5 / 2f;
			float x = vector.x;
			vector.x = 0f - vector.y;
			vector.y = x;
			float num7 = random.RandomRange(1500f, 2000f) / 2f;
			Vector2 point = vector2 + -vector * (num6 + num7);
			Vector2 point2 = vector2 + vector * (num6 + num7);
			point = FindSafePoint(point, -vector, 25f, 600f);
			point2 = FindSafePoint(point2, vector, 25f, 600f);
			float num8 = num5 / (float)num3;
			float num9 = (0f - num8) * Math.Max(1f, ((float)num3 - 1f) / 2f);
			FlightPath flightPath = new FlightPath();
			flightPath.Start = new Vector3(point.x, v, point.y);
			flightPath.End = new Vector3(point2.x, v, point2.y);
			float magnitude = (flightPath.End - flightPath.Start).magnitude;
			for (int i = 0; i < num3; i++)
			{
				Vector2 vector3 = vector2 + (num9 + (float)i * num8) * vector;
				SupplyCrateSpawn supplyCrateSpawn = new SupplyCrateSpawn();
				float num10 = v - 10f;
				if (GameManager.Instance != null && GameManager.Instance.World != null)
				{
					float num11 = (int)GameManager.Instance.World.GetHeight((int)vector3.x, (int)vector3.y);
					if (num10 <= num11 + 15f)
					{
						num10 = num11 + 15f;
					}
				}
				supplyCrateSpawn.SpawnPos = ClampToMapExtents(new Vector3(vector3.x, num10, vector3.y), vector, 25f);
				if (i == 0)
				{
					vector = new Vector2(supplyCrateSpawn.SpawnPos.x, supplyCrateSpawn.SpawnPos.z) - new Vector2(flightPath.Start.x, flightPath.Start.z);
					vector.Normalize();
					flightPath.End = flightPath.Start + new Vector3(vector.x, 0f, vector.y) * magnitude;
				}
				supplyCrateSpawn.Delay = (point - vector3).magnitude / 120f;
				supplyCrateSpawn.ChunkRef = world.GetGameManager().AddChunkObserver(supplyCrateSpawn.SpawnPos, _bBuildVisualMeshAround: false, 3, -1);
				flightPath.Crates.Add(supplyCrateSpawn);
			}
			flightPath.Delay = playerCluster.Delay + random.RandomRange(0f, 15f);
			playerCluster.Delay += random.RandomRange(25f, 120f);
			flightPaths.Add(flightPath);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 FindSafePoint(Vector2 _point, Vector2 _dir, float _stepSize, float _range)
	{
		_range *= _range;
		while (true)
		{
			bool flag = true;
			for (int i = 0; i < clusters.Count; i++)
			{
				PlayerCluster playerCluster = clusters[i];
				for (int j = 0; j < playerCluster.Players.Count; j++)
				{
					EntityPlayer entityPlayer = playerCluster.Players[j];
					if ((_point - new Vector2(entityPlayer.position.x, entityPlayer.position.z)).sqrMagnitude < _range)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
			_point += _dir * _stepSize;
		}
		return _point;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakePlayerClusters(List<EntityPlayer> _players)
	{
		clusters = new List<PlayerCluster>();
		foreach (EntityPlayer _player in _players)
		{
			bool flag = true;
			for (int i = 0; i < clusters.Count; i++)
			{
				PlayerCluster cluster = clusters[i];
				if (TryAddPlayerToCluster(_player, cluster))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				PlayerCluster playerCluster = new PlayerCluster();
				playerCluster.Radius = 30f;
				playerCluster.XZCenter = new Vector2(_player.position.x, _player.position.z);
				playerCluster.Players.Add(_player);
				clusters.Add(playerCluster);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryAddPlayerToCluster(EntityPlayer _player, PlayerCluster _cluster)
	{
		Vector2 vector = _cluster.XZCenter + new Vector2(_player.position.x, _player.position.z);
		vector.Scale(new Vector2(0.5f, 0.5f));
		float num = GetPlayerDistanceSq(_player, vector);
		if (num > 70f)
		{
			return false;
		}
		for (int i = 0; i < _cluster.Players.Count; i++)
		{
			EntityPlayer player = _cluster.Players[i];
			num = Mathf.Max(num, GetPlayerDistanceSq(player, vector));
			if (num > 70f)
			{
				return false;
			}
		}
		_cluster.XZCenter = vector;
		_cluster.Radius = Mathf.Max(num, 30f);
		_cluster.Players.Add(_player);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetPlayerDistanceSq(EntityPlayer _player, Vector2 _xzPos)
	{
		return (_xzPos - new Vector2(_player.position.x, _player.position.z)).sqrMagnitude;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SelectCrateCount(int _numPlayers, out int _min, out int _max)
	{
		_min = 1;
		_max = 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcSupplyDropMetrics(int _numPlayers, int _numClusters, out int _numCrates, out int _numPlanes)
	{
		SelectCrateCount(_numPlayers, out var _min, out var _max);
		_numCrates = controller.Random.RandomRange(_min, _max + 1);
		_numPlanes = Math.Max(1, Math.Min(_numClusters + controller.Random.RandomRange(0, 2), 4));
		if (_numCrates / _numPlanes < 1 || _numCrates / _numPlanes > 3)
		{
			_numPlanes = Math.Min(1, Mathf.CeilToInt((float)_numCrates / 3f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ClampToMapExtents(Vector3 _pos, Vector2 _dir, float _step)
	{
		return world.ClampToValidWorldPos(_pos);
	}
}

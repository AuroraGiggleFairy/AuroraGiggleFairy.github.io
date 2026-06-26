using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplashCubes
{
	public enum SplashType
	{
		Splash,
		Slope,
		Area,
		Mist
	}

	public class ParticlePlacement
	{
		public Vector3i pos;

		public BlockFace dir;

		public SplashType type;

		public ParticlePlacement(Vector3i _pos, BlockFace _dir, SplashType _type)
		{
			pos = _pos;
			dir = _dir;
			type = _type;
		}
	}

	public static WaterSplashCubes instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryList<long, DictionaryList<int, GameObject>> splashes = new DictionaryList<long, DictionaryList<int, GameObject>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject root;

	public static List<ParticlePlacement> addList;

	public static List<Vector3i> removeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int checkListNum = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Object[] waterFallSplashCubeEffect = null;

	public static float particleLimiter = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int particleCount = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float cleanUpTimer = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int currentCleanupIndex = 15;

	public WaterSplashCubes()
	{
		if (instance == null)
		{
			instance = this;
		}
		addList = new List<ParticlePlacement>();
		removeList = new List<Vector3i>();
		root = GameObject.Find("WaterSplashes");
		particleLimiter = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
	}

	public static long MakeKey(int x, int z)
	{
		return (long)((((ulong)z & 0xFFFFFFuL) << 24) | ((ulong)x & 0xFFFFFFuL));
	}

	public static object GetSyncRoot()
	{
		return ((ICollection)splashes.dict).SyncRoot;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject AddParticleEffect(Vector3i pos, BlockFace face, SplashType type)
	{
		if (waterFallSplashCubeEffect == null)
		{
			waterFallSplashCubeEffect = new Object[EnumUtils.Names<SplashType>().Count];
			waterFallSplashCubeEffect[0] = Resources.Load("prefabs/WaterFallSplashCube");
			waterFallSplashCubeEffect[1] = Resources.Load("prefabs/WaterFallSlopeParticles");
			waterFallSplashCubeEffect[2] = Resources.Load("prefabs/WaterFallAreaParticles");
			waterFallSplashCubeEffect[3] = Resources.Load("prefabs/WaterFallSplashCube");
		}
		GameObject gameObject = (GameObject)Object.Instantiate(waterFallSplashCubeEffect[(int)type]);
		particleCount++;
		switch (face)
		{
		case BlockFace.North:
			gameObject.transform.Rotate(new Vector3(0f, 0f, 0f));
			break;
		case BlockFace.South:
			gameObject.transform.Rotate(new Vector3(0f, 180f, 0f));
			break;
		case BlockFace.East:
			gameObject.transform.Rotate(new Vector3(0f, 90f, 0f));
			break;
		case BlockFace.West:
			gameObject.transform.Rotate(new Vector3(0f, -90f, 0f));
			break;
		case BlockFace.Top:
			gameObject.transform.Rotate(new Vector3(90f, 0f, 0f));
			break;
		case BlockFace.Bottom:
			gameObject.transform.Rotate(new Vector3(-90f, 0f, 0f));
			break;
		}
		gameObject.transform.position = new Vector3((float)pos.x + 0.5f, (float)pos.y + 0.5f, (float)pos.z + 0.5f) - Origin.position;
		gameObject.transform.parent = root.transform;
		if (particleLimiter < 1f && particleCount % Mathf.CeilToInt((1f - particleLimiter) * 6f) != 0)
		{
			gameObject.SetActive(value: false);
		}
		return gameObject;
	}

	public static void Update()
	{
		if (removeList == null || addList == null)
		{
			return;
		}
		lock (removeList)
		{
			for (int i = 0; i < removeList.Count; i++)
			{
				Vector3i vector3i = removeList[i];
				int x = vector3i.x;
				int y = vector3i.y;
				int z = vector3i.z;
				long key = MakeKey(x, z);
				lock (GetSyncRoot())
				{
					if (splashes.dict.ContainsKey(key))
					{
						DictionaryList<int, GameObject> dictionaryList = splashes.dict[key];
						if (dictionaryList.dict.ContainsKey(y))
						{
							Object.DestroyImmediate(dictionaryList.dict[y]);
							dictionaryList.Remove(y);
						}
					}
				}
			}
			removeList.Clear();
		}
		lock (addList)
		{
			for (int j = 0; j < addList.Count; j++)
			{
				ParticlePlacement particlePlacement = addList[j];
				int x2 = particlePlacement.pos.x;
				int y2 = particlePlacement.pos.y;
				int z2 = particlePlacement.pos.z;
				long key2 = MakeKey(x2, z2);
				lock (GetSyncRoot())
				{
					if (!splashes.dict.ContainsKey(key2))
					{
						DictionaryList<int, GameObject> dictionaryList2 = new DictionaryList<int, GameObject>();
						dictionaryList2.Add(y2, AddParticleEffect(particlePlacement.pos, particlePlacement.dir, particlePlacement.type));
						splashes.Add(key2, dictionaryList2);
						continue;
					}
					DictionaryList<int, GameObject> dictionaryList3 = splashes.dict[key2];
					if (!dictionaryList3.dict.ContainsKey(y2))
					{
						dictionaryList3.Add(y2, AddParticleEffect(particlePlacement.pos, particlePlacement.dir, particlePlacement.type));
					}
				}
			}
			addList.Clear();
		}
		CleanUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanUp()
	{
		if (!(Time.time > cleanUpTimer + 0.15f))
		{
			return;
		}
		cleanUpTimer = Time.time;
		if (root == null || root.transform == null || GameManager.Instance == null)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		bool flag = false;
		if (currentCleanupIndex >= root.transform.childCount)
		{
			currentCleanupIndex = root.transform.childCount;
			flag = true;
		}
		for (int i = 0; i < root.transform.childCount && i < currentCleanupIndex; i++)
		{
			Transform child = root.transform.GetChild(i);
			if (child != null)
			{
				Vector3i pos = new Vector3i((int)(child.position.x - 0.5f), (int)(child.position.y - 0.5f), (int)(child.position.z - 0.5f));
				if (!world.IsChunkAreaLoaded(pos.x, pos.y, pos.z))
				{
					RemoveSplashAt(pos.x, pos.y, pos.z);
				}
				else if (!world.IsWater(pos) || !world.IsAir((int)(child.position.x - 0.5f), (int)(child.position.y - 0.5f) + 1, (int)(child.position.z - 0.5f)))
				{
					RemoveSplashAt(pos.x, pos.y, pos.z);
				}
			}
		}
		currentCleanupIndex += 15;
		if (flag)
		{
			currentCleanupIndex = 0;
		}
	}

	public static void RemoveSplashAt(int _x, int _y, int _z)
	{
		if (removeList == null)
		{
			return;
		}
		lock (removeList)
		{
			removeList.Add(new Vector3i(_x, _y, _z));
		}
	}

	public static void AddSplashAt(int _x, int _y, int _z, BlockFace _dir, SplashType _type)
	{
		if (particleLimiter <= 0f || addList == null)
		{
			return;
		}
		lock (addList)
		{
			ParticlePlacement item = new ParticlePlacement(new Vector3i(_x, _y, _z), _dir, _type);
			addList.Add(item);
		}
	}

	public static void Clear()
	{
		lock (GetSyncRoot())
		{
			for (int i = 0; i < splashes.Count; i++)
			{
				DictionaryList<int, GameObject> dictionaryList = splashes.list[i];
				for (int j = 0; j < dictionaryList.list.Count; j++)
				{
					Object.DestroyImmediate(dictionaryList.list[j]);
				}
				dictionaryList.Clear();
			}
			splashes.Clear();
		}
	}
}

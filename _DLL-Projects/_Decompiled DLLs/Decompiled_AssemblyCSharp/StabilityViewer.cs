using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class StabilityViewer
{
	public class BuildStabilityBlocks
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i startPos;

		public BuildStabilityBlocks(Vector3i _startPos)
		{
			startPos = _startPos;
			GameManager.Instance.StartCoroutine(RegisterWhenDone());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator RegisterWhenDone()
		{
			List<Vector3i> positions = null;
			List<float> stabilityValues = null;
			yield return new WaitForSeconds(0.01f);
			Chunk chunk = (Chunk)GameManager.Instance.World.ChunkClusters[0].GetChunkFromWorldPos(startPos);
			if (chunk == null)
			{
				lock (boxes)
				{
					lock (buildingChunks)
					{
						buildingChunks.Remove(startPos);
						boxes[startPos] = null;
						yield break;
					}
				}
			}
			if (chunk.GetAvailable())
			{
				if (chunk.IsEmpty())
				{
					lock (boxes)
					{
						lock (buildingChunks)
						{
							buildingChunks.Remove(startPos);
							boxes[startPos] = null;
						}
					}
				}
				else
				{
					positions = new List<Vector3i>();
					stabilityValues = new List<float>();
					Vector3i vector3i = default(Vector3i);
					for (int i = 0; i < 16; i++)
					{
						for (int j = 0; j < 16; j++)
						{
							for (int k = 0; k < 16; k++)
							{
								vector3i.x = k;
								vector3i.y = j;
								vector3i.z = i;
								BlockValue block = GameManager.Instance.World.GetBlock(startPos + vector3i);
								if (!block.Block.shape.IsTerrain() && !block.isair)
								{
									float blockStability = StabilityCalculator.GetBlockStability(startPos + vector3i);
									positions.Add(startPos + vector3i);
									stabilityValues.Add(blockStability);
								}
							}
						}
					}
				}
				GameObject gameObject = null;
				if (positions != null)
				{
					gameObject = new GameObject();
					gameObject.AddComponent<MeshRenderer>().material = stabilityMtrl;
					MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
					meshFilter.mesh = new Mesh();
					Vector3[] array = new Vector3[8 * positions.Count];
					Color[] array2 = new Color[8 * positions.Count];
					int[] array3 = new int[36 * positions.Count];
					for (int l = 0; l < positions.Count; l++)
					{
						for (int m = 0; m < 8; m++)
						{
							array2[l * 8 + m] = Color.white * stabilityValues[l];
						}
						array[l * 8].x = (float)positions[l].x - 0.01f;
						array[l * 8].y = (float)positions[l].y - 0.01f;
						array[l * 8].z = (float)positions[l].z - 0.01f;
						array[l * 8 + 1].x = (float)positions[l].x - 0.01f;
						array[l * 8 + 1].y = (float)positions[l].y - 0.01f;
						array[l * 8 + 1].z = (float)positions[l].z + 1.01f;
						array[l * 8 + 2].x = (float)positions[l].x + 1.01f;
						array[l * 8 + 2].y = (float)positions[l].y - 0.01f;
						array[l * 8 + 2].z = (float)positions[l].z + 1.01f;
						array[l * 8 + 3].x = (float)positions[l].x + 1.01f;
						array[l * 8 + 3].y = (float)positions[l].y - 0.01f;
						array[l * 8 + 3].z = (float)positions[l].z - 0.01f;
						array[l * 8 + 4].x = (float)positions[l].x - 0.01f;
						array[l * 8 + 4].y = (float)positions[l].y + 1.01f;
						array[l * 8 + 4].z = (float)positions[l].z - 0.01f;
						array[l * 8 + 5].x = (float)positions[l].x - 0.01f;
						array[l * 8 + 5].y = (float)positions[l].y + 1.01f;
						array[l * 8 + 5].z = (float)positions[l].z + 1.01f;
						array[l * 8 + 6].x = (float)positions[l].x + 1.01f;
						array[l * 8 + 6].y = (float)positions[l].y + 1.01f;
						array[l * 8 + 6].z = (float)positions[l].z + 1.01f;
						array[l * 8 + 7].x = (float)positions[l].x + 1.01f;
						array[l * 8 + 7].y = (float)positions[l].y + 1.01f;
						array[l * 8 + 7].z = (float)positions[l].z - 0.01f;
						int num = 0;
						array3[l * 36 + num] = l * 8;
						num++;
						array3[l * 36 + num] = 3 + l * 8;
						num++;
						array3[l * 36 + num] = 2 + l * 8;
						num++;
						array3[l * 36 + num] = 2 + l * 8;
						num++;
						array3[l * 36 + num] = 1 + l * 8;
						num++;
						array3[l * 36 + num] = l * 8;
						num++;
						array3[l * 36 + num] = 4 + l * 8;
						num++;
						array3[l * 36 + num] = 5 + l * 8;
						num++;
						array3[l * 36 + num] = 6 + l * 8;
						num++;
						array3[l * 36 + num] = 6 + l * 8;
						num++;
						array3[l * 36 + num] = 7 + l * 8;
						num++;
						array3[l * 36 + num] = 4 + l * 8;
						num++;
						array3[l * 36 + num] = 4 + l * 8;
						num++;
						array3[l * 36 + num] = 7 + l * 8;
						num++;
						array3[l * 36 + num] = 3 + l * 8;
						num++;
						array3[l * 36 + num] = 3 + l * 8;
						num++;
						array3[l * 36 + num] = l * 8;
						num++;
						array3[l * 36 + num] = 4 + l * 8;
						num++;
						array3[l * 36 + num] = 6 + l * 8;
						num++;
						array3[l * 36 + num] = 5 + l * 8;
						num++;
						array3[l * 36 + num] = 1 + l * 8;
						num++;
						array3[l * 36 + num] = 1 + l * 8;
						num++;
						array3[l * 36 + num] = 2 + l * 8;
						num++;
						array3[l * 36 + num] = 6 + l * 8;
						num++;
						array3[l * 36 + num] = 7 + l * 8;
						num++;
						array3[l * 36 + num] = 6 + l * 8;
						num++;
						array3[l * 36 + num] = 2 + l * 8;
						num++;
						array3[l * 36 + num] = 2 + l * 8;
						num++;
						array3[l * 36 + num] = 3 + l * 8;
						num++;
						array3[l * 36 + num] = 7 + l * 8;
						num++;
						array3[l * 36 + num] = 5 + l * 8;
						num++;
						array3[l * 36 + num] = 4 + l * 8;
						num++;
						array3[l * 36 + num] = l * 8;
						num++;
						array3[l * 36 + num] = l * 8;
						num++;
						array3[l * 36 + num] = 1 + l * 8;
						num++;
						array3[l * 36 + num] = 5 + l * 8;
					}
					meshFilter.mesh.Clear(keepVertexLayout: false);
					meshFilter.mesh.vertices = array;
					meshFilter.mesh.SetIndices(array3, MeshTopology.Triangles, 0);
					meshFilter.mesh.colors = array2;
					if (StabilityViewBoxes != null)
					{
						gameObject.transform.parent = StabilityViewBoxes.transform;
					}
					else
					{
						Object.Destroy(gameObject);
					}
				}
				lock (boxes)
				{
					lock (buildingChunks)
					{
						boxes[startPos] = gameObject;
						buildingChunks.Remove(startPos);
						yield break;
					}
				}
			}
			lock (buildingChunks)
			{
				buildingChunks.Remove(startPos);
			}
		}
	}

	public static Dictionary<Vector3i, GameObject> boxes = new Dictionary<Vector3i, GameObject>();

	public static List<Vector3i> buildingChunks = new List<Vector3i>();

	public bool worldIsReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i startPos;

	public static List<Material> materials = new List<Material>();

	public static GameObject displayObject = null;

	public static GameObject StabilityViewBoxes = null;

	public static Material stabilityMtrl = null;

	public static int numMaterials = 7;

	public static bool bGatheringChunks = false;

	public static int TotalIterations = 0;

	public static int GetBlocks = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public Text debugText;

	public int searchSizeXZ = 5;

	public int searchSizeY = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i currSearch;

	[PublicizedFrom(EAccessModifier.Private)]
	public int asynCount = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float prevTotalTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateDisplayTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject textGo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera mainCam;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startedSearch;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool flipFlop;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CalculatedStability;

	public ChunkCluster ChunkCluster0
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!(GameManager.Instance == null))
			{
				if (GameManager.Instance.World != null)
				{
					return GameManager.Instance.World.ChunkClusters[0];
				}
				return null;
			}
			return null;
		}
	}

	public StabilityViewer()
	{
		displayObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		displayObject.transform.localScale = Vector3.one * 1.01f;
		Object.Destroy(displayObject.GetComponent<BoxCollider>());
		for (int i = 0; i < numMaterials; i++)
		{
			materials.Add(Resources.Load<Material>("Materials/Stability" + i));
		}
		stabilityMtrl = Resources.Load<Material>("Materials/Stability");
		StabilityViewBoxes = new GameObject();
		StabilityViewBoxes.name = "StabilityViewBoxes";
		if (debugText == null)
		{
			GameObject gameObject = Resources.Load<GameObject>("Prefabs/StabilityCanvas");
			if (gameObject != null)
			{
				textGo = Object.Instantiate(gameObject);
				if (textGo != null)
				{
					debugText = textGo.GetComponentInChildren<Text>();
					debugText.text = "Recalculating Stability.  Please Wait...";
				}
			}
		}
		startedSearch = false;
		flipFlop = false;
		bGatheringChunks = false;
		CalculatedStability = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecalcStability()
	{
		if (ChunkCluster0 == null)
		{
			return;
		}
		ReaderWriterLockSlim syncRoot = ChunkCluster0.GetSyncRoot();
		syncRoot.EnterWriteLock();
		StabilityInitializer stabilityInitializer = new StabilityInitializer(GameManager.Instance.World);
		MicroStopwatch microStopwatch = new MicroStopwatch();
		foreach (Chunk item in ChunkCluster0.GetChunkArray())
		{
			item.ResetStability();
		}
		syncRoot.ExitWriteLock();
		foreach (Chunk item2 in ChunkCluster0.GetChunkArray())
		{
			stabilityInitializer.DistributeStability(item2);
			item2.NeedsRegeneration = true;
		}
		Log.Out("#" + ChunkCluster0.GetChunkArray().Count + " chunks needed " + microStopwatch.ElapsedMilliseconds + "ms");
	}

	public static GameObject GetBlock(float stability)
	{
		GameObject gameObject = Object.Instantiate(displayObject);
		if (stability <= 0f)
		{
			gameObject.GetComponent<MeshRenderer>().material = materials[numMaterials - 1];
			return gameObject;
		}
		float num = 1f / (float)numMaterials;
		bool flag = false;
		for (int i = 0; i < numMaterials; i++)
		{
			if (stability >= 1f - num * (float)i)
			{
				gameObject.GetComponent<MeshRenderer>().material = materials[i];
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			gameObject.GetComponent<MeshRenderer>().material = materials[numMaterials - 1];
		}
		return gameObject;
	}

	public void OnDestroy()
	{
		Clear();
	}

	public void Clear()
	{
		TotalIterations = 0;
		buildingChunks.Clear();
		boxes.Clear();
		Object.Destroy(StabilityViewBoxes);
		StabilityViewBoxes = null;
		Object.Destroy(textGo);
		textGo = null;
		debugText = null;
		Object.Destroy(displayObject);
		displayObject = null;
		CalculatedStability = false;
	}

	public void StartSearch(int _asynCount = 100)
	{
		startedSearch = true;
		asynCount = _asynCount;
		bGatheringChunks = true;
		currSearch.x = -searchSizeXZ;
		currSearch.y = -searchSizeY;
		currSearch.z = -searchSizeXZ;
		totalTimer = Time.realtimeSinceStartup;
		prevTotalTimer = Time.realtimeSinceStartup;
		updateDisplayTimer = 0f;
	}

	public void Update()
	{
		if (Time.time > updateDisplayTimer + 3f)
		{
			updateDisplayTimer = Time.time;
			prevTotalTimer = totalTime;
			if (!CalculatedStability)
			{
				RecalcStability();
				CalculatedStability = true;
			}
		}
		if (!bGatheringChunks)
		{
			return;
		}
		totalTime = Time.realtimeSinceStartup - totalTimer;
		if (debugText != null)
		{
			debugText.text = "Chunks Finished: " + boxes.Count + " Time( " + prevTotalTimer.ToCultureInvariantString() + " : " + totalTime.ToCultureInvariantString() + " ) GetBlock(): " + GetBlocks;
		}
		if (buildingChunks.Count > asynCount)
		{
			return;
		}
		if (!worldIsReady && GameManager.Instance.World != null && Camera.main != null)
		{
			mainCam = Camera.main;
			worldIsReady = true;
		}
		if (GameManager.Instance.World == null)
		{
			worldIsReady = false;
		}
		if (!worldIsReady)
		{
			return;
		}
		Vector3i blockPos = default(Vector3i);
		blockPos.x = Mathf.FloorToInt(mainCam.transform.position.x);
		blockPos.y = Mathf.FloorToInt(mainCam.transform.position.y);
		blockPos.z = Mathf.FloorToInt(mainCam.transform.position.z);
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[0];
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(blockPos);
		if (chunk == null)
		{
			return;
		}
		startPos = chunk.GetWorldPos();
		startPos.y = blockPos.y - blockPos.y % 16;
		lock (boxes)
		{
			lock (buildingChunks)
			{
				if (!boxes.ContainsKey(startPos) && !buildingChunks.Contains(startPos))
				{
					buildingChunks.Add(startPos);
					new BuildStabilityBlocks(startPos);
				}
			}
		}
		Vector3i zero = Vector3i.zero;
		lock (boxes)
		{
			lock (buildingChunks)
			{
				zero.x = currSearch.x * 16;
				zero.y = currSearch.y * 16;
				zero.z = currSearch.z * 16;
				Vector3i vector3i = startPos + zero;
				if (!boxes.ContainsKey(vector3i) && !buildingChunks.Contains(vector3i))
				{
					Chunk chunk2 = (Chunk)chunkCluster.GetChunkFromWorldPos(vector3i);
					if (chunk2 == null)
					{
						boxes.Add(vector3i, null);
					}
					else if (chunk2.GetAvailable())
					{
						if (chunk2.IsEmpty())
						{
							boxes.Add(vector3i, null);
						}
						else
						{
							buildingChunks.Add(vector3i);
							new BuildStabilityBlocks(vector3i);
						}
					}
					else
					{
						buildingChunks.Add(vector3i);
						new BuildStabilityBlocks(vector3i);
					}
				}
			}
		}
		currSearch.x++;
		if (currSearch.x >= searchSizeXZ)
		{
			currSearch.x = -searchSizeXZ;
			currSearch.z++;
		}
		if (currSearch.z >= searchSizeXZ)
		{
			currSearch.z = -searchSizeXZ;
			currSearch.y++;
		}
		if (currSearch.y >= searchSizeY)
		{
			currSearch.y = -searchSizeY;
			bGatheringChunks = false;
			Log.Out("Stability DONE");
		}
	}
}

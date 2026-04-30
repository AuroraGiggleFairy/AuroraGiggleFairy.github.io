using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabLODManager
{
	public class PrefabGameObject
	{
		public string meshPath;

		public PrefabInstance prefabInstance;

		public GameObject go;

		public bool isAllShown;
	}

	public class MeshPrefabSet
	{
		public bool isLoading;

		public SimpleMeshInfo meshInfo;

		public List<int> prefabIDs;

		public MeshPrefabSet()
		{
			prefabIDs = new List<int>();
		}
	}

	public const int cPrefabYPosition = 4;

	public const int cLodPoiDistance = 1000;

	public static int lodPoiDistance = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material sharedMat;

	public Dictionary<int, PrefabInstance> prefabsAroundFar = new Dictionary<int, PrefabInstance>();

	public Dictionary<int, PrefabInstance> prefabsAroundNear = new Dictionary<int, PrefabInstance>();

	public Dictionary<int, PrefabGameObject> displayedPrefabs = new Dictionary<int, PrefabGameObject>();

	public Dictionary<string, MeshPrefabSet> meshPathToData = new Dictionary<string, MeshPrefabSet>();

	public Transform parentTransform;

	public float lastTime;

	public float lastDisplayUpdate;

	public MicroStopwatch stopWatch = new MicroStopwatch();

	public void TriggerUpdate()
	{
		lastTime = 0f;
		lastDisplayUpdate = 0f;
	}

	public void FrameUpdate()
	{
		try
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			float time = Time.time;
			if (time - lastDisplayUpdate < 1f)
			{
				return;
			}
			lastDisplayUpdate = time;
			World world = GameManager.Instance.World;
			if (world.ChunkClusters[0].IsFixedSize)
			{
				return;
			}
			List<EntityPlayerLocal> localPlayers = world.GetLocalPlayers();
			if (localPlayers.Count == 0)
			{
				return;
			}
			EntityPlayerLocal entityPlayerLocal = localPlayers[0];
			if (entityPlayerLocal == null)
			{
				return;
			}
			UpdateDisplay(entityPlayerLocal);
			if (time - lastTime < 1f)
			{
				return;
			}
			lastTime = time;
			DynamicPrefabDecorator dynamicPrefabDecorator = world.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator == null)
			{
				return;
			}
			for (int i = 0; i < world.Players.list.Count; i++)
			{
				prefabsAroundFar.Clear();
				prefabsAroundNear.Clear();
				EntityPlayer entityPlayer = world.Players.list[i];
				if (!(entityPlayer == null))
				{
					Vector3 position = entityPlayer.position;
					int num = ((entityPlayer.ChunkObserver != null) ? entityPlayer.ChunkObserver.viewDim : GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance));
					num = (num - 1) * 16;
					if (!entityPlayer.isEntityRemote)
					{
						dynamicPrefabDecorator.GetPrefabsAround(position, num, lodPoiDistance, prefabsAroundFar, prefabsAroundNear);
						UpdatePrefabsAround(prefabsAroundFar, prefabsAroundNear);
					}
					else if (!((entityPlayer.position - entityPlayerLocal.position).sqrMagnitude > (float)(2 * num * (2 * num))))
					{
						dynamicPrefabDecorator.GetPrefabsAround(position, num, lodPoiDistance, prefabsAroundFar, prefabsAroundNear);
						entityPlayer.SetPrefabsAroundNear(prefabsAroundNear);
					}
				}
			}
		}
		finally
		{
		}
	}

	public PrefabGameObject GetInstance(int id)
	{
		displayedPrefabs.TryGetValue(id, out var value);
		return value;
	}

	public void UpdatePrefabsAround(Dictionary<int, PrefabInstance> _prefabsAroundFar, Dictionary<int, PrefabInstance> _prefabsAroundNear)
	{
		try
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
			if (localPlayers.Count == 0)
			{
				return;
			}
			localPlayers[0].SetPrefabsAroundNear(_prefabsAroundNear);
			foreach (KeyValuePair<int, PrefabInstance> item in _prefabsAroundFar)
			{
				PrefabInstance value = item.Value;
				if (displayedPrefabs.ContainsKey(value.id))
				{
					continue;
				}
				PathAbstractions.AbstractedLocation imposterLocation = value.GetImposterLocation();
				if (imposterLocation.Type != PathAbstractions.EAbstractedLocationType.None)
				{
					string fullPath = imposterLocation.FullPath;
					PrefabGameObject prefabGameObject = new PrefabGameObject();
					prefabGameObject.prefabInstance = value;
					prefabGameObject.meshPath = imposterLocation.FullPath;
					displayedPrefabs.Add(value.id, prefabGameObject);
					if (!meshPathToData.ContainsKey(fullPath))
					{
						meshPathToData.Add(fullPath, new MeshPrefabSet());
					}
					meshPathToData[fullPath].prefabIDs.Add(value.id);
				}
			}
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab in displayedPrefabs)
			{
				int key = displayedPrefab.Key;
				if (!_prefabsAroundFar.ContainsKey(key))
				{
					list.Add(key);
				}
			}
			if (list.Count > 0)
			{
				removePrefabs(list);
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removePrefabs(List<int> toRemove)
	{
		try
		{
			foreach (int item in toRemove)
			{
				string meshPath = displayedPrefabs[item].meshPath;
				GameObject go = displayedPrefabs[item].go;
				if (!go)
				{
					continue;
				}
				MeshPrefabSet meshPrefabSet = meshPathToData[meshPath];
				meshPrefabSet.prefabIDs.Remove(item);
				if (meshPrefabSet.prefabIDs.Count == 0)
				{
					Mesh[] meshes = meshPrefabSet.meshInfo.meshes;
					for (int i = 0; i < meshes.Length; i++)
					{
						Object.Destroy(meshes[i]);
					}
					meshPathToData.Remove(meshPath);
				}
				displayedPrefabs.Remove(item);
				Object.Destroy(go);
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildGameObjectFromMeshInfo(SimpleMeshInfo _meshInfo, PrefabGameObject _pgo)
	{
		try
		{
			_pgo.go = SimpleMeshFile.CreateUnityObjects(_meshInfo);
			GameObject go = _pgo.go;
			PrefabInstance prefabInstance = _pgo.prefabInstance;
			if (!go)
			{
				Log.Error("Loading LOD mesh for Prefab " + _pgo.prefabInstance.name + " failed.");
				return;
			}
			go.name = prefabInstance.location.Name;
			Transform transform = go.transform;
			transform.SetParent(parentTransform, worldPositionStays: false);
			Vector3 vector = prefabInstance.boundingBoxPosition.ToVector3();
			vector += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, -4f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
			Vector3 vector2 = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			switch (prefabInstance.rotation)
			{
			case 1:
				vector2 = new Vector3(0.5f, 0f, -0.5f);
				rotation = Quaternion.Euler(0f, 270f, 0f);
				break;
			case 2:
				vector2 = new Vector3(0.5f, 0f, 0.5f);
				rotation = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 3:
				vector2 = new Vector3(-0.5f, 0f, 0.5f);
				rotation = Quaternion.Euler(0f, 90f, 0f);
				break;
			case 0:
				vector2 = new Vector3(-0.5f, 0f, -0.5f);
				break;
			}
			if (Utils.FastAbs(vector.x - (float)(int)vector.x) > 0.001f)
			{
				vector.x += vector2.x;
			}
			if (Utils.FastAbs(vector.z - (float)(int)vector.z) > 0.001f)
			{
				vector.z += vector2.z;
			}
			float num = ((!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) ? prefabInstance.yOffsetOfPrefab : prefabInstance.prefab.distantPOIYOffset);
			vector.y += num;
			transform.SetPositionAndRotation(vector - Origin.position, rotation);
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.SetActive(value: false);
			}
			if (OcclusionManager.Instance.cullPrefabs)
			{
				Occludee.Add(go);
			}
		}
		finally
		{
		}
	}

	public void meshLoadedCallback(SimpleMeshInfo meshInfo, object userCallbackData)
	{
		try
		{
			string text = (string)userCallbackData;
			MeshPrefabSet meshPrefabSet = meshPathToData[text];
			if (meshPrefabSet.meshInfo != null)
			{
				Log.Error("Meshes have already been provided for path " + text);
				return;
			}
			meshPrefabSet.meshInfo = meshInfo;
			meshPrefabSet.isLoading = false;
			PrefabGameObject prefabGameObject = null;
			for (int i = 0; i < meshPrefabSet.prefabIDs.Count; i++)
			{
				prefabGameObject = displayedPrefabs[meshPrefabSet.prefabIDs[i]];
				BuildGameObjectFromMeshInfo(meshInfo, prefabGameObject);
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDisplay(EntityPlayerLocal _localPlayer)
	{
		try
		{
			stopWatch.ResetAndRestart();
			if (!parentTransform)
			{
				parentTransform = new GameObject("PrefabsLOD").transform;
				Origin.Add(parentTransform, 0);
				sharedMat = MeshDescription.GetOpaqueMaterial();
			}
			bool bTextureArray = MeshDescription.meshes[0].bTextureArray;
			foreach (KeyValuePair<string, MeshPrefabSet> meshPathToDatum in meshPathToData)
			{
				string key = meshPathToDatum.Key;
				MeshPrefabSet value = meshPathToDatum.Value;
				if (value.meshInfo != null)
				{
					foreach (int prefabID in value.prefabIDs)
					{
						PrefabGameObject prefabGameObject = displayedPrefabs[prefabID];
						if (prefabGameObject.go == null)
						{
							BuildGameObjectFromMeshInfo(value.meshInfo, prefabGameObject);
						}
					}
				}
				else if (!value.isLoading)
				{
					value.isLoading = true;
					Material mat = sharedMat;
					SimpleMeshFile.GameObjectMeshesReadCallback asyncCallback = meshLoadedCallback;
					SimpleMeshFile.ReadMesh(key, 0f, mat, bTextureArray, _markMeshesNoLongerReadable: true, key, asyncCallback);
					if (stopWatch.ElapsedMicroseconds > 900)
					{
						lastDisplayUpdate = 0f;
						return;
					}
				}
			}
			Vector3 position = _localPlayer.position;
			int num = (int)position.x;
			int num2 = num;
			int num3 = (int)position.z;
			int num4 = num3;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
			for (int i = 0; i < chunkArrayCopySync.Count; i++)
			{
				Chunk chunk = chunkArrayCopySync[i];
				if (chunk.IsDisplayed && chunk.IsCollisionMeshGenerated && !((chunk.GetAABB().center - position).sqrMagnitude > 36864f))
				{
					if (chunk.worldPosIMin.x < num)
					{
						num = chunk.worldPosIMin.x;
						num5 = 1;
					}
					else if (chunk.worldPosIMin.x == num)
					{
						num5++;
					}
					if (chunk.worldPosIMax.x > num2)
					{
						num2 = chunk.worldPosIMax.x;
						num6 = 1;
					}
					else if (chunk.worldPosIMax.x == num2)
					{
						num6++;
					}
					if (chunk.worldPosIMin.z < num3)
					{
						num3 = chunk.worldPosIMin.z;
						num7 = 1;
					}
					else if (chunk.worldPosIMin.z == num3)
					{
						num7++;
					}
					if (chunk.worldPosIMax.z > num4)
					{
						num4 = chunk.worldPosIMax.z;
						num8 = 1;
					}
					else if (chunk.worldPosIMax.z == num4)
					{
						num8++;
					}
				}
			}
			int num9 = num2 - num + 1;
			int num10 = num4 - num3 + 1;
			if (num5 * 16 != num10)
			{
				num += 16;
			}
			if (num6 * 16 != num10)
			{
				num2 -= 16;
			}
			if (num7 * 16 != num9)
			{
				num3 += 16;
			}
			if (num8 * 16 != num9)
			{
				num4 -= 16;
			}
			Vector3 vector = new Vector3(num, 0f, num3);
			Vector3 vector2 = new Vector3(num2 + 1, 256f, num4 + 1);
			Bounds bounds = new Bounds((vector2 + vector) * 0.5f, vector2 - vector);
			foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab in displayedPrefabs)
			{
				PrefabGameObject value2 = displayedPrefab.Value;
				if (!value2.go)
				{
					continue;
				}
				PrefabInstance prefabInstance = value2.prefabInstance;
				Vector3 point = prefabInstance.boundingBoxPosition;
				Vector3 point2 = prefabInstance.boundingBoxPosition + prefabInstance.boundingBoxSize;
				if ((bounds.Contains(point) ? 1 : 0) + (bounds.Contains(point2) ? 1 : 0) + (bounds.Contains(new Vector3(point2.x, point.y, point.z)) ? 1 : 0) + (bounds.Contains(new Vector3(point.x, point.y, point2.z)) ? 1 : 0) == 0)
				{
					if (!value2.isAllShown)
					{
						value2.isAllShown = true;
						Transform transform = value2.go.transform;
						int childCount = transform.childCount;
						for (int j = 0; j < childCount; j++)
						{
							transform.GetChild(j).gameObject.SetActive(value: true);
						}
					}
					continue;
				}
				value2.isAllShown = false;
				Transform transform2 = value2.go.transform;
				int childCount2 = transform2.childCount;
				for (int k = 0; k < childCount2; k++)
				{
					Transform child = transform2.GetChild(k);
					string name = child.name;
					StringParsers.SeparatorPositions separatorPositions = StringParsers.GetSeparatorPositions(name, ',', 1);
					if (separatorPositions.TotalFound != 1)
					{
						break;
					}
					int num11 = StringParsers.ParseSInt32(name, 0, separatorPositions.Sep1 - 1);
					int num12 = StringParsers.ParseSInt32(name, separatorPositions.Sep1 + 1);
					float num13 = 1f;
					float num14 = 1f;
					switch (prefabInstance.rotation)
					{
					case 3:
					{
						int num16 = num11;
						num11 = num12;
						num12 = -num16;
						num14 = -1f;
						break;
					}
					case 2:
						num11 = -num11;
						num12 = -num12;
						num13 = -1f;
						num14 = -1f;
						break;
					case 1:
					{
						int num15 = num11;
						num11 = -num12;
						num12 = num15;
						num13 = -1f;
						break;
					}
					}
					num11 *= 32;
					num12 *= 32;
					num11 += prefabInstance.boundingBoxPosition.x;
					num12 += prefabInstance.boundingBoxPosition.z;
					num11 += prefabInstance.boundingBoxSize.x / 2;
					num12 += prefabInstance.boundingBoxSize.z / 2;
					Vector3 point3 = new Vector3(num11, prefabInstance.boundingBoxPosition.y, num12);
					Vector3 point4 = new Vector3((float)num11 + num13 * 32f, prefabInstance.boundingBoxPosition.y, (float)num12 + num14 * 32f);
					if (bounds.Contains(point3) && bounds.Contains(point4))
					{
						if (child.gameObject.activeSelf)
						{
							child.gameObject.SetActive(value: false);
						}
					}
					else if (!child.gameObject.activeSelf)
					{
						child.gameObject.SetActive(value: true);
					}
				}
			}
		}
		finally
		{
		}
	}

	public void Cleanup()
	{
		if (parentTransform != null)
		{
			Origin.Remove(parentTransform);
			Object.Destroy(parentTransform.gameObject);
			parentTransform = null;
		}
		removePrefabs(displayedPrefabs.Keys.ToList());
	}

	public void SetPOIDistance(int _distance)
	{
		lodPoiDistance = _distance;
		lastDisplayUpdate = 0f;
		lastTime = 0f;
	}

	public void UpdateMaterials()
	{
		try
		{
			Object.Destroy(sharedMat);
			sharedMat = MeshDescription.GetOpaqueMaterial();
			foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab in displayedPrefabs)
			{
				PrefabGameObject value = displayedPrefab.Value;
				if ((bool)value.go)
				{
					MeshDescription.GetOpaqueMaterial();
					Renderer[] componentsInChildren = value.go.GetComponentsInChildren<Renderer>(includeInactive: true);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].material = sharedMat;
					}
				}
			}
		}
		finally
		{
		}
	}
}
